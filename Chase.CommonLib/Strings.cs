/*
    Chase CommonLib - LFInteractive LLC. 2021-2024
    CommonLib is a library of common functions and classes for .NET 6.0+.
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

namespace Chase.CommonLib;

/// <summary>
/// A static class for string related functions
/// </summary>
public static class Strings
{
    /// <summary>
    /// Gets a valid file name from the original string
    /// </summary>
    /// <param name="original"></param>
    /// <returns></returns>
    public static string GetValidFileName(string original)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            original = original.Replace(c.ToString(), "");
        }
        return original;
    }

    /// <summary>
    /// Checks if the string is alpha numeric.
    /// <code>Ex: "abc123" returns true, "abc123!" returns false</code>
    /// </summary>
    /// <param name="str">The string to check.</param>
    /// <returns>True if the string is alphanumeric, false otherwise.</returns>
    public static bool IsAlphaNumeric(string str)
    {
        foreach (char c in str)
        {
            if (!char.IsLetterOrDigit(c)) return false;
        }
        return true;
    }

    /// <summary>
    /// Checks if the string is alphabetical.
    /// <code>Ex: "abc" returns true, "abc123" returns false</code>
    /// </summary>
    /// <param name="str">The string to check.</param>
    /// <returns>True if the string is alphabetical, false otherwise.</returns>
    public static bool IsAlphabetical(string str)
    {
        foreach (char c in str)
        {
            if (!char.IsLetter(c)) return false;
        }
        return true;
    }
}