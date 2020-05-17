// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Services.FileSystem.Internal
{
    public interface IDirectory
    {
        void Delete(string path, bool recursive);
        bool Exists(string path);
        string[] GetDirectories(string path);
        string[] GetFiles(string path);
        void Move(string sourceDirName, string destDirName);
        void CreateDirectory(string path);
    }
}
