// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
