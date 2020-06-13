// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem
{
    using DotNetNuke.Services.FileSystem.EventArgs;

    public interface IFileEventHandlers
    {
        void FileDeleted(object sender, FileDeletedEventArgs args);

        void FileRenamed(object sender, FileRenamedEventArgs args);

        void FileMoved(object sender, FileMovedEventArgs args);

        void FileAdded(object sender, FileAddedEventArgs args);

        void FileOverwritten(object sender, FileChangedEventArgs args);

        void FileMetadataChanged(object sender, FileChangedEventArgs args);

        void FileDownloaded(object sender, FileDownloadedEventArgs args);

        void FolderAdded(object sender, FolderChangedEventArgs args);

        void FolderMoved(object sender, FolderMovedEventArgs args);

        void FolderRenamed(object sender, FolderRenamedEventArgs args);

        void FolderDeleted(object sender, FolderDeletedEventArgs args);
    }
}
