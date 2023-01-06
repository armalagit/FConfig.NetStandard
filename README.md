# FConfig 

FConfig is a configuration management library for .NET applications. It provides an easy way to store and retrieve configuration values in a file, with the option to use AES encryption for added security.
## Installation

### PackageManager
```
NuGet\Install-Package Armala.FConfig
```

### .NET CLI
```
dotnet add package Armala.FConfig
```
## Dependencies
FConfig depends on the following libraries:
- System.Security.Cryptography.Algorithms (≥ 4.3.0)
These dependencies will be automatically installed when you install FConfig.

## ConfigManager
This is a C# class for managing a configuration file using the AES algorithm for optional encryption.

### Methods
#### `Construct(string configurationPath, string secretKey = null, bool useEncryption = true)`
Constructs a new instance of the ConfigManager class. 

The configuration path, secret key, and use of encryption can be specified. If the secret key is not provided or is not 32 characters in length, an ArgumentException will be thrown. 

If the configuration file at the specified path does not exist, an empty file will be created. The global variables ConfigurationPath, Encrypt, and SecretKey will be set, and the Configuration dictionary will be populated with the data from the configuration file. If the file is empty, the Configuration dictionary will be initialized as an empty dictionary. If encryption is enabled, the data will be decrypted before being deserialized into the Configuration dictionary. The OnConfigurationLoaded event will be raised after the configuration has been loaded.

#### `Get<T>(string identifier, T fallback = default)`
Gets the configuration value with the specified identifier. If the value is not found or cannot be converted to the specified type, the fallback value will be returned and set as the value for the specified identifier.

#### `Set<T>(string identifier, T newValue)`
Overwrites the specified configuration value in the Configuration dictionary and flushes the configuration bytes to a file.

#### `Remove(string identifier)`
Removes the configuration value with the specified identifier from the Configuration dictionary and saves the updated configuration to the configuration file.

### Properties
#### Configuration
A `Dictionary<string, object>` that contains the configuration data.

#### ConfigurationPath
A string containing the full path to the configuration file.

#### Encrypt
A bool indicating whether the configuration file is encrypted.

#### SecretKey
A string containing the secret key used for encryption.

### Events
#### OnConfigurationLoaded(Dictionary<string, object> configuration)
This event is raised after the configuration has been loaded. The configuration parameter contains the Configuration dictionary.

### Examples
```c#
FConfig.Construct("C:\\my-config-folder", "my-secret-key", true);

// Get the value of the "database-password" configuration key
string password = FConfig.Get<string>("database-password", "fallback value");

// Set the value of the "api-key" configuration key
FConfig.Set("api-key", "abc123");

// Remove the "api-key" configuration element
FConfig.Remove("api-key");
```

## ByteEncryption
This is a C# class for encrypting and decrypting byte arrays using the AES algorithm.

### Methods
#### `Encrypt(byte[] rawBytes, string secretKey)`
Encrypts the given raw bytes using the specified password.

#### `Decrypt(byte[] secretInput, string password = null)`
Decrypts the given encrypted bytes using the specified password.

#### `Encrypt(byte[] rawBytes, byte[] salt, bool useMd5)`
Encrypts the given raw bytes. An optional salt can be provided for encryption, and the use of MD5 can be specified.

#### `Decrypt(byte[] secretInput, byte[] salt, bool useMd5)`
Decrypts the given encrypted bytes. The salt and use of MD5 that were used for encryption must be provided.

### Examples
```c#
byte[] rawBytes = Encoding.UTF8.GetBytes("Hello, world!");

// Encrypt the raw bytes
byte[] encryptedBytes = ByteEncryption.Encrypt(rawBytes, "my-secret-key");

// Decrypt the encrypted bytes
byte[] decryptedBytes = ByteEncryption.Decrypt(encryptedBytes, "my-secret-key");

// Convert the decrypted bytes back to a string and print it
string decryptedString = Encoding.UTF8.GetString(decryptedBytes);
Console.WriteLine(decryptedString);  // Outputs: "Hello, world!"
```


## License

[The Unlicense](https://unlicense.org/)

