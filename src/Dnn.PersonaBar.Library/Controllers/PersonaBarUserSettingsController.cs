#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved
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
        public void UpdatePersonaBarUserSettings(UserSettings settings, int userId, int portalId)
        {
            var controller = new PersonalizationController();
            var personalizationInfo = controller.LoadProfile(userId, portalId);
            FixUserSettingsDates(settings);
            Personalization.SetProfile(personalizationInfo, ContainerName, UserSettingsKey, settings);
            controller.SaveProfile(personalizationInfo);
        }

        public UserSettings GetPersonaBarUserSettings()
        {
            var settings = (UserSettings) Personalization.GetProfile(ContainerName, UserSettingsKey);
            FixUserSettingsDates(settings);
            return settings ?? GetDefaultSettings();
        }
        #endregion

        #region private methods

        private static UserSettings GetDefaultSettings()
        {
            return new UserSettings
                {
                    Period = "Week", //TODO Set Default AnalyticPeriod
                    ComparativeTerm = "1 w",
                    ExpandPersonaBar = false,
                    ExpandTasksPane = true,
                    Legends = new string[] {},
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today
                };
        }

        private static void FixUserSettingsDates(UserSettings userSettings)
        {
            if (userSettings != null)
            {
                userSettings.StartDate = FixDate(userSettings.StartDate);
                userSettings.EndDate = FixDate(userSettings.EndDate);
            }
        }

        private static DateTime FixDate(DateTime date)
        {
            if (date == DateTime.MinValue || date == DateTime.MaxValue)
            {
                return DateTime.Today;
            }

            return date;
        }

        #endregion

        protected override Func<IPersonaBarUserSettingsController> GetFactory()
        {
            return () => new PersonaBarUserSettingsController();
        }
    }
}