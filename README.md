The project is maintained at GitHub: https://github.com/armalagit/FConfig.NetStandard

# FConfig
## What is FConfig?
FConfig is a simple method to handle application configuration. In an essence this is just a fancy ``Dictionary<key, value>`` which saves the collection array to a disk and even encrypts it. Because this is actually a Dictionary in disguise you can save anything as the value. It is very easy to use and has no dependencies. Thanks to the framework it uses I don't think there should be any noticable compatibility issues.

## Pre-requisites
This little piece of fine code runs on **NetStandard 2.0**.

## How to use
### **Construct**
To construct the initial method call this method once per project initialization. Calling this method again in the same instance reloads the configuration from the file.
#### FConfig.Construct
| Parameter| Object type | Default value | Optional | Description |
|---|---|---|---|---|
| configurationPath | string | string.Empty | false | Directory path to where the configuration will be or is saved at. |
| secretKey | string | string.Empty | true | A 32 character secret key to use for encryption. |
| useEncryption | boolean | true | true | Encrypt configuration before flushing the bytes to disk. |
##### Encrypt
Using encryption requires the ``secret key`` to be supplied. The secret key must be a **32 character** string.

### **Get**
Reads the specified configuration item from the collection.
#### FConfig.Get
| Parameter| Object type | Default value | Optional | Description |
|---|---|---|---|---|
| identifier | string | string.Empty | false | Configuration collection item identifier. |
| fallback | object | defaukt | true | Configuration fallback item to return on null result. |

### **Set**
Sets the specified value in the configuration collection.
#### FConfig.Set
After each ``Set<T>`` the configuration bytes are flushed to the disk.
| Parameter| Object type | Default value | Optional | Description |
|---|---|---|---|---|
| identifier | string | string.Empty | false | Configuration collection item identifier. |
| newValue | object | defaukt | false | New value. |

### **Del**
Removes the specified configuration item from the collection.
#### FConfig.Del
After each ``Del<T>`` the configuration bytes are flushed to the disk.
| Parameter| Object type | Default value | Optional | Description |
|---|---|---|---|---|
| identifier | string | string.Empty | false | Configuration collection item identifier. |
