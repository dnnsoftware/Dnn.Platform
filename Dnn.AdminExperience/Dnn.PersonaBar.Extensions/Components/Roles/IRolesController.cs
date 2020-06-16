// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Roles.Components
{
    using System.Collections.Generic;
    using System.Net;

    using Dnn.PersonaBar.Roles.Services.DTO;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security.Roles;

    public interface IRolesController
    {
        bool SaveRole(PortalSettings portalSettings, RoleDto roleDto, bool assignExistUsers, out KeyValuePair<HttpStatusCode, string> message);
        IEnumerable<RoleInfo> GetRoles(PortalSettings portalSettings, int groupId, string keyword, out int total, int startIndex, int pageSize);
        IList<RoleInfo> GetRolesByNames(PortalSettings portalSettings, int groupId, IList<string> rolesFilter);
        RoleInfo GetRole(PortalSettings portalSettings, int roleId);
        string DeleteRole(PortalSettings portalSettings, int roleId, out KeyValuePair<HttpStatusCode, string> message);
    }
}
