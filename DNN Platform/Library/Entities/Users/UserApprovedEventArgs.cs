namespace DotNetNuke.Entities.Users
{
    public class UserApprovedEventArgs : UserEventArgs
    {
        public bool SendNotification { get; set; }
    }
}
