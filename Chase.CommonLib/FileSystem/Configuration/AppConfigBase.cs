/*
    Chase CommonLib - LFInteractive LLC. 2021-2024
    CommonLib is a library of common functions and classes for .NET 6.0+.
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Chase.CommonLib.FileSystem.Configuration;

/// <summary>
/// Base class for configuration files.
/// </summary>
public class AppConfigBase<T> where T : AppConfigBase<T>, new()
{
    /// <summary>
    /// Event handler for when the configuration file is updated.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void ConfigurationEventHandler(object sender, EventArgs e);

    /// <summary>
    /// Event that is fired when the configuration file is saved.
    /// </summary>
    public event ConfigurationEventHandler? ConfigurationSaved;

    /// <summary>
    /// Event that is fired when the configuration file is loaded.
    /// </summary>
    public event ConfigurationEventHandler? ConfigurationLoaded;

    private static Lazy<T> _instance = new(() => new T());

    /// <summary>
    /// The singleton instance of the configuration file.
    /// </summary>
    [JsonIgnore]
    public static T Instance => _instance.Value;

    /// <summary>
    /// The configuration file path.
    /// </summary>
    [JsonIgnore]
    public string Path { get; set; } = "";

    /// <summary>
    /// Default constructor.
    /// </summary>
    protected AppConfigBase()
    {
    }

    /// <summary>
    /// Initializes and loads the configuration file.
    /// </summary>
    /// <param name="path"></param>
    public virtual void Initialize(string path)
    {
        Log.Debug("Initializing config file: {CONFIG}", path);
        Instance.Path = path;
        Instance.Load();
    }

    /// <summary>
    /// Saves the configuration file from.
    /// </summary>
    /// <exception cref="IOException">If the configuration file path is not set.</exception>
    public virtual void Save()
    {
        if (string.IsNullOrEmpty(Path))
        {
            throw new IOException("Configuration file path is not set.");
        }
        Log.Debug("Saving config file: {CONFIG}", Path);
        using (StreamWriter writer = File.CreateText(Path))
        {
            writer.Write(JsonConvert.SerializeObject(this, Formatting.Indented));
            writer.Flush();
        }

        ConfigurationSaved?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Loads the configuration file from disk.
    /// </summary>
    /// <exception cref="IOException">If the configuration file path is not set.</exception>
    public virtual void Load()
    {
        if (string.IsNullOrEmpty(Path))
        {
            throw new IOException("Configuration file path is not set.");
        }

        if (!File.Exists(Path))
        {
            Save();
        }
        else
        {
            Log.Debug("Loading config file: {CONFIG}", Path);

            T loadedInstance = JObject.Parse(File.ReadAllText(Path))?.ToObject<T>() ?? Instance;
            CopyProperties(loadedInstance);

            ConfigurationLoaded?.Invoke(this, EventArgs.Empty);
        }
    }

    private void CopyProperties(T source)
    {
        foreach (var property in typeof(T).GetProperties())
        {
            if (property.CanWrite)
            {
                property.SetValue(this, property.GetValue(source));
            }
        }
    }
}