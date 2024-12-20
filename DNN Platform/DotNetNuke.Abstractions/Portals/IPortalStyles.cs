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
        StyleColor ColorPrimaryLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the primary color.
        /// </summary>
        StyleColor ColorPrimaryDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the primary color shades.
        /// </summary>
        StyleColor ColorPrimaryContrast { get; set; }

        /// <summary>
        /// Gets or sets the main shade of the secondary color.
        /// </summary>
        StyleColor ColorSecondary { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the secondary color.
        /// </summary>
        StyleColor ColorSecondaryLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the secondary color.
        /// </summary>
        StyleColor ColorSecondaryDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the secondary color shades.
        /// </summary>
        StyleColor ColorSecondaryContrast { get; set; }

        /// <summary>
        /// Gets or sets the main shade of the Tertiary color.
        /// </summary>
        StyleColor ColorTertiary { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the Tertiary color.
        /// </summary>
        StyleColor ColorTertiaryLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the Tertiary color.
        /// </summary>
        StyleColor ColorTertiaryDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the Tertiary color shades.
        /// </summary>
        StyleColor ColorTertiaryContrast { get; set; }

        /// <summary>
        /// Gets or sets the main shade of the Neutral color.
        /// </summary>
        StyleColor ColorNeutral { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the Neutral color.
        /// </summary>
        StyleColor ColorNeutralLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the Neutral color.
        /// </summary>
        StyleColor ColorNeutralDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the Neutral color shades.
        /// </summary>
        StyleColor ColorNeutralContrast { get; set; }

        /// <summary>
        /// Gets or sets the main shade of the Background color.
        /// </summary>
        StyleColor ColorBackground { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the Background color.
        /// </summary>
        StyleColor ColorBackgroundLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the Background color.
        /// </summary>
        StyleColor ColorBackgroundDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the Background color shades.
        /// </summary>
        StyleColor ColorBackgroundContrast { get; set; }

        /// <summary>
        /// Gets or sets the main shade of the Foreground color.
        /// </summary>
        StyleColor ColorForeground { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the Foreground color.
        /// </summary>
        StyleColor ColorForegroundLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the Foreground color.
        /// </summary>
        StyleColor ColorForegroundDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the Foreground color shades.
        /// </summary>
        StyleColor ColorForegroundContrast { get; set; }

        /// <summary>
        /// Gets or sets the main shade of the Info color.
        /// </summary>
        StyleColor ColorInfo { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the Info color.
        /// </summary>
        StyleColor ColorInfoLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the Info color.
        /// </summary>
        StyleColor ColorInfoDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the Info color shades.
        /// </summary>
        StyleColor ColorInfoContrast { get; set; }

        /// <summary>
        /// Gets or sets the main shade of the Success color.
        /// </summary>
        StyleColor ColorSuccess { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the Success color.
        /// </summary>
        StyleColor ColorSuccessLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the Success color.
        /// </summary>
        StyleColor ColorSuccessDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the Success color shades.
        /// </summary>
        StyleColor ColorSuccessContrast { get; set; }

        /// <summary>
        /// Gets or sets the main shade of the Warning color.
        /// </summary>
        StyleColor ColorWarning { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the Warning color.
        /// </summary>
        StyleColor ColorWarningLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the Warning color.
        /// </summary>
        StyleColor ColorWarningDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the Warning color shades.
        /// </summary>
        StyleColor ColorWarningContrast { get; set; }

        /// <summary>
        /// Gets or sets the main shade of the Danger color.
        /// </summary>
        StyleColor ColorDanger { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the Danger color.
        /// </summary>
        StyleColor ColorDangerLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the Danger color.
        /// </summary>
        StyleColor ColorDangerDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the Danger color shades.
        /// </summary>
        StyleColor ColorDangerContrast { get; set; }

        /// <summary>
        /// Gets or sets the color of the surface.
        /// A surface is an area that is overlaid over the main background (menus, popovers, etc).
        /// </summary>
        StyleColor ColorSurface { get; set; }

        /// <summary>
        /// Gets or sets the light variation of surface color.
        /// </summary>
        StyleColor ColorSurfaceLight { get; set; }

        /// <summary>
        /// Gets or sets the dark variation of surface color.
        /// </summary>
        StyleColor ColorSurfaceDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the surface color.
        /// </summary>
        StyleColor ColorSurfaceContrast { get; set; }

        /// <summary>
        /// Gets or sets the controls border radius in pixels.
        /// </summary>
        double ControlsRadius { get; set; }

        /// <summary>
        /// Gets or sets the controls padding in pixels.
        /// </summary>
        double ControlsPadding { get; set; }

        /// <summary>
        /// Gets or sets the base font size in pixels.
        /// </summary>
        double BaseFontSize { get; set; }

        /// <summary>
        /// Gets or sets the color variation opacity.
        /// This can be used to define how opaque a contrasting color should look over another.
        /// </summary>
        double VariationOpacity { get; set; }
    }
}
