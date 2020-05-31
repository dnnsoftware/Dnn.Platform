// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
