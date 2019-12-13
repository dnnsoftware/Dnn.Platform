using System;
using DotNetNuke.Entities.Users;

namespace DotNetNuke.Security.Roles
{
    public class RoleEventArgs : EventArgs
    {
        public RoleInfo Role { get; set; }

        public UserInfo User { get; set; }
    }
}
