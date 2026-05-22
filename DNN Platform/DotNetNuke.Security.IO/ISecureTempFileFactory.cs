// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.IO;

/// <summary>
/// Factory that produces <see cref="SecureTempFile"/> instances. Replaces direct calls to
/// <see cref="System.IO.Path.GetTempFileName"/>, which is flagged by SonarQube rule
/// <c>csharpsquid:S5445</c> as an insecure temporary-file creation API.
/// </summary>
public interface ISecureTempFileFactory
{
    /// <summary>Creates a new, exclusively-owned temporary file with a cryptographically
    /// random name in the OS temp directory. The returned instance MUST be disposed by the
    /// caller; on disposal the underlying file is automatically deleted.</summary>
    /// <returns>A freshly-created <see cref="SecureTempFile"/> in the open state.</returns>
    SecureTempFile Create();
}
