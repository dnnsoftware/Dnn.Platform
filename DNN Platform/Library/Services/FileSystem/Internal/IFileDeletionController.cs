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
        /// Unlinks the specified file
        /// </summary>
        /// <param name="file">The file to unlink</param>
        void UnlinkFile(IFileInfo file);

        /// <summary>
        /// Deletes the specified file metadata.
        /// </summary>
        /// <param name="file">The file to delete its metadata.</param>
        void DeleteFileData(IFileInfo file);
    }    
}
