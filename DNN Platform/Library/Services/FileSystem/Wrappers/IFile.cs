// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem.Internal
{
    using System;
    using System.IO;

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
