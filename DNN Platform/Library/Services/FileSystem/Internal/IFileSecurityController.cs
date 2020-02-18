// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.IO;

namespace DotNetNuke.Services.FileSystem.Internal
{
    /// <summary>
    /// Internal class to check file security.
    /// </summary>
    public interface IFileSecurityController
    {
        /// <summary>
        /// Checks if the file has valid content.
        /// </summary>
        /// <param name="fileName">The File Name.</param>
        /// <param name="fileContent">The File Content.</param>
        bool Validate(string fileName, Stream fileContent);
    }
}
