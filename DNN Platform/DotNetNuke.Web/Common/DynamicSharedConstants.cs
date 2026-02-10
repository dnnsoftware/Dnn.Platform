// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Common
{
    using DotNetNuke.Services.Localization;

    /// <summary>Common localized values.</summary>
    public class DynamicSharedConstants
    {
        /// <summary>Gets the friendly name of the site root folder.</summary>
        public static string RootFolder => Localization.GetString("RootFolder.Name", Localization.SharedResourceFile);

        /// <summary>Gets the friendly name of the host root folder.</summary>
        public static string HostRootFolder => Localization.GetString("HostRootFolder.Name", Localization.SharedResourceFile);

        /// <summary>Gets the friendly name an unspecified option.</summary>
        public static string Unspecified => $"<{Localization.GetString("None_Specified", Localization.SharedResourceFile)}>";
    }
}
