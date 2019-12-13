namespace DotNetNuke.Services.FileSystem.EventArgs
{
    public class FolderMovedEventArgs : FolderChangedEventArgs
    {
        public string OldFolderPath { get; set; }
    }
}
