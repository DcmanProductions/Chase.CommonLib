/*
    Chase CommonLib - LFInteractive LLC. 2021-2024
    CommonLib is a library of common functions and classes for .NET 6.0+.
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Newtonsoft.Json;
using Serilog;
using System.IO.Compression;

namespace Chase.CommonLib.FileSystem;

/// <summary>
/// A database file is a compressed file that contains a collection of entries, this is best used
/// for large amounts of data.
/// </summary>
public class DatabaseFile : IDisposable
{
    private readonly ZipArchive baseStream;
    private readonly string filePath;

    /// <summary>
    /// Creates a new database file.
    /// </summary>
    /// <param name="filePath"></param>
    public DatabaseFile(string filePath)
    {
        Log.Debug("Creating or opening database file: {FILE}", filePath);
        this.filePath = filePath;
        baseStream = ZipFile.Open(filePath, ZipArchiveMode.Update);
    }

    /// <summary>
    /// Creates or opens a database file.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static DatabaseFile Open(string filePath)
    {
        return new(filePath);
    }

    /// <summary>
    /// Writes an entry to the database file.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void WriteEntry(Guid key, object value)
    {
        Log.Debug("Writing entry to {KEY}", key.ToString("N"));
        ZipArchiveEntry zipEntry = baseStream.GetEntry(ParseEntryPath(key)) ?? baseStream.CreateEntry(ParseEntryPath(key), CompressionLevel.SmallestSize);
        using Stream stream = zipEntry.Open();
        using StreamWriter writer = new(stream);
        writer.Write(JsonConvert.SerializeObject(value));
        writer.Flush();
        stream.Flush();
    }

    /// <summary>
    /// Reads an entry from the database file.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public T? ReadEntry<T>(Guid key)
    {
        ZipArchiveEntry? zipEntry = baseStream.GetEntry(ParseEntryPath(key));
        if (zipEntry != null)
        {
            Log.Debug("Reading entry {KEY}", key.ToString("N"));
            using Stream stream = zipEntry.Open();
            using StreamReader reader = new(stream);
            return JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
        }
        return default;
    }

    /// <summary>
    /// Checks if an entry exists in the database file.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool Exists(Guid key)
    {
        ZipArchiveEntry? zipEntry = baseStream.GetEntry(ParseEntryPath(key));
        return zipEntry != null;
    }

    /// <summary>
    /// Releases all resources used by the database file and writes them to file.
    /// </summary>
    public void Dispose()
    {
        Log.Warning("Disposing of the Database File: {FILE}", filePath);
        baseStream.Dispose();
        GC.SuppressFinalize(this);
    }

    private static string ParseEntryPath(Guid key) => Path.Combine(key.ToString("N")[..2], key.ToString("N"));
}