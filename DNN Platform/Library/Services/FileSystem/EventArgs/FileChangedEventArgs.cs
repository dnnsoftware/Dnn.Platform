namespace DotNetNuke.Services.FileSystem.EventArgs
{
    public class FileChangedEventArgs : System.EventArgs
    {
        public IFileInfo FileInfo { get; set; }
        public int UserId { get; set; }
    }
}
