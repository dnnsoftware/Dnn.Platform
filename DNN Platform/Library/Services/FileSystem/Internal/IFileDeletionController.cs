// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem.Internal
{
    public interface IFileDeletionController
    {
        /// <summary>
        /// Deletes the specified file.
        /// </summary>
        /// <param name="file">The file to delete.</param>
        void DeleteFile(IFileInfo file);

        /// <summary>
        /// Unlinks the specified file.
        /// </summary>
        /// <param name="file">The file to unlink.</param>
        void UnlinkFile(IFileInfo file);

        /// <summary>
        /// Deletes the specified file metadata.
        /// </summary>
        /// <param name="file">The file to delete its metadata.</param>
        void DeleteFileData(IFileInfo file);
    }
}
