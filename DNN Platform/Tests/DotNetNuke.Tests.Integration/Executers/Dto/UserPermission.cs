using System.Collections.Generic;

namespace DotNetNuke.Tests.Integration.Executers.Dto
{
    public class UserPermission
    {
        public int UserId { get; set; }

        public string DisplayName { get; set; }

        public IList<Permission> Permissions { get; set; }
    }
}
