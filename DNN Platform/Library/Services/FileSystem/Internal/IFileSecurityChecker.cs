// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem.Internal
{
    using System.IO;

    /// <summary>
    /// File Content Security Checker.
    /// </summary>
    public interface IFileSecurityChecker
    {
        /// <summary>
        /// Checks if the file has valid content.
        /// </summary>
        /// <param name="fileContent">The File Content.</param>
        /// <returns></returns>
        bool Validate(Stream fileContent);
    }
}
