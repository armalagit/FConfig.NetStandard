using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

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

        if (!string.IsNullOrWhiteSpace(secretKey)) {
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

    /// <summary>
    /// Writes the unencrypted configuration file to specified directory
    /// </summary>
    /// <param name="outputDirectory">The directory where the configuration file will be written</param>
    public static void WriteToFile(string outputDirectory) {
        // Construct the output path using the output directory and the current date and time
        string outputPath = Path.Combine(outputDirectory, $"fconfig-{DateTime.UtcNow:s}.txt");

        // Use a StringBuilder to construct the string to be written to the file
        StringBuilder sb = new StringBuilder();

        // Iterate through the key-value pairs in the Configuration dictionary
        foreach (KeyValuePair<string, object> configItem in Configuration) {
            // Append the key and value to the StringBuilder in the key:value format, followed by a newline
            sb.AppendLine($"{configItem.Key}:{configItem.Value}");
        }

        // Write the contents of the StringBuilder to the file
        File.WriteAllText(outputPath, sb.ToString());
    }

    /// <summary>
    /// Reads the configuration values from a file and stores them in a dictionary
    /// </summary>
    /// <param name="inputPath">The path to the input file</param>
    /// <param name="backup">Make a backup of current instance</param>
    public static void ReadFromFile(string inputPath, bool backup = true) {
        // Check if the input file exists
        if (!File.Exists(inputPath)) {
            throw new FileNotFoundException("The input file was not found");
        }
        // Read all lines from the input file
        string[] inputFileContents = File.ReadAllLines(inputPath);

        // Remove empty lines from the inputFileContents array
        inputFileContents = inputFileContents.Where(x => !string.IsNullOrEmpty(x)).ToArray();

        // Create a new dictionary to store the configuration values
        Dictionary<string, object> inputDictionary = new Dictionary<string, object> { };

        // Iterate through the lines in the input file
        foreach (string inputLine in inputFileContents) {
            // Split the line at the colon to separate the key and value
            (string keyName, string keyValue) = SplitKeyValue(inputLine);

            // Add the key-value pair to the dictionary
            inputDictionary.Add(keyName, keyValue);
        }

        // Check if the number of items in the dictionary matches the number of lines in the input file
        if (inputDictionary.Count == inputFileContents.Length) {
            // Make a backup of the file
            if (backup) {
                string backupPath = Path.Combine(Path.GetDirectoryName(inputPath), $"{DateTime.UtcNow:s}.fdata");
                File.Copy(ConfigurationPath, ConfigurationPath, true);
            }

            // If the number of items in the dictionary matches the number of lines in the input file, set the Configuration dictionary to the input dictionary
            _configuration = inputDictionary;
        } else {
            throw new FileLoadException("The input length does not match the constructed length");
        }

    }

    private static (string, string) SplitKeyValue(string inputLine) {
        int colonIndex = inputLine.IndexOf(':');
        if (colonIndex == -1) {
            throw new FileLoadException("The input file has an invalid format");
        }
        string keyName = inputLine.Substring(0, colonIndex).Trim();
        string keyValue = inputLine.Substring(colonIndex + 1).Trim();
        return (keyName, keyValue);
    }

}
