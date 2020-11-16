// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Common
{
    using DotNetNuke.Services.Localization;

    public static class SharedConstants
    {
        public static readonly string RootFolder = null;
        public static readonly string HostRootFolder = null;
        public static readonly string Unspecified = null;

        static SharedConstants()
        {
            RootFolder = Localization.GetString("RootFolder.Name", Localization.SharedResourceFile);
            HostRootFolder = Localization.GetString("HostRootFolder.Name", Localization.SharedResourceFile);
            Unspecified = "<" + Localization.GetString("None_Specified", Localization.SharedResourceFile) + ">";
        }
    }
}
