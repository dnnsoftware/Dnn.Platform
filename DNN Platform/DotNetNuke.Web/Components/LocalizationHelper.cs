using DotNetNuke.Services.Localization;

namespace DotNetNuke.Web.Components
{
    public class LocalizationHelper
    {
        private const string ResourceFile = "admin/ControlPanel/App_LocalResources/ControlBar";

        public static string GetControlBarString(string key)
        {
            return Localization.GetString(key, ResourceFile);
        }
    }
}
