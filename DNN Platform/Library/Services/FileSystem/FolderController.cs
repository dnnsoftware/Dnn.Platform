// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.FileSystem
{
    public class FolderController
    {
        public enum StorageLocationTypes
        {
            /// <summary>Stored via the <see cref="StandardFolderProvider"/>.</summary>
            InsecureFileSystem = 0,

            /// <summary>Stored via the <see cref="SecureFolderProvider"/>.</summary>
            SecureFileSystem = 1,

            /// <summary>Stored via the <see cref="DatabaseFolderProvider"/>.</summary>
            DatabaseSecure = 2,
        }
    }
}
