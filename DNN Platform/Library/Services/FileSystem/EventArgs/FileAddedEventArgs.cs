namespace DotNetNuke.Services.FileSystem.EventArgs
{
    public class FileAddedEventArgs : FileChangedEventArgs
    {
        public IFolderInfo FolderInfo { get; set; }
    }
}
