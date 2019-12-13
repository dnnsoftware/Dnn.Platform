namespace DotNetNuke.Services.FileSystem.EventArgs
{
    public class FolderRenamedEventArgs : FolderChangedEventArgs
    {
        public string OldFolderName { get; set; }
    }
}
