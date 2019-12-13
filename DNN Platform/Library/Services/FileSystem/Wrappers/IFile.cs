// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.IO;

namespace DotNetNuke.Services.FileSystem.Internal
{
    public interface IFile
    {
        Stream Create(string path);
        void Delete(string path);
        bool Exists(string path);
        FileAttributes GetAttributes(string path);
        DateTime GetLastWriteTime(string path);
        void Move(string sourceFileName, string destFileName);
        void Copy(string sourceFileName, string destinationFileName, bool overwrite);
        Stream OpenRead(string path);
        byte[] ReadAllBytes(string path);
        void SetAttributes(string path, FileAttributes fileAttributes);
    }
}
