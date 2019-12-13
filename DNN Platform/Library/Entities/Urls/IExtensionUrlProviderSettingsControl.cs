using System.Collections.Generic;

namespace DotNetNuke.Entities.Urls
{
    public interface IExtensionUrlProviderSettingsControl
    {
        ExtensionUrlProviderInfo Provider { get; set; }

        void LoadSettings();

        /// <summary>
        /// Build the Settings Dictionary and return it to the caller to persist to the database
        /// </summary>
        /// <returns></returns>
        Dictionary<string, string> SaveSettings();
    }
}
