The project is maintained at GitHub: https://github.com/armalagit/FConfig.NetStandard

# FConfig
## What is FConfig?
FConfig is a simple method to handle application configuration. Supports encryption. It is very easy to use and has no dependencies. Thanks to the framework it uses I don't think there should be any noticable compatibility issues.

## Pre-requisites
This little piece of fine code runs on **NetStandard 2.0**.

## How to use
### **Construct**
To construct the initial method call ``FConfig.Construct(<file path>, <secret key>, <encrypt>)`` once. Calling this method again reloads the configuration from the file.
#### FCondig.Construct
| Parameter| Object type | Default value | Optional | Description |
|---|---|---|---|---|
| **file path** | string | string.Empty | false | Directory path to where the configuration will be or is saved at. |
| **secret key** | string | string.Empty | true | A 32 character secret key to use for encryption. |
| **encrypt** | boolean | true | true | Encrypt configuration before flushing the bytes to disk. |
#### Encrypt
Using encryption requires the ``secret key`` to be supplied. The secret key must be a **32 character** string.

### **Get**
* Read value from collection: ``FConfig.Get<T>(identifier)``
#### Method
Optionally you can supply the method with a fallback object. This fallback object is also written to the configuration file.

| Usage | Method | Return type | Object identifier | Fallback object (optional) |
|---|---|---|---|---|
| FConfig.Get<string>("key", "fallback") | Get | object | string | object |

### **Set**
* Overwrite new value: ``FConfig.Set<T>(<identifier>, <value>)``
* Remove value from collection: ``FConfig.Set<T>(<identifier>)``
#### Method
After each ``Set<T>`` the file is written to the disk.

| Usage | Method | Return type | Object identifier | New value object |
|---|---|---|---|---|
| FConfig.Set<string>("key", "value") | Set | object | string | object |
