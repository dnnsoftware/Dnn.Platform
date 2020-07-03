// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Data
{
    using System;
    using System.Collections.Generic;

    using Dnn.PersonaBar.Users.Components.Dto;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework;

    public class UsersDataService : ServiceLocator<IUsersDataService, UsersDataService>, IUsersDataService
    {
        public IList<UserBasicDto> GetUsersByUserIds(int portalId, string userIds)
        {
            return CBO.FillCollection<UserBasicDto>(DotNetNuke.Data.DataProvider.Instance()
                .ExecuteReader("Personabar_GetUsersByUserIds",
                    portalId, userIds));
        }

        protected override Func<IUsersDataService> GetFactory()
        {
            return () => new UsersDataService();
        }
    }
}
