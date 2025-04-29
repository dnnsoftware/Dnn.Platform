// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Cryptography;

using DotNetNuke.ComponentModel;

public abstract class CryptographyProvider : ICryptographyProvider
{
    public static CryptographyProvider Instance()
    {
        return ComponentFactory.GetComponent<CryptographyProvider>();
    }

    /// <inheritdoc />
    public abstract string EncryptParameter(string message, string passphrase);

    /// <inheritdoc />
    public abstract string DecryptParameter(string message, string passphrase);

    /// <inheritdoc />
    public abstract string EncryptString(string message, string passphrase);

    /// <inheritdoc />
    public abstract string DecryptString(string message, string passphrase);
}
