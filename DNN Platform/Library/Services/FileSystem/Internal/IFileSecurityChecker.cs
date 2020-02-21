// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.IO;

namespace DotNetNuke.Services.FileSystem.Internal
{
    /// <summary>
    /// File Content Security Checker.
    /// </summary>
    public interface IFileSecurityChecker
    {
        /// <summary>
        /// Checks if the file has valid content.
        /// </summary>
        /// <param name="fileContent">The File Content.</param>
        bool Validate(Stream fileContent);
    }
}
