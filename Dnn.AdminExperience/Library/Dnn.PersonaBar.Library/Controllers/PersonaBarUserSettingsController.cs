// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using Dnn.PersonaBar.Library.DTO;
using DotNetNuke.Framework;
using DotNetNuke.Services.Personalization;

namespace Dnn.PersonaBar.Library.Controllers
{
    public class PersonaBarUserSettingsController : ServiceLocator<IPersonaBarUserSettingsController, PersonaBarUserSettingsController>, IPersonaBarUserSettingsController
    {
        private const string ContainerName = "AdminPersonaBar";
        private const string UserSettingsKey = "UserSettings";

        #region IUserSettingsController Implementation
        public void UpdatePersonaBarUserSettings(UserSettings settings, int userId, int portalId)
        {
            var controller = new PersonalizationController();
            var personalizationInfo = controller.LoadProfile(userId, portalId);
            Personalization.SetProfile(personalizationInfo, ContainerName, UserSettingsKey, settings);
            controller.SaveProfile(personalizationInfo);
        }

        public UserSettings GetPersonaBarUserSettings()
        {
            var settings = (UserSettings)Personalization.GetProfile(ContainerName, UserSettingsKey);
            return settings ?? GetDefaultSettings();
        }
        #endregion

        #region private methods

        private static UserSettings GetDefaultSettings()
        {
            return new UserSettings
            {
                ExpandPersonaBar = false
            };
        }

        #endregion

        protected override Func<IPersonaBarUserSettingsController> GetFactory()
        {
            return () => new PersonaBarUserSettingsController();
        }
    }
}
