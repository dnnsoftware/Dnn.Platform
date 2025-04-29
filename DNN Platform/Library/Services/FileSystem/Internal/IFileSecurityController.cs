// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.FileSystem.Internal;

using System.IO;

/// <summary>Internal class to check file security.</summary>
public interface IFileSecurityController
{
    /// <summary>Checks if the file has valid content.</summary>
    /// <param name="fileName">The File Name.</param>
    /// <param name="fileContent">The File Content.</param>
    /// <returns><see langword="true"/> if the file has valid content, otherwise <see langword="false"/>.</returns>
    bool Validate(string fileName, Stream fileContent);

    /// <summary>
    /// Checks the file content isn't an exectuable file.
    /// </summary>
    /// <param name="fileContent">The File Content.</param>
    /// <returns>Whether the file is an exectuable file.</returns>
    bool ValidateNotExectuable(Stream fileContent);
}
