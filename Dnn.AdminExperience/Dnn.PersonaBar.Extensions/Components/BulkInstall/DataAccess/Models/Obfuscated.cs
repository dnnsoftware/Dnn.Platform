// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Components.BulkInstall.DataAccess.Models
{
    using DotNetNuke.BulkInstall.Encryption;

    /// <summary>A base class for entities with obfuscated content.</summary>
    public class Obfuscated
    {
        /// <summary>Initializes a new instance of the <see cref="Obfuscated"/> class.</summary>
        protected Obfuscated()
        {
        }

        /// <summary>Generates a hash.</summary>
        /// <param name="value">The value to hash.</param>
        /// <param name="salt">The salt.</param>
        /// <returns>The hashed value.</returns>
        internal static string GenerateHash(string value, string salt)
        {
            // Hash.
            string hash = CryptoUtilities.SHA256HashString(value + salt);

            // Return upper case.
            return hash.ToUpperInvariant();
        }

        /// <summary>Generate a salt.</summary>
        /// <returns>The salt.</returns>
        internal static string GenerateSalt()
        {
            // Salt length of 16 bytes should be fine for now.
            int saltLength = 16;

            // Generate random bytes.
            byte[] bytes = CryptoUtilities.GenerateRandomBytes(saltLength);

            // Convert to string.
            string salt = string.Empty;

            for (int i = 0; i < bytes.Length; i++)
            {
                salt = $"{salt}{bytes[i]:X2}";
            }

            // Return upper case.
            return salt.ToUpperInvariant();
        }
    }
}
