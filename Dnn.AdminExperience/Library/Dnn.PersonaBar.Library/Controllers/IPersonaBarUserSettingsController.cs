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
