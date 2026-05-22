// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.IO;

/// <summary>Default stateless, thread-safe implementation of
/// <see cref="ISecureTempFileFactory"/>.</summary>
public sealed class SecureTempFileFactory : ISecureTempFileFactory
{
    /// <inheritdoc />
    public SecureTempFile Create() => new SecureTempFile();
}
