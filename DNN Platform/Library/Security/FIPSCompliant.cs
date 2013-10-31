#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

#endregion

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using DotNetNuke.Common;

namespace DotNetNuke.Security
{
    /// <summary>
    ///     This class implements a number of methods that can be safely used in a FIPS-140 compliant environment
    ///     FIPS compliant Algorithms:
    ///     Hash algorithms
    ///     HMACSHA1
    ///     MACTripleDES
    ///     SHA1CryptoServiceProvider
    ///     SHA256CryptoServiceProvider
    ///     Symmetric algorithms (use the same key for encryption and decryption)
    ///     DESCryptoServiceProvider
    ///     TripleDESCryptoServiceProvider
    ///     Asymmetric algorithms (use a public key for encryption and a private key for decryption)
    ///     DSACryptoServiceProvider
    ///     RSACryptoServiceProvider
    /// </summary>
    public class FIPSCompliant
    {
        /// <summary>
        /// uses the AES FIPS-140 compliant algorithm to encrypt a string
        /// </summary>
        /// <param name="plainText">the text to encrypt</param>
        /// <param name="passPhrase">the pass phase to do the encryption</param>
        /// <param name="salt">a salt value to ensure ciphertext using the same text/password is different</param>
        /// <param name="iterations">number of iterations to derive the key (higher is slower but more secure) - optional parameter with a default of 1000</param>
        /// <returns></returns>
        public static String EncryptAES(String plainText, String passPhrase, String salt, int iterations = 1000)
        {
            VerifyAesSettings(passPhrase, salt);

            byte[] saltBytes = Encoding.ASCII.GetBytes(salt);
            var aesProvider = new AesCryptoServiceProvider();
            var derivedBytes = new Rfc2898DeriveBytes(passPhrase, saltBytes, iterations);
            Byte[] derivedKey = derivedBytes.GetBytes(32); // 256 bits
            Byte[] derivedInitVector = derivedBytes.GetBytes(16); // 128 bits
            Byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            aesProvider.KeySize = 256;
            aesProvider.Padding = PaddingMode.ISO10126;
            aesProvider.Mode = CipherMode.CBC;

            ICryptoTransform encryptor = aesProvider.CreateEncryptor(derivedKey, derivedInitVector);
            var memStream = new MemoryStream();
            var cryptoStream = new CryptoStream(memStream, encryptor, CryptoStreamMode.Write);

            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();
            Byte[] cipherTextBytes = memStream.ToArray();

            memStream.Close();
            cryptoStream.Close();

            return Convert.ToBase64String(cipherTextBytes);
        }

        private static void VerifyAesSettings(string passPhrase, string salt)
        {
            Requires.NotNull("passPhrase", passPhrase);
            Requires.NotNull("salt", salt);
            // Throw exception if the password or salt are too short  
            if (passPhrase.Length < 8)
                throw new CryptographicException("Passphrase must be at least 8 characters long.");
            if (salt.Length < 8) throw new CryptographicException("Salt must be at least 8 characters long.");
        }

        /// <summary>
        /// uses the AES FIPS-140 compliant algorithm to encrypt a string
        /// </summary>
        /// <param name="encryptedText">the text to decrypt</param>
        /// <param name="passPhrase">the pass phase to do the decryption</param>
        /// <param name="salt">a salt value to ensure ciphertext using the same text/password is different</param>
        /// <param name="iterations">number of iterations to derive the key (higher is slower but more secure) - optional parameter with a default of 1000</param>
        /// <returns></returns>
        public static String DecryptAES(String encryptedText, String passPhrase, String salt, int iterations = 1000)
        {
            VerifyAesSettings(passPhrase, salt);

            Byte[] saltBytes = Encoding.ASCII.GetBytes(salt);
            var aesProvider = new AesCryptoServiceProvider();
            var derivedBytes = new Rfc2898DeriveBytes(passPhrase, saltBytes, iterations);
            Byte[] derivedKey = derivedBytes.GetBytes(32); // 256 bits
            Byte[] derivedInitVector = derivedBytes.GetBytes(16); // 128 bits
            Byte[] cipherTextBytes = Convert.FromBase64String(encryptedText);

            aesProvider.KeySize = 256;
            aesProvider.Padding = PaddingMode.ISO10126;
            aesProvider.Mode = CipherMode.CBC;

            ICryptoTransform decryptor = aesProvider.CreateDecryptor(derivedKey, derivedInitVector);
            var memStream = new MemoryStream(cipherTextBytes);
            var cryptoStream = new CryptoStream(memStream, decryptor, CryptoStreamMode.Read);
            var plainTextBytes = new Byte[cipherTextBytes.Length];
            Int32 byteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);

            memStream.Close();
            cryptoStream.Close();
            return Encoding.UTF8.GetString(plainTextBytes, 0, byteCount);
        }
    }
}