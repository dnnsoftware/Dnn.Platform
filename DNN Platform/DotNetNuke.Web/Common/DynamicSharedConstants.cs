// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Common
{
    using DotNetNuke.Services.Localization;

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
