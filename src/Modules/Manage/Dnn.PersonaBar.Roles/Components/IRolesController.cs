using System.Collections.Generic;
using System.Net;
using Dnn.PersonaBar.Roles.Services.DTO;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Roles;

namespace Dnn.PersonaBar.Roles.Components
{
    public interface IRolesController
    {
        bool SaveRole(PortalSettings portalSettings, RoleDto roleDto, bool assignExistUsers, out KeyValuePair<HttpStatusCode, string> message);
        IEnumerable<RoleInfo> GetRoles(PortalSettings portalSettings, int groupId, string keyword, out int total, int startIndex, int pageSize);
        RoleInfo GetRole(PortalSettings portalSettings, int roleId);
        string DeleteRole(PortalSettings portalSettings, int roleId, out KeyValuePair<HttpStatusCode, string> message);
    }
}
