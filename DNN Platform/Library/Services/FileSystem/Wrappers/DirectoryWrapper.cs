﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.IO;

using DotNetNuke.ComponentModel;

namespace DotNetNuke.Services.FileSystem.Internal
{
    public class DirectoryWrapper : ComponentBase<IDirectory, DirectoryWrapper>, IDirectory
    {
        public void Delete(string path, bool recursive)
        {
            if (Exists(path))
            {
                Directory.Delete(path, recursive);
            }
        }

        public bool Exists(string path)
        {
            return Directory.Exists(path);
        }

        public string[] GetDirectories(string path)
        {
            return Directory.GetDirectories(path);
        }

        public string[] GetFiles(string path)
        {
            return Directory.GetFiles(path);
        }

        public void Move(string sourceDirName, string destDirName)
        {
            Directory.Move(sourceDirName, destDirName);
        }

        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }
    }
}
