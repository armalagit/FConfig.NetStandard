using EventConstructor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class FConfig
{

    // Register events
    public static EventRegistry Events = new EventRegistry();

    // Property constructors
    #region Property constructors

    // Configuration
    private static Dictionary<string, object> _configuration = new Dictionary<string, object> { };
    public static Dictionary<string, object> Configuration
    {
        get => _configuration;
        set => _configuration = value;
    }

    // Configuration path
    private static string _configPath = string.Empty;
    public static string ConfigurationPath
    {
        get => _configPath;
        set => _configPath = value;
    }

    // Secret key
    private static string _secretKey = string.Empty;
    public static string SecretKey
    {
        private get => _secretKey;
        set => _secretKey = value;
    }

    // Encryption
    private static bool _useEncryption = true;
    public static bool Encrypt
    {
        private get => _useEncryption;
        set => _useEncryption = value;
    }

    #endregion

    // Initialize a new configuration object
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

        if (secretKey.Length != 32)
            throw new ArgumentException($"Secret key must be 32 characters in length. Supplied was {secretKey.Length} characters in length.", "secretKey");

        // Append file name
        configurationPath = Path.Combine(configurationPath, ".fdata");

        // Verify if configuration file exists in specified path
        if (File.Exists(configurationPath) == false)
            File.WriteAllBytes(configurationPath, new byte[0]);

        // Set global
        ConfigurationPath = configurationPath;
        Encrypt = useEncryption;

        if (string.IsNullOrEmpty(secretKey) == false)
            SecretKey = secretKey;

        // Read text contents
        byte[] rawConfig = new byte[0];
        using (FileStream stream = new FileStream(configurationPath, FileMode.Open, FileAccess.Read))
        {

            rawConfig = new byte[stream.Length];
            stream.Read(rawConfig, 0, (int)stream.Length);

        };

        // Is there bytes to it?
        if (rawConfig.Length == 0)
        {

            Configuration = new Dictionary<string, object> { };

        }
        else
        {

            // Define the byte array containing the serialized dictionary
            if (Encrypt == true) rawConfig = FByteEncryption.Decrypt(rawConfig, secretKey);

            // Create a BinaryFormatter to deserialize the byte array
            BinaryFormatter formatter = new BinaryFormatter();

            // Deserialize the byte array to a Dictionary<string, object> object
            using (MemoryStream stream = new MemoryStream(rawConfig))
            {
                Configuration = (Dictionary<string, object>)formatter.Deserialize(stream);
            };

        };

        // Raise the configuration loaded event
        Events.OnConfigurationLoaded(Configuration);

    }

    // Initialize a new configuration object
    /// <summary>
    /// Reads the specified configuration value from the configuration object read from the file into process memory.
    /// </summary>
    /// <param name="identifier">Configuration identifier value.</param>
    /// <param name="fallback">Optional object value to use as a fallback return.</param>
    /// <returns>
    /// Return an object from configuration dictionary.
    /// </returns>
    public static T Get<T>(string identifier, T fallback = default)
    {

        Dictionary<string, object> configObj = Configuration;

        object fetchedValue = null;
        if (configObj.ContainsKey(identifier) == true)
            fetchedValue = configObj[identifier];

        // Return and set optional value if value unspecified
        if (fetchedValue == null)
        {

            // Set optional value as default
            // We do not save it in here because we hope something else does it later on
            configObj[identifier] = fallback;

            // Return optional value
            return fallback;

        };

        if (fetchedValue is T t)
        {

            // Value is already of the correct type, so return it directly
            return t;

        }
        else
        {

            // Value is not of the correct type, so try to convert it
            return (T)Convert.ChangeType(fetchedValue, typeof(T));

        };

    }

    // Initialize a new configuration object
    /// <summary>
    /// Overwrites the specified configuration value in the dictionary and flushes the configuration bytes to a file.
    /// </summary>
    /// <param name="identifier">Configuration identifier value.</param>
    /// <param name="newValue">New value object.</param>
    public static void Set<T>(string identifier, T newValue = null)
    {

        Dictionary<string, object> configObj = Configuration;

        configObj[identifier] = newValue;

        // Remove the value instead?
        if (newValue == null) { configObj.Remove(identifier); };

        // Create a BinaryFormatter to serialize the dictionary
        BinaryFormatter formatter = new BinaryFormatter();

        // Serialize the dictionary to a byte array
        byte[] configBytes;
        using (MemoryStream stream = new MemoryStream())
        {
            formatter.Serialize(stream, configObj);
            configBytes = stream.ToArray();
        };

        // Encrypt the bytes
        if (Encrypt == true) configBytes = FByteEncryption.Encrypt(configBytes, SecretKey);

        // Write bytes to file
        using (FileStream stream = new FileStream(ConfigurationPath, FileMode.Create, FileAccess.Write))
        {
            stream.Write(configBytes, 0, configBytes.Length);
        };

    }

}
