namespace DotNetNuke.Services.FileSystem.EventArgs
{
    public class FileDeletedEventArgs : FileChangedEventArgs
    {
        public bool IsCascadeDeleting { get; set; }
    }
}
