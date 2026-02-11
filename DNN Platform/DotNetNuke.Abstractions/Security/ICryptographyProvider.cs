// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Security;

/// <summary>A contract specifying the ability to encrypt and decrypt strings.</summary>
public interface ICryptographyProvider
{
    /// <summary>Gets the algorithm name used by <see cref="EncryptParameter"/>.</summary>
    string EncryptParameterAlgorithmName { get; }

    /// <summary>Gets the algorithm name used by <see cref="EncryptString"/>.</summary>
    string EncryptStringAlgorithmName { get; }

    /// <summary>simple method that uses basic encryption to safely encode parameters.</summary>
    /// <param name="message">the text to be encrypted (encoded).</param>
    /// <param name="passphrase">the key to perform the encryption.</param>
    /// <returns>A tuple of the encrypted string and the name of the algorithm used.</returns>
    (string EncryptedMessage, string Algorithm) EncryptParameter(string message, string passphrase);

    /// <summary>simple method that uses basic encryption to safely decode parameters.</summary>
    /// <param name="message">the text to be decrypted (decoded).</param>
    /// <param name="passphrase">the key to perform the decryption.</param>
    /// <param name="algorithmName">the name of the algorithm used to encrypt the parameter.</param>
    /// <returns>decrypted string.</returns>
    string DecryptParameter(string message, string passphrase, string algorithmName);

    /// <summary>safely encrypt sensitive data.</summary>
    /// <param name="message">the text to be encrypted.</param>
    /// <param name="passphrase">the key to perform the encryption.</param>
    /// <returns>A tuple of the encrypted string, the name of the algorithm used, and the initialization vector (or <see langword="null"/> if the algorithm doesn't require an IV).</returns>
    (string EncryptedMessage, string Algorithm, string InitializationVector) EncryptString(string message, string passphrase);

    /// <summary>safely decrypt sensitive data.</summary>
    /// <param name="message">the text to be decrypted.</param>
    /// <param name="passphrase">the key to perform the decryption.</param>
    /// <param name="algorithmName">the name of the algorithm used to encrypt the message.</param>
    /// <param name="initializationVector">the initialization vector returned from <see cref="EncryptString"/>.</param>
    /// <returns>decrypted string.</returns>
    string DecryptString(string message, string passphrase, string algorithmName, string initializationVector);
}
