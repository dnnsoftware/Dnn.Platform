using System;
using System.Collections.Generic;
using System.Net;
using Dnn.PersonaBar.Users.Components.Contracts;
using Dnn.PersonaBar.Users.Components.Dto;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Users.Components
{
    public interface IUsersController
    {
        IEnumerable<UserBasicDto> GetUsers(GetUsersContract usersContract, bool isSuperUser, out int totalRecords);
        IEnumerable<KeyValuePair<string, int>> GetUserFilters(bool isSuperUser = false);
        UserDetailDto GetUserDetail(int portalId, int userId);
        bool ChangePassword(int portalId, int userId, string newPassword);
        UserBasicDto UpdateUserBasicInfo(UserBasicDto userBasicDto, int portalId = -1);

        UserRoleDto SaveUserRole(int portalId, UserInfo currentUserInfo, UserRoleDto userRoleDto, bool notifyUser,
            bool isOwner);

        void AddUserToRoles(UserInfo currentUserInfo, int userId, int portalId, string roleNames, string roleDelimiter = ",",
            DateTime? effectiveDate = null, DateTime? expiryDate = null);

        IList<UserRoleInfo> GetUserRoles(UserInfo user, string keyword, out int total, int pageIndex = -1, int pageSize = -1);

        IEnumerable<UserInfo> GetUsersInRole(PortalSettings portalSettings, string roleName, out int total,
            out KeyValuePair<HttpStatusCode, string> message, int pageIndex = -1, int pageSize = -1);

        bool ForceChangePassword(UserInfo userInfo, int portalId, bool notify);

        void UpdateAuthorizeStatus(UserInfo userInfo, int portalId, bool authorized);
    }
}