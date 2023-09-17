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
/// A configuration file is a json file that contains a collection of entries.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ConfigurationFile<T> where T : ConfigurationFile<T>, new()
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

    /// <summary>
    /// The configuration file path.
    /// </summary>
    [JsonIgnore]
    public string Path { get; set; } = "";

    /// <summary>
    /// Default constructor.
    /// </summary>
    protected ConfigurationFile()
    {
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
    public virtual T? Load()
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

            ConfigurationLoaded?.Invoke(this, EventArgs.Empty);
            T? item = JObject.Parse(File.ReadAllText(Path))?.ToObject<T>();
            if (item != null)
            {
                item.Path = Path;
            }
            return item;
        }
        return null;
    }
}