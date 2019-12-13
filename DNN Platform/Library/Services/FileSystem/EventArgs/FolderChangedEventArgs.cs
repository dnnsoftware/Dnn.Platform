namespace DotNetNuke.Services.FileSystem.EventArgs
{
    public class FolderChangedEventArgs : System.EventArgs
    {
        public IFolderInfo FolderInfo { get; set; }
        public int UserId { get; set; }
    }
}
