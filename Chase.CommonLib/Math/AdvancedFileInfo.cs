/*
    Chase CommonLib - LFInteractive LLC. 2021-2024
    CommonLib is a library of common functions and classes for .NET 6.0+.
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

namespace Chase.CommonLib.Math;

/// <summary>
/// An enum for the different file size units!
/// </summary>
public enum FileSizeUnit
{
    /// <summary>
    /// Ex: 1.2 MB
    /// </summary>
    Bytes,

    /// <summary>
    /// Ex: 1.2 Mb
    /// </summary>
    Bits,

    /// <summary>
    /// Ex: 1.2 MiB
    /// </summary>
    IbiBytes,
}

/// <summary>
/// Provides additional information about a file!
/// </summary>
public class AdvancedFileInfo
{
    /// <summary>
    /// The base <see cref="FileInfo"/> for the path specified!
    /// </summary>
    public FileInfo BaseInfo { get; }

    /// <summary>
    /// Gets if the path is a directory or not!
    /// </summary>
    public bool IsDirectory => BaseInfo.Attributes.HasFlag(FileAttributes.Directory);

    /// <summary>
    /// Creates a new <see cref="AdvancedFileInfo"/> from the path specified!
    /// </summary>
    /// <param name="file"></param>
    public AdvancedFileInfo(string file)
    {
        BaseInfo = new FileInfo(file);
    }

    /// <summary>
    /// Parses the file size to a string with the specified unit and decimal places!
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="places"></param>
    /// <param name="unit"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static string SizeToString(long bytes, int places = 2, FileSizeUnit unit = FileSizeUnit.Bytes)
    {
        string[] sizeSuffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB" };
        string[] bitSuffixes = { "b", "Kb", "Mb", "Gb", "Tb", "Pb", "Eb", "Zb" };
        string[] ibiSuffixes = { "iB", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB", "ZiB" };

        int index = 0;
        string[] suffixes = sizeSuffixes;
        double size = bytes;

        int section = 0;

        switch (unit)
        {
            case FileSizeUnit.Bits:
                size *= 8;
                section = 1000;
                suffixes = bitSuffixes;
                break;

            case FileSizeUnit.Bytes:
                section = 1000;
                suffixes = sizeSuffixes;
                break;

            case FileSizeUnit.IbiBytes:
                section = 1024;
                suffixes = ibiSuffixes;
                break;
        }
        while (size >= section && index < suffixes.Length - 1)
        {
            size /= section;
            index++;
        }

        return $"{Round(size, places)} {suffixes[index]}";
    }

    /// <summary>
    /// Parses the file size to a string with the specified unit and decimal places!
    /// </summary>
    /// <param name="places"></param>
    /// <param name="unit"></param>
    /// <returns></returns>
    public string SizeToString(int places = 2, FileSizeUnit unit = FileSizeUnit.Bytes)
    {
        return SizeToString(BaseInfo.Length, places, unit);
    }

    private static double Round(double value, int decimalPlaces)
    {
        double multiplier = System.Math.Pow(10, decimalPlaces);
        return System.Math.Round(value * multiplier) / multiplier;
    }
}