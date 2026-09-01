// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.BulkInstall.DeployClient;

using System.IO;
using System.Threading.Tasks;

/// <summary>A contact specifying the ability to encrypt a <see cref="Stream"/> of content.</summary>
public interface IEncryptor
{
    /// <summary>Encrypts the contents of the <paramref name="packageStream"/>.</summary>
    /// <param name="options">The input options.</param>
    /// <param name="packageStream">The stream to encrypt.</param>
    /// <returns>A <see cref="Task{TResult}"/> which resolves to a <see cref="Stream"/> containing the encrypted contents.</returns>
    Task<Stream> GetEncryptedStream(DeployInput options, Stream packageStream);
}
