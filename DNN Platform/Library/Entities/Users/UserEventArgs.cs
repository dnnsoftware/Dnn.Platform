using System;

namespace DotNetNuke.Entities.Users
{
    public class UserEventArgs : EventArgs
    {
        public UserInfo User { get; set; }
        public bool SendNotification { get; set; }
    }
}
