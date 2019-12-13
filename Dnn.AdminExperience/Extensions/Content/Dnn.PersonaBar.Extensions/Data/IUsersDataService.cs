using System.Collections.Generic;
using Dnn.PersonaBar.Users.Components.Dto;

namespace Dnn.PersonaBar.Users.Data
{
    public interface IUsersDataService
    {
        /// <summary>
        /// Get Users basic info by UserId
        /// </summary>
        /// <param name="portalId">PortalId</param>
        /// <param name="userIds">Comma separated user Id</param>
        /// <returns>List of UserBasic</returns>
        IList<UserBasicDto> GetUsersByUserIds(int portalId, string userIds);        
    }
}
