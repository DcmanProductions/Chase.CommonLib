/*
    Chase CommonLib - LFInteractive LLC. 2021-2024
    CommonLib is a library of common functions and classes for .NET 6.0+.
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using DeviceId;
using System.Text;
using System.Security.Cryptography;

namespace Chase.CommonLib.Math;

/// <summary>
/// A class for encrypting and decrypting strings and files.
/// </summary>
public class Crypt
{
    /// <summary>
    /// The salt used for encryption and decryption.
    /// </summary>
    public string Salt { get; set; }

    /// <summary>
    /// Creates a new instance of <see cref="Crypt"/> with a random salt based on the current machine.
    /// </summary>
    public Crypt() : this(new DeviceIdBuilder().AddMachineName().AddMacAddress().ToString())
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="Crypt"/> with the specified salt.
    /// </summary>
    /// <param name="salt"></param>
    public Crypt(string salt)
    {
        Salt = salt;
    }

    /// <summary>
    /// Encrypts the specified text.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public string Encrypt(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentNullException(nameof(text), "This can not be blank.");
        }

        using Aes aes = Aes.Create();
        using MemoryStream ms = new();
        using CryptoStream cs = new(ms, aes.CreateEncryptor(GetSaltBytes(), GetSaltBytes()), CryptoStreamMode.Write);
        using (StreamWriter sw = new(cs))
        {
            sw.Write(text);
        }
        return Convert.ToBase64String(ms.ToArray())[..^2];
    }

    /// <summary>
    /// Encrypts the specified file.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="output"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public void Encrypt(string path, string output)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentNullException(nameof(path), "This can not be blank.");
        }
        if (string.IsNullOrWhiteSpace(output))
        {
            throw new ArgumentNullException(nameof(output), "This can not be blank.");
        }

        using Aes aesAlg = Aes.Create();
        aesAlg.Key = GetSaltBytes();
        aesAlg.IV = GetSaltBytes();

        using FileStream inputFileStream = new(path, FileMode.Open);
        using FileStream outputFileStream = new(output, FileMode.Create);

        using CryptoStream cryptoStream = new(outputFileStream, aesAlg.CreateEncryptor(), CryptoStreamMode.Write);

        inputFileStream.CopyTo(cryptoStream);
    }

    /// <summary>
    /// Decrypts the specified file.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="output"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public void Decrypt(string path, string output)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentNullException(nameof(path), "This can not be blank.");
        }
        if (string.IsNullOrWhiteSpace(output))
        {
            throw new ArgumentNullException(nameof(output), "This can not be blank.");
        }

        using Aes aesAlg = Aes.Create();
        aesAlg.Key = GetSaltBytes();
        aesAlg.IV = GetSaltBytes();

        using FileStream inputFileStream = new(path, FileMode.Open);
        using FileStream outputFileStream = new(output, FileMode.Create);

        using CryptoStream cryptoStream = new(outputFileStream, aesAlg.CreateDecryptor(), CryptoStreamMode.Write);

        inputFileStream.CopyTo(cryptoStream);
    }

    /// <summary>
    /// Decrypts the specified text.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public string Decrypt(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentNullException(nameof(text), "This can not be blank.");
        }

        byte[] cipher = Convert.FromBase64String(text + "==");
        using Aes aes = Aes.Create();
        using MemoryStream ms = new(cipher);
        using CryptoStream cs = new(ms, aes.CreateDecryptor(GetSaltBytes(), GetSaltBytes()), CryptoStreamMode.Read);
        using StreamReader reader = new(cs);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Gets the salt bytes.
    /// </summary>
    /// <returns></returns>
    private byte[] GetSaltBytes()
    {
        byte[] saltBytes = Encoding.UTF8.GetBytes(Salt);
        byte[] secret = new byte[16];
        for (int i = 0; i < secret.Length; i++)
        {
            if (i < saltBytes.Length)
            {
                secret[i] = (byte)Salt[i];
            }
            else
            {
                secret[i] = (byte)i;
            }
        }
        return secret;
    }
}