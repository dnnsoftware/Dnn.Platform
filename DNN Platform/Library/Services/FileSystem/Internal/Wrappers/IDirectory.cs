// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem.Internal
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    public interface IDirectory
    {
        void Delete(string path, bool recursive);

        Task DeleteAsync(string path, bool recursive);

        bool Exists(string path);

        Task<bool> ExistsAsync(string path);

        string[] GetDirectories(string path);

        Task<IEnumerable<string>> GetDirectoriesAsync(string path);

        string[] GetFiles(string path);

        string[] GetFiles(string path, string searchPattern, SearchOption searchOption);

        Task<IEnumerable<string>> GetFilesAsync(string path);

        Task<IEnumerable<string>> GetFilesAsync(string path, string searchPattern, SearchOption searchOption);

        void Move(string sourceDirName, string destDirName);

        Task MoveAsync(string sourceDirName, string destDirName);

        void CreateDirectory(string path);

        Task CreateDirectoryAsync(string path);

        void CreateDirectory(string path, bool recursive);

        Task CreateDirectoryAsync(string path, bool recursive);
    }
}
