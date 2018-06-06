using System.Collections.Generic;
using System.Net;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using System.Linq;

namespace Dnn.PersonaBar.Users.Components
{
    public class UserControllerWrapper : IUserControllerWrapper
    {
        public UserInfo GetUserById(int portalId, int userId)
        {
            return UserController.GetUserById(portalId, userId);
        }

        public UserInfo GetUser(int userId, PortalSettings portalSettings, UserInfo currentUserInfo, out KeyValuePair<HttpStatusCode, string> response)
        {
            return UsersController.GetUser(userId, portalSettings, currentUserInfo, out response);
        }

        public int? GetUsersByEmail(int portalId, string searchTerm, int pageIndex, int pageSize, ref int recCount, bool includeDeleted, bool isSuperUserOnly)
        {
            return (UserController.GetUsersByEmail(portalId, searchTerm, pageIndex, pageSize, ref recCount, includeDeleted, isSuperUserOnly).ToArray().FirstOrDefault() as UserInfo)?.UserID;
        }

        public int? GetUsersByUserName(int portalId, string searchTerm, int pageIndex, int pageSize, ref int recCount, bool includeDeleted, bool isSuperUserOnly)
        {
            return (UserController.GetUsersByUserName(portalId, searchTerm, pageIndex, pageSize, ref recCount, includeDeleted, isSuperUserOnly).ToArray().FirstOrDefault() as UserInfo)?.UserID;
        }
    }
}