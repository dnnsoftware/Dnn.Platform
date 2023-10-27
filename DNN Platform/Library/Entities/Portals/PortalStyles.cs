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
        public IStyleColor ColorPrimary { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorPrimaryLight { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorPrimaryDark { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorPrimaryContrast { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorSecondary { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorSecondaryLight { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorSecondaryDark { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorSecondaryContrast { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorTertiary { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorTertiaryLight { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorTertiaryDark { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorTertiaryContrast { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorNeutral { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorNeutralLight { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorNeutralDark { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorNeutralContrast { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorBackground { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorBackgroundLight { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorBackgroundDark { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorBackgroundContrast { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorForeground { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorForegroundLight { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorForegroundDark { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorForegroundContrast { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorInfo { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorInfoLight { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorInfoDark { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorInfoContrast { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorSuccess { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorSuccessLight { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorSuccessDark { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorSuccessContrast { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorWarning { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorWarningLight { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorWarningDark { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorWarningContrast { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorDanger { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorDangerLight { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorDangerDark { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public IStyleColor ColorDangerContrast { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public int ControlsRadius { get; set; }

        /// <inheritdoc/>
        [PortalSetting]
        public int ControlsPadding { get; set; }
    }
}
