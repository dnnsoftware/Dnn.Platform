// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Cryptography
{
    public interface ICryptographyProvider
    {
        /// <summary>
        ///     simple method that uses basic encryption to safely encode parameters.
        /// </summary>
        /// <param name="message">the text to be encrypted (encoded).</param>
        /// <param name="passphrase">the key to perform the encryption.</param>
        /// <returns>encrypted string.</returns>
        string EncryptParameter(string message, string passphrase);

        /// <summary>
        ///     simple method that uses basic encryption to safely decode parameters.
        /// </summary>
        /// <param name="message">the text to be decrypted (decoded).</param>
        /// <param name="passphrase">the key to perform the decryption.</param>
        /// <returns>decrypted string.</returns>
        string DecryptParameter(string message, string passphrase);

        /// <summary>
        ///     safely encrypt sensitive data.
        /// </summary>
        /// <param name="message">the text to be encrypted.</param>
        /// <param name="passphrase">the key to perform the encryption.</param>
        /// <returns>encrypted string.</returns>
        string EncryptString(string message, string passphrase);

        /// <summary>
        ///     safely decrypt sensitive data.
        /// </summary>
        /// <param name="message">the text to be decrypted.</param>
        /// <param name="passphrase">the key to perform the decryption.</param>
        /// <returns>decrypted string.</returns>
        string DecryptString(string message, string passphrase);
    }
}
