// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Controllers
{
    using System;

    using Dnn.PersonaBar.Library.Dto;

    using DotNetNuke.Common;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Personalization;

    using Microsoft.Extensions.DependencyInjection;

    public class PersonaBarUserSettingsController : ServiceLocator<IPersonaBarUserSettingsController, PersonaBarUserSettingsController>, IPersonaBarUserSettingsController
    {
        private const string ContainerName = "AdminPersonaBar";
        private const string UserSettingsKey = "UserSettings";
        private readonly PersonalizationController personalizationController;

        /// <summary>Initializes a new instance of the <see cref="PersonaBarUserSettingsController"/> class.</summary>
        public PersonaBarUserSettingsController()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PersonaBarUserSettingsController"/> class.</summary>
        /// <param name="personalizationController">The personalization controller.</param>
        public PersonaBarUserSettingsController(PersonalizationController personalizationController)
        {
            this.personalizationController = personalizationController ?? Globals.GetCurrentServiceProvider().GetRequiredService<PersonalizationController>();
        }

        /// <inheritdoc/>
        public void UpdatePersonaBarUserSettings(UserSettings settings, int userId, int portalId)
        {
            var personalizationInfo = this.personalizationController.LoadProfile(userId, portalId);
            Personalization.SetProfile(personalizationInfo, ContainerName, UserSettingsKey, settings);
            this.personalizationController.SaveProfile(personalizationInfo);
        }

        /// <inheritdoc/>
        public UserSettings GetPersonaBarUserSettings()
        {
            var settings = (UserSettings)Personalization.GetProfile(ContainerName, UserSettingsKey);
            return settings ?? GetDefaultSettings();
        }

        /// <inheritdoc/>
        protected override Func<IPersonaBarUserSettingsController> GetFactory()
        {
            return () => Globals.GetCurrentServiceProvider().GetRequiredService<IPersonaBarUserSettingsController>();
        }

        private static UserSettings GetDefaultSettings()
        {
            return new UserSettings
            {
                ExpandPersonaBar = false,
            };
        }
    }
}
