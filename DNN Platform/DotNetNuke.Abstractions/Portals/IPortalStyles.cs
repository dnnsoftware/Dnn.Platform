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
        StyleColor ColorPrimary { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the primary color.
        /// </summary>
        public StyleColor ColorPrimaryLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the primary color.
        /// </summary>
        public StyleColor ColorPrimaryDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the primary color shades.
        /// </summary>
        public StyleColor ColorPrimaryContrast { get; set; }

        /// <summary>
        /// Gets or sets the main shade of the secondary color.
        /// </summary>
        public StyleColor ColorSecondary { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the secondary color.
        /// </summary>
        public StyleColor ColorSecondaryLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the secondary color.
        /// </summary>
        public StyleColor ColorSecondaryDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the secondary color shades.
        /// </summary>
        public StyleColor ColorSecondaryContrast { get; set; }

        /// <summary>
        /// Gets or sets the main shade of the Tertiary color.
        /// </summary>
        public StyleColor ColorTertiary { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the Tertiary color.
        /// </summary>
        public StyleColor ColorTertiaryLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the Tertiary color.
        /// </summary>
        public StyleColor ColorTertiaryDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the Tertiary color shades.
        /// </summary>
        public StyleColor ColorTertiaryContrast { get; set; }

        /// <summary>
        /// Gets or sets the main shade of the Neutral color.
        /// </summary>
        public StyleColor ColorNeutral { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the Neutral color.
        /// </summary>
        public StyleColor ColorNeutralLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the Neutral color.
        /// </summary>
        public StyleColor ColorNeutralDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the Neutral color shades.
        /// </summary>
        public StyleColor ColorNeutralContrast { get; set; }

        /// <summary>
        /// Gets or sets the main shade of the Background color.
        /// </summary>
        public StyleColor ColorBackground { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the Background color.
        /// </summary>
        public StyleColor ColorBackgroundLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the Background color.
        /// </summary>
        public StyleColor ColorBackgroundDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the Background color shades.
        /// </summary>
        public StyleColor ColorBackgroundContrast { get; set; }

        /// <summary>
        /// Gets or sets the main shade of the Foreground color.
        /// </summary>
        public StyleColor ColorForeground { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the Foreground color.
        /// </summary>
        public StyleColor ColorForegroundLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the Foreground color.
        /// </summary>
        public StyleColor ColorForegroundDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the Foreground color shades.
        /// </summary>
        public StyleColor ColorForegroundContrast { get; set; }

        /// <summary>
        /// Gets or sets the main shade of the Info color.
        /// </summary>
        public StyleColor ColorInfo { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the Info color.
        /// </summary>
        public StyleColor ColorInfoLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the Info color.
        /// </summary>
        public StyleColor ColorInfoDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the Info color shades.
        /// </summary>
        public StyleColor ColorInfoContrast { get; set; }

        /// <summary>
        /// Gets or sets the main shade of the Success color.
        /// </summary>
        public StyleColor ColorSuccess { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the Success color.
        /// </summary>
        public StyleColor ColorSuccessLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the Success color.
        /// </summary>
        public StyleColor ColorSuccessDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the Success color shades.
        /// </summary>
        public StyleColor ColorSuccessContrast { get; set; }

        /// <summary>
        /// Gets or sets the main shade of the Warning color.
        /// </summary>
        public StyleColor ColorWarning { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the Warning color.
        /// </summary>
        public StyleColor ColorWarningLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the Warning color.
        /// </summary>
        public StyleColor ColorWarningDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the Warning color shades.
        /// </summary>
        public StyleColor ColorWarningContrast { get; set; }

        /// <summary>
        /// Gets or sets the main shade of the Danger color.
        /// </summary>
        public StyleColor ColorDanger { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the Danger color.
        /// </summary>
        public StyleColor ColorDangerLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the Danger color.
        /// </summary>
        public StyleColor ColorDangerDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the Danger color shades.
        /// </summary>
        public StyleColor ColorDangerContrast { get; set; }

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
