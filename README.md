# Chase CommonLib
A library of common functions and classes for .NET 6.0+.

# Configuration File

## Overview

The `ConfigurationFile<T>` class is a part of the Chase CommonLib library, provided by LFInteractive LLC. This class is designed for managing configuration files in .NET 6.0+ applications. It allows you to easily read and write configuration data stored in JSON format to a file. 

## Class Declaration

```csharp
namespace Chase.CommonLib.FileSystem.Configuration;

public class ConfigurationFile<T> : IDisposable
```

### Type Parameters

- `T`: The type representing the content of the configuration file.

## Properties

### `Content`

- Type: `T?`
- Description: Gets or sets the content of the configuration file.

### `KeepOpen`

- Type: `bool`
- Description: Gets a value indicating whether the configuration file should be kept open for efficient read and write operations.

### `FilePath`

- Type: `string`
- Description: Gets the path to the configuration file.

## Constructors

### `ConfigurationFile(string filePath, bool keepOpen = false)`

- Parameters:
  - `filePath`: The path to the configuration file.
  - `keepOpen`: A boolean indicating whether to keep the file open for efficient read and write operations. Default is `false`.

- Description: Initializes a new instance of the `ConfigurationFile<T>` class. It creates or opens the specified configuration file based on the provided file path and optional keepOpen parameter.

## Methods

### `Save()`

- Returns: `bool`
- Description: Saves the contents of the `Content` property to the configuration file. If `KeepOpen` is set to `true`, it will use an open file stream for writing; otherwise, it will write directly to the file.

### `Load()`

- Returns: `T?`
- Description: Loads the contents of the configuration file into the `Content` property. If the file fails to load or doesn't exist, it returns the original content.

### `Dispose()`

- Description: Releases all resources used by the `ConfigurationFile<T>` object. This method saves the configuration file (if necessary) and disposes of the underlying file stream.

## Example Usage

```csharp
using Chase.CommonLib.FileSystem.Configuration;

// Define a class to represent your configuration data
public class AppConfig
{
    public string ApiKey { get; set; }
    public int MaxRetryAttempts { get; set; }
}

// Create a ConfigurationFile instance
var configFilePath = "app-config.json";
var config = new ConfigurationFile<AppConfig>(configFilePath);

// Load the configuration from the file (or use default values if the file doesn't exist)
var appConfig = config.Load();

// Modify the configuration
appConfig.ApiKey = "your-api-key";
appConfig.MaxRetryAttempts = 3;

// Save the modified configuration back to the file
config.Save();

// Dispose of the ConfigurationFile when done
config.Dispose();
```

In this example, we create a `ConfigurationFile` instance for managing an `AppConfig` object. We load the configuration from the file, make changes, and then save it back to the file. Finally, we dispose of the `ConfigurationFile` object to release resources and ensure the changes are persisted.

# AppConfigBase Class Documentation

The `AppConfigBase` class is a base class for configuration files in the Chase CommonLib library. It provides a framework for managing and handling configuration data. This class is designed to be extended by specific configuration classes that define their own configuration properties and behavior.

## Table of Contents
- [Namespace](#namespace)
- [Inheritance](#inheritance)
- [Constructor](#constructor)
- [Properties](#properties)
- [Events](#events)
- [Methods](#methods)
- [Example Usage](#example-usage)
  
## Namespace

```csharp
namespace Chase.CommonLib.FileSystem.Configuration
```

## Inheritance

- `AppConfigBase` is the base class for all configuration classes in the library.

## Constructor

### `public AppConfigBase()`

- Initializes a new instance of the `AppConfigBase` class. This constructor is protected to enforce the Singleton pattern.

## Properties

### `public static AppConfigBase Instance { get; protected set; }`

- Gets the singleton instance of the configuration file.

### `public string Path { get; set; }`

- Gets or sets the configuration file path.

## Events

### `public event ConfigurationEventHandler? ConfigurationSaved`

- Event that is fired when the configuration file is saved. Subscribers can handle this event to perform actions after the configuration is saved.

### `public event ConfigurationEventHandler? ConfigurationLoaded`

- Event that is fired when the configuration file is loaded. Subscribers can handle this event to perform actions after the configuration is loaded.

## Methods

### `public virtual void Initialize(string path)`

- Initializes and loads the configuration file.
  
  - `path`: A string representing the file path of the configuration file to load.

### `public virtual void Save()`

- Saves the configuration file to disk.

  - Throws `IOException` if the configuration file path is not set.

### `public virtual void Load()`

- Loads the configuration file from disk.
  
  - Throws `IOException` if the configuration file path is not set.

## Example Usage

```csharp
using Chase.CommonLib.FileSystem.Configuration;
using System;

namespace YourNamespace
{
    // Define a custom configuration class that extends AppConfigBase
    public class CustomConfig : AppConfigBase
    {
        public string SomeSetting { get; set; }

        [JsonProperty("another_setting")]
        public int AnotherSetting { get; set; }

        [JsonIgnore]
        public int IgnoredSetting { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Create an instance of your custom configuration class
            var customConfig = new CustomConfig();

            // Set the path to the configuration file
            customConfig.Initialize("config.json");

            // Modify configuration settings
            customConfig.SomeSetting = "Hello, World!";
            customConfig.AnotherSetting = 42;

            // Save the configuration to disk
            customConfig.Save();

            // Load the configuration from disk
            customConfig.Load();

            // Access configuration settings
            Console.WriteLine($"SomeSetting: {customConfig.SomeSetting}");
            Console.WriteLine($"AnotherSetting: {customConfig.AnotherSetting}");
        }
    }
}
```

In this example, we create a custom configuration class `CustomConfig` that extends `AppConfigBase`. We initialize, modify, save, and load configuration settings using this custom class.

# Database File

## Overview

The `DatabaseFile` class is a part of the Chase CommonLib library, designed for use with .NET 6.0 and above. It provides functionality to work with compressed database files containing a collection of entries. This class allows you to efficiently store and retrieve large amounts of data using a key-value approach. Each entry is identified by a unique `Guid` key and can store any serializable object.

## Licensing

This class is licensed under the [GNU General Public License (GPL-3.0)](https://www.gnu.org/licenses/gpl-3.0.en.html#license-text).

## Namespace

```csharp
using Chase.CommonLib.FileSystem.Configuration;
```

## Constructors

### `DatabaseFile(string filePath)`

- **Description**: Creates a new instance of the `DatabaseFile` class and opens or creates a database file at the specified `filePath`.
- **Parameters**:
  - `filePath` (string): The path to the database file.
- **Example**:

```csharp
string dbFilePath = "myDatabase.db";
DatabaseFile database = new DatabaseFile(dbFilePath);
```

### `static DatabaseFile Open(string filePath)`

- **Description**: Creates or opens an instance of the `DatabaseFile` class from an existing database file at the specified `filePath`.
- **Parameters**:
  - `filePath` (string): The path to the database file.
- **Returns**: An instance of the `DatabaseFile` class.
- **Example**:

```csharp
string dbFilePath = "myDatabase.db";
DatabaseFile database = DatabaseFile.Open(dbFilePath);
```

## Methods

### `void WriteEntry(Guid key, object value)`

- **Description**: Writes an entry to the database file with the specified `Guid` key and associated object value. If an entry with the same key already exists, it will be overwritten.
- **Parameters**:
  - `key` (Guid): The unique identifier for the entry.
  - `value` (object): The value to be stored in the entry. This value must be serializable.
- **Example**:

```csharp
Guid entryKey = Guid.NewGuid();
string entryValue = "This is my data.";
database.WriteEntry(entryKey, entryValue);
```

### `T? ReadEntry<T>(Guid key)`

- **Description**: Reads an entry from the database file with the specified `Guid` key and deserializes it into the specified type `T`.
- **Parameters**:
  - `key` (Guid): The unique identifier for the entry.
- **Returns**: The deserialized value of the entry, or `default(T)` if the entry does not exist.
- **Example**:

```csharp
Guid entryKey = Guid.Parse("c7f106bfa0e84c66b9f6d2c4f9e3c3b7");
string entryValue = database.ReadEntry<string>(entryKey);
```

### `bool Exists(Guid key)`

- **Description**: Checks if an entry with the specified `Guid` key exists in the database file.
- **Parameters**:
  - `key` (Guid): The unique identifier for the entry.
- **Returns**: `true` if the entry exists; otherwise, `false`.
- **Example**:

```csharp
Guid entryKey = Guid.Parse("c7f106bfa0e84c66b9f6d2c4f9e3c3b7");
bool entryExists = database.Exists(entryKey);
```

### `void Dispose()`

- **Description**: Releases all resources used by the `DatabaseFile` object and writes any pending changes to the file. This method should be called when you are done using the `DatabaseFile`.
- **Example**:

```csharp
database.Dispose();
```

## Example Usage

Here is an example of how to use the `DatabaseFile` class:

```csharp
string dbFilePath = "myDatabase.db";
DatabaseFile database = new DatabaseFile(dbFilePath);

Guid entryKey = Guid.NewGuid();
string entryValue = "This is my data.";

// Write an entry
database.WriteEntry(entryKey, entryValue);

// Check if an entry exists
bool entryExists = database.Exists(entryKey);

if (entryExists)
{
    // Read the entry
    string retrievedValue = database.ReadEntry<string>(entryKey);
    Console.WriteLine($"Retrieved Value: {retrievedValue}");
}

// Dispose of the DatabaseFile to release resources
database.Dispose();
```

This example demonstrates creating a `DatabaseFile`, writing an entry, checking its existence, reading it, and finally disposing of the `DatabaseFile` object when done.

Please ensure that you have appropriate error handling and data validation in your actual implementation.