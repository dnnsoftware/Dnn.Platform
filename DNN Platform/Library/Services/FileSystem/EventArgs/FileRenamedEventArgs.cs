namespace DotNetNuke.Services.FileSystem.EventArgs
{
    public class FileRenamedEventArgs : FileChangedEventArgs
    {
        public string OldFileName { get; set; }
    }
}
