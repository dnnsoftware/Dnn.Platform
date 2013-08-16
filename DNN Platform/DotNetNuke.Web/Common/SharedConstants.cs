using System;

using DotNetNuke.Services.Localization;

namespace DotNetNuke.Web.Common
{
    public static class SharedConstants
    {
        public static readonly string RootFolder = null;
        public static readonly string Unspecified = null;

        static SharedConstants()
        {
            RootFolder = Localization.GetString("RootFolder.Name", Localization.SharedResourceFile);
            Unspecified = "<" + Localization.GetString("None_Specified", Localization.SharedResourceFile) + ">";
        }
    }
}
