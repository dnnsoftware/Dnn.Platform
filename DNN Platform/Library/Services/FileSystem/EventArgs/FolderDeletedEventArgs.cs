namespace DotNetNuke.Services.FileSystem.EventArgs
{
    public class FolderDeletedEventArgs : FolderChangedEventArgs
    {
        public bool IsCascadeDeletng { get; set; }
    }
}
