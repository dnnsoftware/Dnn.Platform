#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using Dnn.PersonaBar.Library.DTO;

namespace Dnn.PersonaBar.Library.Controllers
{
    public interface IPersonaBarUserSettingsController
    {
        /// <summary>
        /// Updates the PersonaBar user settings
        /// </summary>
        void UpdatePersonaBarUserSettings(UserSettings settings, int userId, int portalId);

        /// <summary>
        /// Gets the PersonaBar user settings
        /// </summary>
        UserSettings GetPersonaBarUserSettings();
    }
}