// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Data
{
    using System.Collections.Generic;

    using Dnn.PersonaBar.Users.Components.Dto;

    public interface IUsersDataService
    {
        /// <summary>
        /// Get Users basic info by UserId.
        /// </summary>
        /// <param name="portalId">PortalId.</param>
        /// <param name="userIds">Comma separated user Id.</param>
        /// <returns>List of UserBasic.</returns>
        IList<UserBasicDto> GetUsersByUserIds(int portalId, string userIds);
    }
}
