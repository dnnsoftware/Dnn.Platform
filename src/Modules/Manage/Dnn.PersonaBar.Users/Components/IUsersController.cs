using System.Collections.Generic;
using Dnn.PersonaBar.Users.Components.Contracts;
using Dnn.PersonaBar.Users.Components.Dto;

namespace Dnn.PersonaBar.Users.Components
{
    public interface IUsersController
    {
        IList<UserBasicDto> GetUsers(GetUsersContract usersContract, out int totalRecords);
        UserDetailDto GetUserDetail(int portalId, int userId);
        bool ChangePassword(int portalId, int userId, string newPassword, out string errorMessage);
    }
}