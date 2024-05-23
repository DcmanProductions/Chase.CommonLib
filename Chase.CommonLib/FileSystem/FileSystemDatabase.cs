/*
    Chase CommonLib - LFInteractive LLC. 2021-2024
    CommonLib is a library of common functions and classes for .NET 6.0+.
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Chase.CommonLib.Math;
using Newtonsoft.Json;

namespace Chase.CommonLib.FileSystem;

/// <summary>
/// Creates a database from the file system using directories and files. This is similar to <seealso
/// cref="DatabaseFile">Database File</seealso> without the compression and allows for much larger
/// file sizes and faster open/close times.
/// </summary>
public class FileSystemDatabase : IDisposable
{
    private readonly string filePath;
    private readonly bool autoFlush;
    private readonly Dictionary<Guid, IDisposable> queuedStreams;
    private readonly AdvancedTimer? timer;

    /// <summary>
    /// Creates a file system database object.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="auto_flush">If the database should flush to disk every write.</param>
    public FileSystemDatabase(string filePath, bool auto_flush = false)
    {
        this.filePath = filePath;
        queuedStreams = new();
        autoFlush = auto_flush;
    }

    /// <summary>
    /// Creates a file system database object with a flush interval.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="flush_interval"></param>
    public FileSystemDatabase(string filePath, TimeSpan flush_interval)
    {
        this.filePath = filePath;
        queuedStreams = new();
        autoFlush = false;
        timer = new(flush_interval)
        {
            AutoReset = true,
            Interruptible = true,
        };
        timer.Elapsed += (s, e) => Flush();
        timer.Start();
    }

    /// <summary>
    /// Creates a file system database object with a flush interval in milliseconds.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="flush_interval">flush interval in milliseconds</param>
    public FileSystemDatabase(string filePath, long flush_interval) : this(filePath, TimeSpan.FromMilliseconds(flush_interval))
    {
    }

    /// <summary>
    /// Statically opens a file system database object.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="auto_flush"></param>
    /// <returns></returns>
    public static FileSystemDatabase Open(string filePath, bool auto_flush = false)
    {
        return new(filePath, auto_flush);
    }

    /// <summary>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void WriteEntry(Guid key, object value)
    {
        if (queuedStreams.ContainsKey(key) && queuedStreams[key] is StreamWriter writer)
        {
            writer.Write(JsonConvert.SerializeObject(value));
            if (autoFlush)
            {
                writer.Flush();
            }
        }
        else
        {
            queuedStreams[key] = new StreamWriter(ParseEntryPath(key), false);
            WriteEntry(key, value);
        }
    }

    /// <summary>
    /// Writes a file to the database file.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="file"></param>
    public void WriteEntry(Guid key, FileStream file)
    {
        if (queuedStreams.ContainsKey(key) && queuedStreams[key] is FileStream fs)
        {
            file.CopyTo(fs);
            if (autoFlush)
            {
                fs.Flush();
            }
        }
        else
        {
            queuedStreams[key] = new FileStream(ParseEntryPath(key), FileMode.Create, FileAccess.Write, FileShare.None);
            WriteEntry(key, file);
        }
    }

    /// <summary>
    /// Reads an entry from the database file.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public T? ReadEntry<T>(Guid key)
    {
        if (Exists(key))
        {
            using FileStream? fs = ReadFile(key);
            if (fs != null)
            {
                using StreamReader reader = new(fs);
                return JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
            }
        }
        return default;
    }

    /// <summary>
    /// Reads a file from the database file.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public FileStream? ReadFile(Guid key)
    {
        if (Exists(key))
        {
            return new(ParseEntryPath(key), FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        return null;
    }

    /// <summary>
    /// Checks if an entry exists in the database.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool Exists(Guid key) => File.Exists(ParseEntryPath(key));

    /// <summary>
    /// Flushes the queued database items to disk.
    /// </summary>
    public void Flush()
    {
        Parallel.ForEach(queuedStreams, stream =>
        {
            if (stream.Value is FileStream fileStream)
            {
                fileStream.Flush();
            }
            else if (stream.Value is StreamWriter writer)
            {
                writer.Flush();
            }
        });
    }

    /// <summary>
    /// Flushes and Disposes of the database object and its references.
    /// </summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Parallel.ForEach(queuedStreams, stream => stream.Value.Dispose());
        queuedStreams.Clear();
        timer?.Stop();
        timer?.Dispose();
    }

    private string ParseEntryPath(Guid key) => Path.Combine(Directory.CreateDirectory(Path.Combine(filePath, key.ToString("N")[..2])).FullName, key.ToString("N"));
}