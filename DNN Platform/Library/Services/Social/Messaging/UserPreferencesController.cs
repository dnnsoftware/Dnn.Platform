// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Services.Social.Messaging.Data;

namespace DotNetNuke.Services.Social.Messaging
{
    public class UserPreferencesController : ServiceLocator<IUserPreferencesController, UserPreferencesController>, IUserPreferencesController
    {
        #region Private Memebers
        private readonly IDataService dataService;
        #endregion

        protected override Func<IUserPreferencesController> GetFactory()
        {
            return () => new UserPreferencesController();
        }

        #region Constructor
        public UserPreferencesController() : this(DataService.Instance)
        {
        }

        public UserPreferencesController(IDataService dataService)
        {
            //Argument Contract
            Requires.NotNull("dataService", dataService);

            this.dataService = dataService;
        }
        #endregion

        #region Public API
        public void SetUserPreference(UserPreference userPreference)
        {
            dataService.SetUserPreference(userPreference.PortalId, userPreference.UserId, Convert.ToInt32(userPreference.MessagesEmailFrequency), Convert.ToInt32(userPreference.NotificationsEmailFrequency));
        }

        public UserPreference GetUserPreference(UserInfo userinfo)
        {
            return CBO.FillObject<UserPreference>(dataService.GetUserPreference(userinfo.PortalID, userinfo.UserID));
        }

        #endregion

    }
}
