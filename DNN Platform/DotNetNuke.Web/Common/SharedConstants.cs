// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Common
{
    using DotNetNuke.Services.Localization;

    /// <summary>Common localized text.</summary>
    public static class SharedConstants
    {
        /// <summary>The localized text to display for the root folder of a portal's files.</summary>
        public static readonly string RootFolder;

        /// <summary>The localized text to display for the root folder of the host-level files.</summary>
        public static readonly string HostRootFolder;

        /// <summary>The localized text to display when no item is selected.</summary>
        public static readonly string Unspecified;

        static SharedConstants()
        {
            RootFolder = Localization.GetString("RootFolder.Name", Localization.SharedResourceFile);
            HostRootFolder = Localization.GetString("HostRootFolder.Name", Localization.SharedResourceFile);
            Unspecified = "<" + Localization.GetString("None_Specified", Localization.SharedResourceFile) + ">";
        }
    }
}
