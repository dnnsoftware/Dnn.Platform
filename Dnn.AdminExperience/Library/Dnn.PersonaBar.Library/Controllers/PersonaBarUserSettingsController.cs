// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
            var settings = (UserSettings) Personalization.GetProfile(ContainerName, UserSettingsKey);
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
