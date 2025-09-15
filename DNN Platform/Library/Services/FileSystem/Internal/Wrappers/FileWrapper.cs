// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem.Internal
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using DotNetNuke.ComponentModel;

    using SchwabenCode.QuickIO;

    /// <summary>The default <see cref="IFile"/> implementation.</summary>
    public class FileWrapper : ComponentBase<IFile, FileWrapper>, IFile
    {
        /// <inheritdoc/>
        public Stream Create(string path)
        {
            EnsureFileFolderExists(path);
            return File.Create(path);
        }

        /// <inheritdoc/>
        public void Delete(string path)
        {
            File.Delete(path);
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(string path)
        {
            await QuickIOFile.DeleteAsync(path);
        }

        /// <inheritdoc/>
        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(string path)
        {
            return await QuickIOFile.ExistsAsync(path);
        }

        /// <inheritdoc/>
        public FileAttributes GetAttributes(string path)
        {
            return File.GetAttributes(path);
        }

        /// <inheritdoc/>
        public async Task<FileAttributes> GetAttributesAsync(string path)
        {
            return await QuickIOFile.GetAttributesAsync(path);
        }

        /// <inheritdoc/>
        public DateTime GetLastWriteTime(string path)
        {
            return File.GetLastWriteTime(path);
        }

        /// <inheritdoc/>
        public async Task<DateTime> GetLastWriteTimeAsync(string path)
        {
            return await QuickIOFile.GetLastWriteTimeAsync(path);
        }

        /// <inheritdoc/>
        public void Move(string sourceFileName, string destFileName)
        {
            EnsureFileFolderExists(destFileName);
            File.Move(sourceFileName, destFileName);
        }

        /// <inheritdoc/>
        public async Task MoveAsync(string sourceFileName, string destFileName)
        {
            await EnsureFileFolderExistsAsync(destFileName);
            await QuickIOFile.MoveAsync(sourceFileName, destFileName);
        }

        /// <inheritdoc/>
        public void Copy(string sourceFileName, string destinationFileName, bool overwrite)
        {
            EnsureFileFolderExists(destinationFileName);
            File.Copy(sourceFileName, destinationFileName, overwrite);
        }

        /// <inheritdoc/>
        public async Task CopyAsync(string sourceFileName, string destinationFileName, bool overwrite)
        {
            await EnsureFileFolderExistsAsync(destinationFileName);
            await QuickIOFile.CopyAsync(sourceFileName, destinationFileName, overwrite);
        }

        /// <inheritdoc/>
        public Stream OpenRead(string path)
        {
            return File.OpenRead(path);
        }

        /// <inheritdoc/>
        public byte[] ReadAllBytes(string path)
        {
            return File.ReadAllBytes(path);
        }

        /// <inheritdoc/>
        public async Task<byte[]> ReadAllBytesAsync(string path)
        {
            return await new QuickIOFileInfo(path).ReadAllBytesAsync();
        }

        /// <inheritdoc/>
        public void SetAttributes(string path, FileAttributes fileAttributes)
        {
            File.SetAttributes(path, fileAttributes);
        }

        /// <inheritdoc/>
        public async Task SetAttributesAsync(string path, FileAttributes fileAttributes)
        {
            await QuickIOFile.SetAttributesAsync(path, fileAttributes);
        }

        private static void EnsureFileFolderExists(string filePath)
        {
            var fi = new System.IO.FileInfo(filePath);
            if (fi.Directory != null && !fi.Directory.Exists)
            {
                fi.Directory.Create();
            }
        }

        private static async Task EnsureFileFolderExistsAsync(string filePath)
        {
            var directoryName = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directoryName) && !await QuickIODirectory.ExistsAsync(directoryName))
            {
                await QuickIODirectory.CreateAsync(directoryName);
            }
        }
    }
}
