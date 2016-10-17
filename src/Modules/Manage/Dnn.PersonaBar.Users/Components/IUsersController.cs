using System.Collections.Generic;
using Dnn.PersonaBar.Users.Components.Contracts;
using Dnn.PersonaBar.Users.Components.Dto;

namespace Dnn.PersonaBar.Users.Components
{
    public interface IUsersController
    {
        IEnumerable<UserBasicDto> GetUsers(GetUsersContract usersContract, out int totalRecords);
        IEnumerable<KeyValuePair<string, int>> GetUserFilters();
        UserDetailDto GetUserDetail(int portalId, int userId);
        bool ChangePassword(int portalId, int userId, string newPassword, out string errorMessage);
    }
}