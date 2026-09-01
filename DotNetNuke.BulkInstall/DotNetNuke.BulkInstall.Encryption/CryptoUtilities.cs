// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.BulkInstall.Encryption;

using System.Security.Cryptography;
using System.Text;

/// <summary>Provides useful utility methods which may not easily be grouped elsewhere.</summary>
public static class CryptoUtilities
{
    /// <summary>Generates a byte array of the length specified filled with random bytes.</summary>
    /// <param name="length">The number of bytes.</param>
    /// <returns>A new array of <see cref="byte"/> values.</returns>
    public static byte[] GenerateRandomBytes(int length)
    {
        // Create a new byte array of the size required.
        var bytes = new byte[length];

        using var rngCsp = new RNGCryptoServiceProvider();

        // Fill it with random bytes.
        rngCsp.GetBytes(bytes);

        return bytes;
    }

    /// <summary>Hashes the passed value using the SHA256 algorithm.</summary>
    /// <param name="value">The value to hash.</param>
    /// <returns>The hashed value as a hex string.</returns>
    public static string SHA256HashString(string value)
    {
        var bytes = SHA256HashBytes(value);

        return bytes.Aggregate(string.Empty, (current, part) => $"{current}{part:X2}");
    }

    /// <summary>Hashes the passed value using the SHA256 algorithm.</summary>
    /// <param name="value">The value to hash.</param>
    /// <returns>The hashed value.</returns>
    public static byte[] SHA256HashBytes(string value)
    {
        // Convert string to byte array.
        var bytes = Encoding.UTF8.GetBytes(value);

        using SHA256 sha = new SHA256Managed();

        // Hash bytes.
        return sha.ComputeHash(bytes);
    }
}
