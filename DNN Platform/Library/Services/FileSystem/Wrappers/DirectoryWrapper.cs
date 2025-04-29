// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem.Internal;

using System.IO;

using DotNetNuke.ComponentModel;

public class DirectoryWrapper : ComponentBase<IDirectory, DirectoryWrapper>, IDirectory
{
    /// <inheritdoc/>
    public void Delete(string path, bool recursive)
    {
        if (this.Exists(path))
        {
            Directory.Delete(path, recursive);
        }
    }

    /// <inheritdoc/>
    public bool Exists(string path)
    {
        return Directory.Exists(path);
    }

    /// <inheritdoc/>
    public string[] GetDirectories(string path)
    {
        return Directory.GetDirectories(path);
    }

    /// <inheritdoc/>
    public string[] GetFiles(string path)
    {
        return Directory.GetFiles(path);
    }

    /// <inheritdoc/>
    public void Move(string sourceDirName, string destDirName)
    {
        Directory.Move(sourceDirName, destDirName);
    }

    /// <inheritdoc/>
    public void CreateDirectory(string path)
    {
        Directory.CreateDirectory(path);
    }
}
