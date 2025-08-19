// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem.Internal
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public interface IFile
    {
        Stream Create(string path);

        void Delete(string path);

        Task DeleteAsync(string path);

        bool Exists(string path);

        Task<bool> ExistsAsync(string path);

        FileAttributes GetAttributes(string path);

        Task<FileAttributes> GetAttributesAsync(string path);

        DateTime GetLastWriteTime(string path);

        Task<DateTime> GetLastWriteTimeAsync(string path);

        void Move(string sourceFileName, string destFileName);

        Task MoveAsync(string sourceFileName, string destFileName);

        void Copy(string sourceFileName, string destinationFileName, bool overwrite);

        Task CopyAsync(string sourceFileName, string destinationFileName, bool overwrite);

        Stream OpenRead(string path);

        byte[] ReadAllBytes(string path);

        Task<byte[]> ReadAllBytesAsync(string path);

        void SetAttributes(string path, FileAttributes fileAttributes);

        Task SetAttributesAsync(string path, FileAttributes fileAttributes);
    }
}
