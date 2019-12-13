using System;

using DotNetNuke.Services.Localization;

namespace DotNetNuke.Modules.DigitalAssets.Components.ExtensionPoint
{
    public class LocalizationHelper
    {
        private const string ResourceFile = "DesktopModules/DigitalAssets/App_LocalResources/SharedResources";

        public static string GetString(string key)
        {
            return Localization.GetString(key, ResourceFile);
        }
    }
}
