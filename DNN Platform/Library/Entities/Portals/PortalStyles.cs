// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Portals
{
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Entities.Modules.Settings;

    /// <summary>
    /// Provides css custom properties to customize the Dnn UI in the platform, themes and modules for an overall consistent look.
    /// </summary>
    public class PortalStyles : IPortalStyles
    {
        /// <inheritdoc/>
        [PortalSetting]
        public bool AllowAdminEdits { get; set; } = false;

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorPrimary { get; set; } = "00A5E0";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorPrimaryLight { get; set; } = "1AAEE3";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorPrimaryDark { get; set; } = "0091C5";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorPrimaryContrast { get; set; } = "FFFFFF";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorSecondary { get; set; } = "ED3D46";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorSecondaryLight { get; set; } = "EF5059";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorSecondaryDark { get; set; } = "D1363E";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorSecondaryContrast { get; set; } = "FFFFFF";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorTertiary { get; set; } = "0E2936";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorTertiaryLight { get; set; } = "3C7A9A";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorTertiaryDark { get; set; } = "0B1C24";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorTertiaryContrast { get; set; } = "FFFFFF";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorNeutral { get; set; } = "EDEDED";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorNeutralLight { get; set; } = "FFFFFF";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorNeutralDark { get; set; } = "999999";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorNeutralContrast { get; set; } = "000000";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorBackground { get; set; } = "FFFFFF";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorBackgroundLight { get; set; } = "F5F5F5";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorBackgroundDark { get; set; } = "CCCCCC";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorBackgroundContrast { get; set; } = "000000";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorForeground { get; set; } = "000000";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorForegroundLight { get; set; } = "333333";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorForegroundDark { get; set; } = "000000";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorForegroundContrast { get; set; } = "FFFFFF";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorInfo { get; set; } = "17A2B8";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorInfoLight { get; set; } = "23B8CF";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorInfoDark { get; set; } = "00889E";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorInfoContrast { get; set; } = "FFFFFF";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorSuccess { get; set; } = "28A745";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorSuccessLight { get; set; } = "49C25D";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorSuccessDark { get; set; } = "00902F";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorSuccessContrast { get; set; } = "FFFFFF";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorWarning { get; set; } = "FFC107";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorWarningLight { get; set; } = "FFD42E";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorWarningDark { get; set; } = "E9AD00";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorWarningContrast { get; set; } = "FFFFFF";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorDanger { get; set; } = "DC3545";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorDangerLight { get; set; } = "F14954";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorDangerDark { get; set; } = "C51535";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorDangerContrast { get; set; } = "FFFFFF";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorSurface { get; set; } = "EEEEEE";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorSurfaceLight { get; set; } = "F5F5F5";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorSurfaceDark { get; set; } = "CCCCCC";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorSurfaceContrast { get; set; } = "000000";

        /// <inheritdoc/>
        [PortalSetting]
        public double ControlsRadius { get; set; } = 0;

        /// <inheritdoc/>
        [PortalSetting]
        public double ControlsPadding { get; set; } = 9;

        /// <inheritdoc/>
        [PortalSetting]
        public double BaseFontSize { get; set; } = 16;

        /// <inheritdoc/>
        [PortalSetting]
        public double VariationOpacity { get; set; } = 0.8;
    }
}
