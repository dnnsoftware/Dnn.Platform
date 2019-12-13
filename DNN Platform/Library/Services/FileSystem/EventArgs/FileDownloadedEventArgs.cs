namespace DotNetNuke.Services.FileSystem.EventArgs
{
    public class FileDownloadedEventArgs : System.EventArgs
    {
        public IFileInfo FileInfo { get; set; }
        public int UserId { get; set; }
    }
}
