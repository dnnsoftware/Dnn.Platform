// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.FileSystem;

/// Project  : DotNetNuke
/// Class    : FolderController
///
/// <summary>
/// Business Class that provides access to the Database for the functions within the calling classes
/// Instantiates the instance of the DataProvider and returns the object, if any.
/// </summary>
public class FolderController
{
    public enum StorageLocationTypes
    {
        InsecureFileSystem = 0,
        SecureFileSystem = 1,
        DatabaseSecure = 2,
    }
}
