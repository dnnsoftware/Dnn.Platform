// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Cryptography;

using System.Security.Cryptography;

using DotNetNuke.Internal.SourceGenerators;

/// <summary>A <see cref="CryptographyProvider"/> implementation which avoids MD5 in order to be FIPS compliant.</summary>
#pragma warning disable CS0618 // Type or member is obsolete
internal partial class FipsCompilanceCryptographyProvider : CryptographyProvider, DotNetNuke.Abstractions.Security.ICryptographyProvider
#pragma warning restore CS0618 // Type or member is obsolete
{
    private const string InterfaceEncryptStringHashAlgorithmName = nameof(SHA512);
    private const string InterfaceEncryptStringSymmetricAlgorithmName = nameof(Aes);

    /// <inheritdoc />
    string DotNetNuke.Abstractions.Security.ICryptographyProvider.EncryptParameterAlgorithmName => nameof(Aes);

    /// <inheritdoc />
    string DotNetNuke.Abstractions.Security.ICryptographyProvider.EncryptStringAlgorithmName => $"{InterfaceEncryptStringHashAlgorithmName}|{InterfaceEncryptStringSymmetricAlgorithmName}";

    /// <inheritdoc />
    protected override string EncryptStringHashAlgorithmName => nameof(SHA512);

    /// <inheritdoc />
    protected override string EncryptStringSymmetricAlgorithmName => nameof(TripleDESCryptoServiceProvider);

    /// <inheritdoc />
    [DnnDeprecated(10, 2, 2, "Use DotNetNuke.Abstractions.Security.ICryptographyProvider")]
    public override partial string EncryptParameter(string message, string passphrase)
    {
        using var des = CreateSymmetricAlgorithm(this.EncryptParameterAlgorithmName);
        return EncryptParameter(message, passphrase, des);
    }

    /// <inheritdoc />
    (string EncryptedMessage, string Algorithm) DotNetNuke.Abstractions.Security.ICryptographyProvider.EncryptParameter(string message, string passphrase)
    {
        var algorithmName = ((DotNetNuke.Abstractions.Security.ICryptographyProvider)this).EncryptParameterAlgorithmName;
        using var aes = CreateSymmetricAlgorithm(algorithmName);
        var encryptedMessage = EncryptParameter(message, passphrase, aes);
        return (encryptedMessage, algorithmName);
    }

    /// <inheritdoc />
    [DnnDeprecated(10, 2, 2, "Use DotNetNuke.Abstractions.Security.ICryptographyProvider")]
    public override partial string DecryptParameter(string message, string passphrase)
    {
        return this.DecryptParameter(message, passphrase, this.EncryptParameterAlgorithmName);
    }

    /// <inheritdoc />
    [DnnDeprecated(10, 2, 2, "Use DotNetNuke.Abstractions.Security.ICryptographyProvider")]
    public override partial string EncryptString(string message, string passphrase)
    {
        using var sha512 = CreateHashAlgorithm(this.EncryptStringHashAlgorithmName);
        return EncryptString(
                message,
                passphrase,
                sha512,
                this.EncryptStringHashAlgorithmName,
                key => CreateSymmetricAlgorithm(this.EncryptStringSymmetricAlgorithmName, key, null),
                this.EncryptStringSymmetricAlgorithmName)
            .EncryptedMessage;
    }

    /// <inheritdoc />
    (string EncryptedMessage, string Algorithm, string InitializationVector) DotNetNuke.Abstractions.Security.ICryptographyProvider.EncryptString(string message, string passphrase)
    {
        using var sha512 = CreateHashAlgorithm(InterfaceEncryptStringHashAlgorithmName);
        return EncryptString(
            message,
            passphrase,
            sha512,
            InterfaceEncryptStringHashAlgorithmName,
            key => CreateSymmetricAlgorithm(InterfaceEncryptStringSymmetricAlgorithmName, key, null),
            InterfaceEncryptStringSymmetricAlgorithmName);
    }

    /// <inheritdoc />
    [DnnDeprecated(10, 2, 2, "Use DotNetNuke.Abstractions.Security.ICryptographyProvider")]
    public override partial string DecryptString(string message, string passphrase)
    {
        return this.DecryptString(message, passphrase, this.EncryptStringAlgorithmName, null);
    }
}
