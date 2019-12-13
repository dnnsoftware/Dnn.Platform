using DotNetNuke.Services.Localization;

namespace DotNetNuke.Web.Common
{
    public class DynamicSharedConstants
    {
        public static string RootFolder
        {
            get
            {
                return Localization.GetString("RootFolder.Name", Localization.SharedResourceFile);
            }
        }

        public static string HostRootFolder
        {
            get
            {
                return Localization.GetString("HostRootFolder.Name", Localization.SharedResourceFile);
            }
        }

        public static string Unspecified
        {
            get
            {
                return "<" + Localization.GetString("None_Specified", Localization.SharedResourceFile) + ">";
            }
        }
    }
}
