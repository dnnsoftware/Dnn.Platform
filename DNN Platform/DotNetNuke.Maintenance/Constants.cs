// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance;

/// <summary>Constant values used for the maintenance project.</summary>
public static class Constants
{
    /// <summary>Key of the Telerik uninstall option in the host settings.</summary>
    public static readonly string TelerikUninstallOptionSettingKey = "telerikUninstallOption";

    /// <summary>Value of the Telerik uninstall option in the host settings to indicate the uninstallation should occur.</summary>
    public static readonly string TelerikUninstallYesValue = "Y";

    /// <summary>Value of the Telerik uninstall option in the host settings to indicate the uninstallation should not occur.</summary>
    public static readonly string TelerikUninstallNoValue = "N";
}
