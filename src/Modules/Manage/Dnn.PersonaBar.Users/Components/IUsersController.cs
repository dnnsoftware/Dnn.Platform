using System.Collections.Generic;
using Dnn.PersonaBar.Users.Components.Contracts;
using Dnn.PersonaBar.Users.Components.Dto;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Users.Components
{
    public interface IUsersController
    {
        IEnumerable<UserBasicDto> GetUsers(GetUsersContract usersContract, bool isSuperUser, out int totalRecords);
        IEnumerable<KeyValuePair<string, int>> GetUserFilters(bool isSuperUser= false);
        UserDetailDto GetUserDetail(int portalId, int userId);
        bool ChangePassword(int portalId, int userId, string newPassword);
        UserBasicDto UpdateUserBasicInfo(UserBasicDto userBasicDto);

        UserRoleDto SaveUserRole(int portalId, UserInfo userInfo, UserRoleDto userRoleDto, bool notifyUser,
            bool isOwner);
    }
}