using DotNetNuke.Services.FileSystem.EventArgs;

namespace DotNetNuke.Services.FileSystem
{
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
