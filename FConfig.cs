using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class FConfig {

    // Register events
    public static EventConstructor.EventRegistry Events = 
        new EventConstructor.EventRegistry();

    // Property constructors
    #region Property constructors

    /// <summary>
    /// Configuration dictionary
    /// </summary>
    private static Dictionary<string, object> _configuration = new Dictionary<string, object> { };

    /// <summary>
    /// Gets or sets the configuration data
    /// </summary>
    /// <returns>An array with configuration data</returns>
    public static Dictionary<string, object> Configuration {
        get => _configuration;
        set => _configuration = value;
    }

    /// <summary>
    /// Configuration path
    /// </summary>
    private static string _configPath = string.Empty;

    /// <summary>
    /// Gets or sets the path to the configuration file
    /// </summary>
    /// <returns>The path to the configuration file</returns>
    public static string ConfigurationPath {
        get => _configPath;
        set => _configPath = value;
    }

    /// <summary>
    /// Secret key
    /// </summary>
    private static string _secretKey = string.Empty;

    /// <summary>
    /// Sets the secret key used for encryption and decryption
    /// </summary>
    public static string SecretKey {
        private get => _secretKey;
        set => _secretKey = value;
    }

    /// <summary>
    /// Encryption
    /// </summary>
    private static bool _useEncryption = true;

    /// <summary>
    /// Gets or sets a value indicating whether to use encryption
    /// </summary>
    /// <returns>True if encryption should be used, false otherwise</returns>
    public static bool Encrypt {
        private get => _useEncryption;
        set => _useEncryption = value;
    }

    #endregion

    /// <summary>
    /// Initializes the configuration object. Required to be done before every use!
    /// </summary>
    /// <param name="configurationPath">Path to the configuration file.</param>
    /// <param name="secretKey">Optional encryption key.</param>
    /// <param name="useEncryption">Should the configuration be encrypted before saving.</param>
    /// <remarks>
    public static void Construct(
        string configurationPath,
        string secretKey = null,
        bool useEncryption = true
    )
    {

        // Verify that the secret key has the correct length
        if (secretKey.Length != 32) {
            throw new ArgumentException($"Secret key must be 32 characters in length. Supplied was {secretKey.Length} characters in length.", "secretKey");
        }

        // Get the full path to the configuration file
        string configFilePath = Path.Combine(configurationPath, ".fdata");

        // If the configuration file does not exist, create an empty file
        if (!File.Exists(configFilePath)) {
            File.WriteAllBytes(configFilePath, new byte[0]);
        }

        // Set the global variables
        ConfigurationPath = configFilePath;
        Encrypt = useEncryption;

        if (!string.IsNullOrEmpty(secretKey)) {
            SecretKey = secretKey;
        }

        // Read the raw configuration data from the file
        byte[] rawConfig;
        using (FileStream stream = new FileStream(configFilePath, FileMode.Open, FileAccess.Read)) {
            rawConfig = new byte[stream.Length];
            stream.Read(rawConfig, 0, (int)stream.Length);
        }

        // If the file is empty, create an empty configuration dictionary
        if (rawConfig.Length == 0) {
            Configuration = new Dictionary<string, object> { };
        } else {
            // If encryption is enabled, decrypt the raw data
            if (Encrypt) {
                rawConfig = ByteEncryption.Decrypt(rawConfig, secretKey);
            }

            // Deserialize the raw data into a dictionary object
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream(rawConfig)) {
                Configuration = (Dictionary<string, object>)formatter.Deserialize(stream);
            }
        }

        // Raise the configuration loaded event
        Events.OnConfigurationLoaded(Configuration);

    }

    /// <summary>
    /// Gets the configuration value with the specified identifier
    /// </summary>
    /// <typeparam name="T">The type of the configuration value</typeparam>
    /// <param name="identifier">The identifier of the configuration value</param>
    /// <param name="fallback">The fallback value to use if the configuration value is not found or cannot be converted</param>
    /// <returns>The configuration value with the specified identifier, or the fallback value if not found or cannot be converted</returns>
    public static T Get<T>(string identifier, T fallback = default) {
        // Get the configuration dictionary
        Dictionary<string, object> configObj = Configuration;

        // Try to fetch the value from the dictionary
        object fetchedValue = null;
        if (configObj.ContainsKey(identifier)) {
            fetchedValue = configObj[identifier];
        }

        // If the value is not found, set the fallback value and return it
        if (fetchedValue == null) {
            configObj[identifier] = fallback;
            return fallback;
        }

        // If the value is already of the correct type, return it directly
        if (fetchedValue is T t) {
            return t;
        }

        // Otherwise, try to convert the value to the correct type
        return (T)Convert.ChangeType(fetchedValue, typeof(T));
    }

    /// <summary>
    /// Overwrites the specified configuration value in the dictionary and flushes the configuration bytes to a file.
    /// </summary>
    /// <param name="identifier">Configuration identifier value.</param>
    /// <param name="newValue">New value object.</param>
    public static void Set<T>(string identifier, T newValue) {

        Dictionary<string, object> configObj = Configuration;

        configObj[identifier] = newValue;

        // Create a BinaryFormatter to serialize the dictionary
        BinaryFormatter formatter = new BinaryFormatter();

        // Serialize the dictionary to a byte array
        byte[] configBytes;
        using (MemoryStream stream = new MemoryStream()) {
            formatter.Serialize(stream, configObj);
            configBytes = stream.ToArray();
        };

        // Encrypt the bytes
        if (Encrypt == true) configBytes = ByteEncryption.Encrypt(configBytes, SecretKey);

        // Write bytes to file
        using (FileStream stream = new FileStream(ConfigurationPath, FileMode.Create, FileAccess.Write)) {
            stream.Write(configBytes, 0, configBytes.Length);
        };

    }

    /// <summary>
    /// Removes the configuration value with the specified identifier
    /// </summary>
    /// <param name="identifier">The identifier of the configuration value to remove</param>
    public static void Remove(string identifier) {
        // Get the configuration dictionary
        Dictionary<string, object> configObj = Configuration;

        // Remove the specified value from the dictionary
        Configuration.Remove(identifier);

        // Serialize the dictionary to a byte array
        byte[] configBytes;
        BinaryFormatter formatter = new BinaryFormatter();
        using (MemoryStream stream = new MemoryStream()) {
            formatter.Serialize(stream, configObj);
            configBytes = stream.ToArray();
        }

        // If encryption is enabled, encrypt the byte array
        if (Encrypt) {
            configBytes = ByteEncryption.Encrypt(configBytes, SecretKey);
        }

        // Write the byte array to the configuration file
        using (FileStream stream = new FileStream(ConfigurationPath, FileMode.Create, FileAccess.Write)) {
            stream.Write(configBytes, 0, configBytes.Length);
        }
    }

}
