// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using Dnn.PersonaBar.Users.Components.Dto;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Framework;

namespace Dnn.PersonaBar.Users.Data
{
    public class UsersDataService : ServiceLocator<IUsersDataService, UsersDataService>, IUsersDataService
    {
        #region Overrides of ServiceLocator

        protected override Func<IUsersDataService> GetFactory()
        {
            return () => new UsersDataService();
        }

        #endregion

        public IList<UserBasicDto> GetUsersByUserIds(int portalId, string userIds)
        {
            return CBO.FillCollection<UserBasicDto>(DotNetNuke.Data.DataProvider.Instance()
                .ExecuteReader("Personabar_GetUsersByUserIds",
                    portalId, userIds));
        }
    }
}
