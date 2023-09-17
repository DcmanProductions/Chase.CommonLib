/*
    Chase CommonLib - LFInteractive LLC. 2021-2024
    CommonLib is a library of common functions and classes for .NET 6.0+.
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Newtonsoft.Json;
using Serilog;

namespace Chase.CommonLib.FileSystem.Configuration;

/// <summary>
/// Base class for configuration files.
/// </summary>
public class AppConfigBase<T> : ConfigurationFile<T> where T : AppConfigBase<T>, new()
{
    private static readonly Lazy<T> _instance = new(() => new T());

    /// <summary>
    /// The singleton instance of the configuration file.
    /// </summary>
    [JsonIgnore]
    public static T Instance => _instance.Value;

    /// <summary>
    /// Initializes and loads the configuration file.
    /// </summary>
    /// <param name="path"></param>
    public virtual void Initialize(string path)
    {
        Log.Debug("Initializing config file: {CONFIG}", path);
        Path = path;
        Instance.Load();
    }

    /// <summary>
    /// Loads the configuration file from disk.
    /// </summary>
    /// <exception cref="IOException">If the configuration file path is not set.</exception>
    public override T? Load()
    {
        T loadedInstance = base.Load() ?? Instance;
        CopyProperties(loadedInstance);
        return Instance;
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