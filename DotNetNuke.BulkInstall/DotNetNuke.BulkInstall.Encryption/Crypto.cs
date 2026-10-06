// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.BulkInstall.Encryption;

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

/// <summary>Cryptography utilities.</summary>
/// <remarks>
/// This implementation is almost entirely based on the answer found on Stack Overflow here:
/// <see href="https://stackoverflow.com/questions/10168240/encrypting-decrypting-a-string-in-c-sharp" />
///
/// I've added methods to support streams.
/// </remarks>
public class Crypto
{
    // Used to determine the keysize of the encryption algorithm in bits.
    // This is divided by 8 later to get the equivalent number of bytes.
    private const int KeySize = 256;

    // The AES specification states that the block size must be 128.
    private const int BlockSize = 128;

    // Initialisation vector size.
    private const int IvSize = 128;

    // Salt size.
    private const int SaltSize = 256;

    // Determines the number of iterations used during password generation.
    private const int DerivationIterations = 1000;

    /// <summary>Encrypts the <paramref name="plainStream"/>.</summary>
    /// <param name="plainStream">The contents to encrypt.</param>
    /// <param name="passPhrase">The pass phrase with which to encrypt the contents.</param>
    /// <returns>A <see cref="Stream"/> with the encrypted contents.</returns>
    public static Stream Encrypt(Stream plainStream, string passPhrase)
    {
        // Read bytes from stream.
        byte[] plainBytes;

        using (var ms = new MemoryStream())
        {
            var buffer = new byte[2048];
            int bytesRead;

            while ((bytesRead = plainStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, bytesRead);
            }

            plainBytes = ms.ToArray();
        }

        // Encrypt bytes.
        var encryptedBytes = Encrypt(plainBytes, passPhrase);

        // Create stream and return.
        return new MemoryStream(encryptedBytes);
    }

    /// <summary>Encrypts the <paramref name="plainText"/>.</summary>
    /// <param name="plainText">The contents to encrypt.</param>
    /// <param name="passPhrase">The pass phrase with which to encrypt the contents.</param>
    /// <returns>A <see cref="string"/> with the encrypted contents as base 64.</returns>
    public static string Encrypt(string plainText, string passPhrase)
    {
        // Read string as bytes.
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

        // Encrypt bytes.
        byte[] encryptedBytes = Encrypt(plainBytes, passPhrase);

        // Convert back to string and return.
        return Convert.ToBase64String(encryptedBytes);
    }

    /// <summary>Encrypts the <paramref name="plainBytes"/>.</summary>
    /// <param name="plainBytes">The contents to encrypt.</param>
    /// <param name="passPhrase">The pass phrase with which to encrypt the contents.</param>
    /// <returns>An array of <see cref="byte"/> values with the encrypted contents.</returns>
    public static byte[] Encrypt(byte[] plainBytes, string passPhrase)
    {
        // Bytes for salt and initialisation vector are generated randomly each time.
        var saltBytes = GenerateRandomEntropy(SaltSize);
        var ivBytes = GenerateRandomEntropy(IvSize);

        using var password = new Rfc2898DeriveBytes(passPhrase, saltBytes, DerivationIterations);
        var keyBytes = password.GetBytes(KeySize / 8);

        using var symmetricKey = new AesManaged();
        symmetricKey.BlockSize = BlockSize;
        symmetricKey.Mode = CipherMode.CBC;
        symmetricKey.Padding = PaddingMode.PKCS7;

        using var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivBytes);
        using var memoryStream = new MemoryStream();
        using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
        cryptoStream.Write(plainBytes, 0, plainBytes.Length);
        cryptoStream.FlushFinalBlock();

        // Initialise cipher bytes with the salt.
        var cipherBytes = saltBytes;

        // Add the initialisation vector bytes.
        cipherBytes = cipherBytes.Concat(ivBytes).ToArray();

        // Finally add the encrypted data.
        return cipherBytes.Concat(memoryStream.ToArray()).ToArray();
    }

    /// <summary>Decrypts the <paramref name="encryptedStream"/>.</summary>
    /// <param name="encryptedStream">The encrypted contents.</param>
    /// <param name="passPhrase">The pass phrase with which to decrypt the contents.</param>
    /// <returns>A <see cref="Stream"/> with the decrypted contents.</returns>
    public static Stream Decrypt(Stream encryptedStream, string passPhrase)
    {
        // Read bytes from stream.
        byte[] encryptedBytes;

        using (var ms = new MemoryStream())
        {
            var buffer = new byte[2048];
            int bytesRead;

            while ((bytesRead = encryptedStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, bytesRead);
            }

            encryptedBytes = ms.ToArray();
        }

        // Decrypt bytes.
        var plainBytes = Decrypt(encryptedBytes, passPhrase);

        // Create stream and return.
        return new MemoryStream(plainBytes);
    }

    /// <summary>Decrypts the <paramref name="encryptedText"/>.</summary>
    /// <param name="encryptedText">The encrypted contents.</param>
    /// <param name="passPhrase">The pass phrase with which to decrypt the contents.</param>
    /// <returns>A <see cref="string"/> with the decrypted contents.</returns>
    public static string Decrypt(string encryptedText, string passPhrase)
    {
        // Read string as bytes.
        var encryptedBytes = Convert.FromBase64String(encryptedText);

        // Decrypt bytes.
        var plainBytes = Decrypt(encryptedBytes, passPhrase);

        // Convert to string and return.
        return Encoding.UTF8.GetString(plainBytes);
    }

    /// <summary>Decrypts the <paramref name="encryptedBytesWithSaltAndIv"/>.</summary>
    /// <param name="encryptedBytesWithSaltAndIv">The encrypted contents.</param>
    /// <param name="passPhrase">The pass phrase with which to decrypt the contents.</param>
    /// <returns>An array of <see cref="byte"/> values with the decrypted contents.</returns>
    public static byte[] Decrypt(byte[] encryptedBytesWithSaltAndIv, string passPhrase)
    {
        // Get the salt bytes by extracting the first (SaltSize / 8) bytes.
        var saltBytes = encryptedBytesWithSaltAndIv
            .Take(SaltSize / 8)
            .ToArray();

        // Get the initialisation vector bytes by extracting the next (IvSize / 8) bytes after the salt.
        var ivBytes = encryptedBytesWithSaltAndIv
            .Skip(SaltSize / 8)
            .Take(IvSize / 8)
            .ToArray();

        // Get the actual encrypted bytes by removing the salt and iv bytes.
        var encryptedBytes = encryptedBytesWithSaltAndIv
            .Skip((SaltSize / 8) + (IvSize / 8))
            .Take(encryptedBytesWithSaltAndIv.Length - ((SaltSize / 8) + (IvSize / 8)))
            .ToArray();

        // Prepare store for decrypted string and bytes read.
        byte[] plainTextBytes;
        int decryptedByteCount;

        using (var password = new Rfc2898DeriveBytes(passPhrase, saltBytes, DerivationIterations))
        {
            var keyBytes = password.GetBytes(KeySize / 8);

            using var symmetricKey = new AesManaged();
            symmetricKey.BlockSize = BlockSize;
            symmetricKey.Mode = CipherMode.CBC;
            symmetricKey.Padding = PaddingMode.PKCS7;

            using var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivBytes);
            using var memoryStream = new MemoryStream(encryptedBytes);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            var decryptedBytes = new byte[encryptedBytes.Length];
            var bytesDecrypted = cryptoStream.Read(decryptedBytes, 0, decryptedBytes.Length);

            plainTextBytes = decryptedBytes;
            decryptedByteCount = bytesDecrypted;
        }

        return plainTextBytes.Take(decryptedByteCount).ToArray();
    }

    private static byte[] GenerateRandomEntropy(int bitCount)
    {
        return CryptoUtilities.GenerateRandomBytes(bitCount / 8);
    }
}
