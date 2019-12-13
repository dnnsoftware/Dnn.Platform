using System;

namespace DotNetNuke.Entities.Users
{
    public class UpdateUserEventArgs : UserEventArgs
    {
        public UserInfo OldUser { get; set; }
    }
}
