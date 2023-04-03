// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.BulkInstall.DeployClient;

using System.IO;
using System.Threading.Tasks;

using DotNetNuke.BulkInstall.Encryption;

/// <summary>The <see cref="IEncryptor"/> implementation, using <see cref="Crypto.Encrypt(System.IO.Stream,string)"/>.</summary>
public class Encryptor : IEncryptor
{
    /// <inheritdoc/>
    public Task<Stream> GetEncryptedStream(DeployInput options, Stream packageStream)
    {
        return Task.FromResult(Crypto.Encrypt(packageStream, options.EncryptionKey));
    }
}
