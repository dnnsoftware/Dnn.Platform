namespace DotNetNuke.Services.FileSystem.EventArgs
{
    public class FileMovedEventArgs : FileChangedEventArgs
    {
        public string OldFilePath { get; set; }
    }
}
