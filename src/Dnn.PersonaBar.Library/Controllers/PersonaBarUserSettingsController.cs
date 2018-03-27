#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

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
        public void UpdatePersonaBarUserSettings(PersistSettings settings, int userId, int portalId)
        {
            var controller = new PersonalizationController();
            var personalizationInfo = controller.LoadProfile(userId, portalId);
            Personalization.SetProfile(personalizationInfo, ContainerName, UserSettingsKey, settings);
            controller.SaveProfile(personalizationInfo);
        }

        public PersistSettings GetPersonaBarUserSettings()
        {
            var settings = Personalization.GetProfile(ContainerName, UserSettingsKey);
            PersistSettings persistSettings = null;
            //sync the old settings data into new version.
#pragma warning disable 618
            if (settings is UserSettings)
            {
                persistSettings = MigrationUserSettings(settings as UserSettings);
            }
#pragma warning restore 618
            else
            {
                persistSettings = settings as PersistSettings;
            }
            return persistSettings ?? GetDefaultSettings();
        }
        #endregion

        #region private methods

        private static PersistSettings GetDefaultSettings()
        {
            return new PersistSettings
            {
                ExpandPersonaBar = false
            };
        }

#pragma warning disable 618
        private PersistSettings MigrationUserSettings(UserSettings settings)
        {
            var persistSettings = new PersistSettings();
            persistSettings.SetValue("activeIdentifier", settings.ActiveIdentifier);
            persistSettings.SetValue("activePath", settings.ActivePath);
            persistSettings.SetValue("comparativeTerm", settings.ComparativeTerm);
            persistSettings.SetValue("endDate", settings.EndDate);
            persistSettings.SetValue("expandPersonaBar", settings.ExpandPersonaBar);
            persistSettings.SetValue("expandTasksPane", settings.ExpandTasksPane);
            persistSettings.SetValue("legends", settings.Legends);
            persistSettings.SetValue("period", settings.Period);
            persistSettings.SetValue("startDate", settings.StartDate);

            return persistSettings;
        }
#pragma warning restore 618

        #endregion

        protected override Func<IPersonaBarUserSettingsController> GetFactory()
        {
            return () => new PersonaBarUserSettingsController();
        }
    }
}