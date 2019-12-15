// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
