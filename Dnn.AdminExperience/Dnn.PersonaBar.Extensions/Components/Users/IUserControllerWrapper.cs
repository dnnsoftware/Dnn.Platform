// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Components
{
    using System.Collections.Generic;
    using System.Net;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

    public interface IUserControllerWrapper
    {
        UserInfo GetUser(int userId, PortalSettings portalSettings, UserInfo currentUserInfo, out KeyValuePair<HttpStatusCode, string> response);
        UserInfo GetUserById(int portalId, int userId);
        int? GetUsersByUserName(int portalId, string searchTerm, int pageIndex, int pageSize, ref int recCount, bool includeDeleted, bool isSuperUserOnly);
        int? GetUsersByEmail(int portalId, string searchTerm, int pageIndex, int pageSize, ref int recCount, bool includeDeleted, bool isSuperUserOnly);
        bool DeleteUserAndClearCache(ref UserInfo userInfo, bool notify, bool deleteAdmin);
    }
}
