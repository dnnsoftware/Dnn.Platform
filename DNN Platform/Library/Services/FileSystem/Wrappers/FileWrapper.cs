// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem.Internal
{
    using System;
    using System.IO;

    using DotNetNuke.ComponentModel;

    public class FileWrapper : ComponentBase<IFile, FileWrapper>, IFile
    {
        public Stream Create(string path)
        {
            EnsureFileFolderExists(path);
            return File.Create(path);
        }

        public void Delete(string path)
        {
            File.Delete(path);
        }

        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        public FileAttributes GetAttributes(string path)
        {
            return File.GetAttributes(path);
        }

        public DateTime GetLastWriteTime(string path)
        {
            return File.GetLastWriteTime(path);
        }

        public void Move(string sourceFileName, string destFileName)
        {
            EnsureFileFolderExists(destFileName);
            File.Move(sourceFileName, destFileName);
        }

        public void Copy(string sourceFileName, string destinationFileName, bool overwrite)
        {
            EnsureFileFolderExists(destinationFileName);
            File.Copy(sourceFileName, destinationFileName, overwrite);
        }

        public Stream OpenRead(string path)
        {
            return File.OpenRead(path);
        }

        public byte[] ReadAllBytes(string path)
        {
            return File.ReadAllBytes(path);
        }

        public void SetAttributes(string path, FileAttributes fileAttributes)
        {
            File.SetAttributes(path, fileAttributes);
        }

        private static void EnsureFileFolderExists(string filePath)
        {
            var fi = new System.IO.FileInfo(filePath);
            if (fi.Directory != null && !fi.Directory.Exists)
            {
                fi.Directory.Create();
            }
        }
    }
}
