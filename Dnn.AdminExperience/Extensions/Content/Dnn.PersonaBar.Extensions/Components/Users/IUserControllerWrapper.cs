using System.Collections.Generic;
using System.Net;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Users.Components
{
    public interface IUserControllerWrapper
    {
        UserInfo GetUser(int userId, PortalSettings portalSettings, UserInfo currentUserInfo, out KeyValuePair<HttpStatusCode, string> response);
        UserInfo GetUserById(int portalId, int userId);
        int? GetUsersByUserName(int portalId, string searchTerm, int pageIndex, int pageSize, ref int recCount, bool includeDeleted, bool isSuperUserOnly);
        int? GetUsersByEmail(int portalId, string searchTerm, int pageIndex, int pageSize, ref int recCount, bool includeDeleted, bool isSuperUserOnly);
        bool DeleteUserAndClearCache(ref UserInfo userInfo, bool notify, bool deleteAdmin);
    }
}