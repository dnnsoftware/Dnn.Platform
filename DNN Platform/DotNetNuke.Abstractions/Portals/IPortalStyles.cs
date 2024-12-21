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
        /// Gets or sets a value indicating whether the site admins can edit the styles.
        /// </summary>
        bool AllowAdminEdits { get; set; }

        /// <summary>
        /// Gets or sets the main shade of the primary color.
        /// </summary>
        string ColorPrimary { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the primary color.
        /// </summary>
        string ColorPrimaryLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the primary color.
        /// </summary>
        string ColorPrimaryDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the primary color shades.
        /// </summary>
        string ColorPrimaryContrast { get; set; }

        /// <summary>
        /// Gets or sets the main shade of the secondary color.
        /// </summary>
        string ColorSecondary { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the secondary color.
        /// </summary>
        string ColorSecondaryLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the secondary color.
        /// </summary>
        string ColorSecondaryDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the secondary color shades.
        /// </summary>
        string ColorSecondaryContrast { get; set; }

        /// <summary>
        /// Gets or sets the main shade of the Tertiary color.
        /// </summary>
        string ColorTertiary { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the Tertiary color.
        /// </summary>
        string ColorTertiaryLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the Tertiary color.
        /// </summary>
        string ColorTertiaryDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the Tertiary color shades.
        /// </summary>
        string ColorTertiaryContrast { get; set; }

        /// <summary>
        /// Gets or sets the main shade of the Neutral color.
        /// </summary>
        string ColorNeutral { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the Neutral color.
        /// </summary>
        string ColorNeutralLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the Neutral color.
        /// </summary>
        string ColorNeutralDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the Neutral color shades.
        /// </summary>
        string ColorNeutralContrast { get; set; }

        /// <summary>
        /// Gets or sets the main shade of the Background color.
        /// </summary>
        string ColorBackground { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the Background color.
        /// </summary>
        string ColorBackgroundLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the Background color.
        /// </summary>
        string ColorBackgroundDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the Background color shades.
        /// </summary>
        string ColorBackgroundContrast { get; set; }

        /// <summary>
        /// Gets or sets the main shade of the Foreground color.
        /// </summary>
        string ColorForeground { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the Foreground color.
        /// </summary>
        string ColorForegroundLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the Foreground color.
        /// </summary>
        string ColorForegroundDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the Foreground color shades.
        /// </summary>
        string ColorForegroundContrast { get; set; }

        /// <summary>
        /// Gets or sets the main shade of the Info color.
        /// </summary>
        string ColorInfo { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the Info color.
        /// </summary>
        string ColorInfoLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the Info color.
        /// </summary>
        string ColorInfoDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the Info color shades.
        /// </summary>
        string ColorInfoContrast { get; set; }

        /// <summary>
        /// Gets or sets the main shade of the Success color.
        /// </summary>
        string ColorSuccess { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the Success color.
        /// </summary>
        string ColorSuccessLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the Success color.
        /// </summary>
        string ColorSuccessDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the Success color shades.
        /// </summary>
        string ColorSuccessContrast { get; set; }

        /// <summary>
        /// Gets or sets the main shade of the Warning color.
        /// </summary>
        string ColorWarning { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the Warning color.
        /// </summary>
        string ColorWarningLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the Warning color.
        /// </summary>
        string ColorWarningDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the Warning color shades.
        /// </summary>
        string ColorWarningContrast { get; set; }

        /// <summary>
        /// Gets or sets the main shade of the Danger color.
        /// </summary>
        string ColorDanger { get; set; }

        /// <summary>
        /// Gets or sets the light shade of the Danger color.
        /// </summary>
        string ColorDangerLight { get; set; }

        /// <summary>
        /// Gets or sets the dark shade of the Danger color.
        /// </summary>
        string ColorDangerDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the Danger color shades.
        /// </summary>
        string ColorDangerContrast { get; set; }

        /// <summary>
        /// Gets or sets the color of the surface.
        /// A surface is an area that is overlaid over the main background (menus, popovers, etc).
        /// </summary>
        string ColorSurface { get; set; }

        /// <summary>
        /// Gets or sets the light variation of surface color.
        /// </summary>
        string ColorSurfaceLight { get; set; }

        /// <summary>
        /// Gets or sets the dark variation of surface color.
        /// </summary>
        string ColorSurfaceDark { get; set; }

        /// <summary>
        /// Gets or sets a color that contrasts well over the surface color.
        /// </summary>
        string ColorSurfaceContrast { get; set; }

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
