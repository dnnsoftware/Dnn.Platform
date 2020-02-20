// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
