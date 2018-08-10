using System.Collections.Generic;
using System.Net;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using System.Linq;
using DotNetNuke.Common.Utilities;

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

        public bool DeleteUserAndClearCache(ref UserInfo userInfo, bool notify, bool deleteAdmin)
        {
            bool isDeleted = UserController.DeleteUser(ref userInfo, notify, deleteAdmin);

            if (isDeleted)
            {
                // We must clear User cache or else, when the user is 'removed' (so it can't be restored), you 
                // will not be able to create a new user with the same username -- even though no user with that username
                // exists.
                DataCache.ClearUserCache(userInfo.PortalID, userInfo.Username);
            }

            return isDeleted;
        }
    }
}