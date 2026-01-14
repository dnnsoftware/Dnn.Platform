// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Library.Controllers
{
    using Dnn.PersonaBar.Library.Dto;

    public interface IPersonaBarUserSettingsController
    {
        /// <summary>Updates the PersonaBar user settings.</summary>
        /// <param name="settings">The user settings.</param>
        /// <param name="userId">The user ID.</param>
        /// <param name="portalId">The portal ID.</param>
        void UpdatePersonaBarUserSettings(UserSettings settings, int userId, int portalId);

        /// <summary>Gets the PersonaBar user settings.</summary>
        /// <returns>A <see cref="UserSettings"/> instance representing the user's settings or the default.</returns>
        UserSettings GetPersonaBarUserSettings();
    }
}
