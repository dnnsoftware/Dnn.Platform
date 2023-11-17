// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Portals
{
    /// <summary>
    /// Provides css custom properties to customize the Dnn UI in the platform, themes and modules for an overall consistent look.
    /// </summary>
    public interface IPortalStyles
    {
        /// <summary>
        /// Gets or sets the main shade of the primary color.
        /// </summary>
        IStyleColor ColorPrimary { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the primary color.
        /// </summary>
        public IStyleColor ColorPrimaryLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the primary color.
        /// </summary>
        public IStyleColor ColorPrimaryDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the primary color shades.
        /// </summary>
        public IStyleColor ColorPrimaryContrast { get; set; }

        /// <summary>
        /// Gets or sets the main shade of the secondary color.
        /// </summary>
        public IStyleColor ColorSecondary { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the secondary color.
        /// </summary>
        public IStyleColor ColorSecondaryLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the secondary color.
        /// </summary>
        public IStyleColor ColorSecondaryDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the secondary color shades.
        /// </summary>
        public IStyleColor ColorSecondaryContrast { get; set; }

        /// <summary>
        /// Gets or sets the main shade of the Tertiary color.
        /// </summary>
        public IStyleColor ColorTertiary { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the Tertiary color.
        /// </summary>
        public IStyleColor ColorTertiaryLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the Tertiary color.
        /// </summary>
        public IStyleColor ColorTertiaryDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the Tertiary color shades.
        /// </summary>
        public IStyleColor ColorTertiaryContrast { get; set; }

        /// <summary>
        /// Gets or sets the main shade of the Neutral color.
        /// </summary>
        public IStyleColor ColorNeutral { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the Neutral color.
        /// </summary>
        public IStyleColor ColorNeutralLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the Neutral color.
        /// </summary>
        public IStyleColor ColorNeutralDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the Neutral color shades.
        /// </summary>
        public IStyleColor ColorNeutralContrast { get; set; }

        /// <summary>
        /// Gets or sets the main shade of the Background color.
        /// </summary>
        public IStyleColor ColorBackground { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the Background color.
        /// </summary>
        public IStyleColor ColorBackgroundLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the Background color.
        /// </summary>
        public IStyleColor ColorBackgroundDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the Background color shades.
        /// </summary>
        public IStyleColor ColorBackgroundContrast { get; set; }

        /// <summary>
        /// Gets or sets the main shade of the Foreground color.
        /// </summary>
        public IStyleColor ColorForeground { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the Foreground color.
        /// </summary>
        public IStyleColor ColorForegroundLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the Foreground color.
        /// </summary>
        public IStyleColor ColorForegroundDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the Foreground color shades.
        /// </summary>
        public IStyleColor ColorForegroundContrast { get; set; }

        /// <summary>
        /// Gets or sets the main shade of the Info color.
        /// </summary>
        public IStyleColor ColorInfo { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the Info color.
        /// </summary>
        public IStyleColor ColorInfoLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the Info color.
        /// </summary>
        public IStyleColor ColorInfoDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the Info color shades.
        /// </summary>
        public IStyleColor ColorInfoContrast { get; set; }

        /// <summary>
        /// Gets or sets the main shade of the Success color.
        /// </summary>
        public IStyleColor ColorSuccess { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the Success color.
        /// </summary>
        public IStyleColor ColorSuccessLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the Success color.
        /// </summary>
        public IStyleColor ColorSuccessDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the Success color shades.
        /// </summary>
        public IStyleColor ColorSuccessContrast { get; set; }

        /// <summary>
        /// Gets or sets the main shade of the Warning color.
        /// </summary>
        public IStyleColor ColorWarning { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the Warning color.
        /// </summary>
        public IStyleColor ColorWarningLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the Warning color.
        /// </summary>
        public IStyleColor ColorWarningDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the Warning color shades.
        /// </summary>
        public IStyleColor ColorWarningContrast { get; set; }

        /// <summary>
        /// Gets or sets the main shade of the Danger color.
        /// </summary>
        public IStyleColor ColorDanger { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the Danger color.
        /// </summary>
        public IStyleColor ColorDangerLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the Danger color.
        /// </summary>
        public IStyleColor ColorDangerDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the Danger color shades.
        /// </summary>
        public IStyleColor ColorDangerContrast { get; set; }

        /// <summary>
        /// Gets or sets the controls border radius in pixels.
        /// </summary>
        public int ControlsRadius { get; set; }

        /// <summary>
        /// Gets or sets the controls padding in pixels.
        /// </summary>
        public int ControlsPadding { get; set; }

        /// <summary>
        /// Gets or sets the base font size in pixels.
        /// </summary>
        int BaseFontSize { get; set; }
    }
}
