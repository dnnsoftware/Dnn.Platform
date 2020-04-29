using DotNetNuke.Services.Localization;
using Dnn.Modules.ResourceManager.Components;

namespace Dnn.Modules.ResourceManager.Helpers
{
    internal class LocalizationHelper
    {
        private const string ResourceFile = "~/" + Constants.ModulePath + "/App_LocalResources/ResourceManager.resx";

        public static string GetString(string key)
        {
            return Localization.GetString(key, ResourceFile);
        }
    }
}