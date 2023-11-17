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
        public StyleColor ColorPrimary { get; set; } = new StyleColor("3792ED");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorPrimaryLight { get; set; } = new StyleColor("6CB6F3");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorPrimaryDark { get; set; } = new StyleColor("0D569E");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorPrimaryContrast { get; set; } = new StyleColor("FFFFFF");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorSecondary { get; set; } = new StyleColor("CCCCCC");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorSecondaryLight { get; set; } = new StyleColor("EEEEEE");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorSecondaryDark { get; set; } = new StyleColor("AAAAAA");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorSecondaryContrast { get; set; } = new StyleColor("222222");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorTertiary { get; set; } = new StyleColor("EAEAEA");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorTertiaryLight { get; set; } = new StyleColor("F2F2F2");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorTertiaryDark { get; set; } = new StyleColor("D8D8D8");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorTertiaryContrast { get; set; } = new StyleColor("333333");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorNeutral { get; set; } = new StyleColor("B2B2B2");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorNeutralLight { get; set; } = new StyleColor("E5E5E5");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorNeutralDark { get; set; } = new StyleColor("999999");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorNeutralContrast { get; set; } = new StyleColor("000000");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorBackground { get; set; } = new StyleColor("FFFFFF");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorBackgroundLight { get; set; } = new StyleColor("FFFFFF");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorBackgroundDark { get; set; } = new StyleColor("999999");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorBackgroundContrast { get; set; } = new StyleColor("000000");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorForeground { get; set; } = new StyleColor("000000");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorForegroundLight { get; set; } = new StyleColor("333333");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorForegroundDark { get; set; } = new StyleColor("000000");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorForegroundContrast { get; set; } = new StyleColor("FFFFFF");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorInfo { get; set; } = new StyleColor("17A2B8");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorInfoLight { get; set; } = new StyleColor("23B8CF");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorInfoDark { get; set; } = new StyleColor("00889E");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorInfoContrast { get; set; } = new StyleColor("FFFFFF");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorSuccess { get; set; } = new StyleColor("28A745");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorSuccessLight { get; set; } = new StyleColor("49C25D");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorSuccessDark { get; set; } = new StyleColor("00902F");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorSuccessContrast { get; set; } = new StyleColor("FFFFFF");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorWarning { get; set; } = new StyleColor("FFC107");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorWarningLight { get; set; } = new StyleColor("FFD42E");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorWarningDark { get; set; } = new StyleColor("E9AD00");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorWarningContrast { get; set; } = new StyleColor("FFFFFF");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorDanger { get; set; } = new StyleColor("DC3545");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorDangerLight { get; set; } = new StyleColor("F14954");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorDangerDark { get; set; } = new StyleColor("C51535");

        /// <inheritdoc/>
        [PortalSetting]
        public StyleColor ColorDangerContrast { get; set; } = new StyleColor("FFFFFF");

        /// <inheritdoc/>
        [PortalSetting]
        public int ControlsRadius { get; set; } = 5;

        /// <inheritdoc/>
        [PortalSetting]
        public int ControlsPadding { get; set; } = 5;

        /// <inheritdoc/>
        [PortalSetting]
        public int BaseFontSize { get; set; } = 16;
    }
}
