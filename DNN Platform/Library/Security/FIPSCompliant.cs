// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    using DotNetNuke.Common;
    using DotNetNuke.Internal.SourceGenerators;

    /// <summary>This class implements a number of methods that can be safely used in a FIPS-140 compliant environment.</summary>
    /// <remarks>
    /// <para>FIPS compliant Algorithms:</para>
    /// <list type="bullet">
    /// <item>
    ///     <term>Hash algorithms</term>
    ///     <description>HMACSHA1</description>
    ///     <description>MACTripleDES</description>
    ///     <description>SHA1CryptoServiceProvider</description>
    ///     <description>SHA256CryptoServiceProvider</description>
    /// </item>
    /// <item>
    ///     <term>Symmetric algorithms (use the same key for encryption and decryption)</term>
    ///     <description>DESCryptoServiceProvider</description>
    ///     <description>TripleDESCryptoServiceProvider</description>
    /// </item>
    /// <item>
    ///     <term>Asymmetric algorithms (use a public key for encryption and a private key for decryption)</term>
    ///     <description>DSACryptoServiceProvider</description>
    ///     <description>RSACryptoServiceProvider</description>
    /// </item>
    /// </list>
    /// </remarks>
    public class FIPSCompliant
    {
        /// <summary>uses the AES FIPS-140 compliant algorithm to encrypt a string.</summary>
        /// <param name="plainText">the text to encrypt.</param>
        /// <param name="passPhrase">the pass phase to do the encryption.</param>
        /// <param name="salt">a salt value to ensure cipher text using the same text/password is different.</param>
        /// <param name="iterations">number of iterations to derive the key (higher is slower but more secure) - optional parameter with a default of 1000.</param>
        /// <returns>The encrypted text.</returns>
        [Obsolete("Deprecated in DotNetNuke 10.2.1. Use overload which takes a HashAlgorithmName. Scheduled removal in v12.0.0.")]
        public static string EncryptAES(string plainText, string passPhrase, string salt, int iterations = 1000)
            => EncryptAES(HashAlgorithmName.SHA1, plainText, passPhrase, salt, iterations);

        /// <summary>uses the AES FIPS-140 compliant algorithm to encrypt a string.</summary>
        /// <param name="hashAlgorithm">the hash algorithm to use to derive the encryption key.</param>
        /// <param name="plainText">the text to encrypt.</param>
        /// <param name="passPhrase">the pass phase to do the encryption.</param>
        /// <param name="salt">a salt value to ensure cipher text using the same text/password is different.</param>
        /// <param name="iterations">number of iterations to derive the key (higher is slower but more secure) - optional parameter with a default of 1000.</param>
        /// <returns>The encrypted text.</returns>
        [SuppressMessage("Microsoft.Security", "CA5379:EnsureKeyDerivationFunctionAlgorithmIsSufficientlyStrong", Justification = "Only for already-encrypted data")]
        public static string EncryptAES(HashAlgorithmName hashAlgorithm, string plainText, string passPhrase, string salt, int iterations = 1000)
        {
            VerifyAesSettings(passPhrase, salt);

            byte[] saltBytes = Encoding.ASCII.GetBytes(salt);
            using var aesProvider = new AesCryptoServiceProvider();
            using var derivedBytes = new Rfc2898DeriveBytes(passPhrase, saltBytes, iterations, hashAlgorithm);
            byte[] derivedKey = derivedBytes.GetBytes(32); // 256 bits
            byte[] derivedInitVector = derivedBytes.GetBytes(16); // 128 bits
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            aesProvider.KeySize = 256;
            aesProvider.Padding = PaddingMode.ISO10126;
            aesProvider.Mode = CipherMode.CBC;

            using var encryptor = aesProvider.CreateEncryptor(derivedKey, derivedInitVector);
            using var memStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();
            byte[] cipherTextBytes = memStream.ToArray();

            memStream.Close();
            cryptoStream.Close();

            return Convert.ToBase64String(cipherTextBytes);
        }

        /// <summary>uses the AES FIPS-140 compliant algorithm to encrypt a string.</summary>
        /// <param name="encryptedText">the text to decrypt.</param>
        /// <param name="passPhrase">the pass phase to do the decryption.</param>
        /// <param name="salt">a salt value to ensure cipher text using the same text/password is different.</param>
        /// <param name="iterations">number of iterations to derive the key (higher is slower but more secure) - optional parameter with a default of 1000.</param>
        /// <returns>The decrypted text.</returns>
        [Obsolete("Deprecated in DotNetNuke 10.2.1. Use overload which takes a HashAlgorithmName. Scheduled removal in v12.0.0.")]
        public static string DecryptAES(string encryptedText, string passPhrase, string salt, int iterations = 1000)
            => DecryptAES(HashAlgorithmName.SHA1, encryptedText, passPhrase, salt, iterations);

        /// <summary>uses the AES FIPS-140 compliant algorithm to encrypt a string.</summary>
        /// <param name="hashAlgorithm">the hash algorithm to use to derive the encryption key.</param>
        /// <param name="encryptedText">the text to decrypt.</param>
        /// <param name="passPhrase">the pass phase to do the decryption.</param>
        /// <param name="salt">a salt value to ensure cipher text using the same text/password is different.</param>
        /// <param name="iterations">number of iterations to derive the key (higher is slower but more secure) - optional parameter with a default of 1000.</param>
        /// <returns>The decrypted text.</returns>
        [SuppressMessage("Microsoft.Security", "CA5379:EnsureKeyDerivationFunctionAlgorithmIsSufficientlyStrong", Justification = "Only for already-encrypted data")]
        public static string DecryptAES(HashAlgorithmName hashAlgorithm, string encryptedText, string passPhrase, string salt, int iterations = 1000)
        {
            VerifyAesSettings(passPhrase, salt);

            byte[] saltBytes = Encoding.ASCII.GetBytes(salt);
            using var aesProvider = new AesCryptoServiceProvider();
            using var derivedBytes = new Rfc2898DeriveBytes(passPhrase, saltBytes, iterations, hashAlgorithm);
            byte[] derivedKey = derivedBytes.GetBytes(32); // 256 bits
            byte[] derivedInitVector = derivedBytes.GetBytes(16); // 128 bits
            byte[] cipherTextBytes = Convert.FromBase64String(encryptedText);

            aesProvider.KeySize = 256;
            aesProvider.Padding = PaddingMode.ISO10126;
            aesProvider.Mode = CipherMode.CBC;

            using var decryptor = aesProvider.CreateDecryptor(derivedKey, derivedInitVector);
            using var memStream = new MemoryStream(cipherTextBytes);
            using var cryptoStream = new CryptoStream(memStream, decryptor, CryptoStreamMode.Read);
            var plainTextBytes = new byte[cipherTextBytes.Length];
            int byteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);

            memStream.Close();
            cryptoStream.Close();
            return Encoding.UTF8.GetString(plainTextBytes, 0, byteCount);
        }

        private static void VerifyAesSettings(string passPhrase, string salt)
        {
            Requires.PropertyNotNull("passPhrase", passPhrase);
            Requires.PropertyNotNull("salt", salt);

            // Throw exception if the password or salt are too short
            if (passPhrase.Length < 8)
            {
                throw new CryptographicException("Passphrase must be at least 8 characters long.");
            }

            if (salt.Length < 8)
            {
                throw new CryptographicException("Salt must be at least 8 characters long.");
            }
        }
    }
}
