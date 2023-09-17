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
/// A configuration file is a json file that contains a collection of entries.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ConfigurationFile<T> : IDisposable
{
    /// <summary>
    /// The Configuration Files Content
    /// </summary>
    public T? Content { get; set; }

    /// <summary>
    /// If the Configuration File should be kept open or not
    /// </summary>
    public bool KeepOpen { get; private set; }

    /// <summary>
    /// The Configuration Files Path
    /// </summary>
    public string FilePath { get; private set; }

    private FileStream? stream { get; set; }

    /// <summary>
    /// Creates or opens a configuration file.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="keepOpen"></param>
    public ConfigurationFile(string filePath, bool keepOpen = false)
    {
        Log.Debug("Creating or opening configuration file: {FILE}", filePath);
        KeepOpen = keepOpen;
        FilePath = filePath;
        if (keepOpen)
        {
            stream = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        }
    }

    /// <summary>
    /// Saves the contents of <see cref="Content">Content</see> to the configuration file.
    /// </summary>
    /// <returns></returns>
    public bool Save()
    {
        Log.Debug("Saving configuration file: {FILE}", FilePath);
        if (KeepOpen)
        {
            stream ??= File.Open(FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            using StreamWriter writer = new(stream);
            writer.Write(JsonConvert.SerializeObject(Content));
            writer.Flush();
            stream.Flush();
            return true;
        }
        else
        {
            File.WriteAllText(FilePath, JsonConvert.SerializeObject(Content));
            return true;
        }
    }

    /// <summary>
    /// Loads the contents of the configuration file into <see cref="Content">Content</see>. <br/>
    /// If the file fails to load the original content will be returned.
    /// </summary>
    /// <returns>The loaded content.</returns>
    public T? Load()
    {
        Log.Debug("Loading configuration file: {FILE}", FilePath);
        if (KeepOpen)
        {
            stream ??= File.Open(FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            using StreamReader reader = new(stream);
            Content = JsonConvert.DeserializeObject<T>(reader.ReadToEnd()) ?? Content;
        }
        else
        {
            Content = JsonConvert.DeserializeObject<T>(File.ReadAllText(FilePath)) ?? Content;
        }
        return Content;
    }

    /// <summary>
    /// Releases all resources used by the <see cref="ConfigurationFile{T}"/> object.
    /// </summary>
    public void Dispose()
    {
        Log.Warning("Disposing of the Configuration File: {FILE}", FilePath);
        Save();
        stream?.Dispose();
        GC.SuppressFinalize(this);
    }
}