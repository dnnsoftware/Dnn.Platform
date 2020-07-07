// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
