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
        public string CacheKey => "Dnn_Css_Custom_Properties_{0}";

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorPrimary { get; set; } = new StyleColor("3792ED");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorPrimaryLight { get; set; } = new StyleColor("6CB6F3");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorPrimaryDark { get; set; } = new StyleColor("0D569E");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorPrimaryContrast { get; set; } = new StyleColor("FFFFFF");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorSecondary { get; set; } = new StyleColor("CCCCCC");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorSecondaryLight { get; set; } = new StyleColor("EEEEEE");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorSecondaryDark { get; set; } = new StyleColor("AAAAAA");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorSecondaryContrast { get; set; } = new StyleColor("222222");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorTertiary { get; set; } = new StyleColor("EAEAEA");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorTertiaryLight { get; set; } = new StyleColor("F2F2F2");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorTertiaryDark { get; set; } = new StyleColor("D8D8D8");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorTertiaryContrast { get; set; } = new StyleColor("333333");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorNeutral { get; set; } = new StyleColor("B2B2B2");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorNeutralLight { get; set; } = new StyleColor("E5E5E5");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorNeutralDark { get; set; } = new StyleColor("999999");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorNeutralContrast { get; set; } = new StyleColor("000000");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorBackground { get; set; } = new StyleColor("FFFFFF");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorBackgroundLight { get; set; } = new StyleColor("FFFFFF");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorBackgroundDark { get; set; } = new StyleColor("999999");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorBackgroundContrast { get; set; } = new StyleColor("000000");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorForeground { get; set; } = new StyleColor("000000");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorForegroundLight { get; set; } = new StyleColor("333333");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorForegroundDark { get; set; } = new StyleColor("000000");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorForegroundContrast { get; set; } = new StyleColor("FFFFFF");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorInfo { get; set; } = new StyleColor("17A2B8");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorInfoLight { get; set; } = new StyleColor("23B8CF");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorInfoDark { get; set; } = new StyleColor("00889E");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorInfoContrast { get; set; } = new StyleColor("FFFFFF");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorSuccess { get; set; } = new StyleColor("28A745");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorSuccessLight { get; set; } = new StyleColor("49C25D");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorSuccessDark { get; set; } = new StyleColor("00902F");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorSuccessContrast { get; set; } = new StyleColor("FFFFFF");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorWarning { get; set; } = new StyleColor("FFC107");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorWarningLight { get; set; } = new StyleColor("FFD42E");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorWarningDark { get; set; } = new StyleColor("E9AD00");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorWarningContrast { get; set; } = new StyleColor("FFFFFF");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorDanger { get; set; } = new StyleColor("DC3545");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorDangerLight { get; set; } = new StyleColor("F14954");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorDangerDark { get; set; } = new StyleColor("C51535");

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorDangerContrast { get; set; } = new StyleColor("FFFFFF");

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
