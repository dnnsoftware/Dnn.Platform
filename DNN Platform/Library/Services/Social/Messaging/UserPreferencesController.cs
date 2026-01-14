// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Social.Messaging
{
    using System;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Social.Messaging.Data;

    public class UserPreferencesController : ServiceLocator<IUserPreferencesController, UserPreferencesController>, IUserPreferencesController
    {
        private readonly IDataService dataService;

        /// <summary>Initializes a new instance of the <see cref="UserPreferencesController"/> class.</summary>
        public UserPreferencesController()
            : this(DataService.Instance)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="UserPreferencesController"/> class.</summary>
        /// <param name="dataService">The data service.</param>
        public UserPreferencesController(IDataService dataService)
        {
            // Argument Contract
            Requires.NotNull("dataService", dataService);

            this.dataService = dataService;
        }

        /// <inheritdoc/>
        public void SetUserPreference(UserPreference userPreference)
        {
            this.dataService.SetUserPreference(userPreference.PortalId, userPreference.UserId, (int)userPreference.MessagesEmailFrequency, (int)userPreference.NotificationsEmailFrequency);
        }

        /// <inheritdoc/>
        public UserPreference GetUserPreference(UserInfo userinfo)
        {
            return CBO.FillObject<UserPreference>(this.dataService.GetUserPreference(userinfo.PortalID, userinfo.UserID));
        }

        /// <inheritdoc/>
        protected override Func<IUserPreferencesController> GetFactory()
        {
            return () => new UserPreferencesController();
        }
    }
}
