using System.Collections.Generic;

namespace DotNetNuke.Tests.Integration.Executers.Dto
{
    public class TabPermissions
    {
        public int TabId { get; set; }

        public IList<Permission> PermissionDefinitions { get; set; }

        public IList<RolePermission> RolePermissions { get; set; }

        public IList<UserPermission> UserPermissions { get; set; }
    }
}
