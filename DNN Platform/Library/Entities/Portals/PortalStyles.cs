// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Portals
{
    using System.Globalization;

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
        public string ColorNeutral { get; set; } = "DCDCDC";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorNeutralLight { get; set; } = "F0F0F0";

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
        public string ColorForeground { get; set; } = "323232";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorForegroundLight { get; set; } = "A2A2A2";

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
        public string ColorSurface { get; set; } = "DDDDDD";

        /// <inheritdoc/>
        [PortalSetting]
        public string ColorSurfaceLight { get; set; } = "EEEEEE";

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

        /// <inheritdoc/>
        public string FileName => "dnn-css-variables.css";

        /// <summary>
        /// Converts the styles into a css string.
        /// </summary>
        /// <returns>
        /// A string representing the css properties.
        /// </returns>
        public override string ToString()
        {
            return $$"""
                :root {
                    --dnn-color-primary: #{{this.ColorPrimary}};
                    --dnn-color-primary-light: #{{this.ColorPrimaryLight}};
                    --dnn-color-primary-dark: #{{this.ColorPrimaryDark}};
                    --dnn-color-primary-contrast: #{{this.ColorPrimaryContrast}};
                    --dnn-color-primary-r: {{GetRed(this.ColorPrimary)}};
                    --dnn-color-primary-g: {{GetGreen(this.ColorPrimary)}};
                    --dnn-color-primary-b: {{GetBlue(this.ColorPrimary)}};
                        
                    --dnn-color-secondary: #{{this.ColorSecondary}};
                    --dnn-color-secondary-light: #{{this.ColorSecondaryLight}};
                    --dnn-color-secondary-dark: #{{this.ColorSecondaryDark}};
                    --dnn-color-secondary-contrast: #{{this.ColorSecondaryContrast}};
                    --dnn-color-secondary-r: {{GetRed(this.ColorSecondary)}};
                    --dnn-color-secondary-g: {{GetGreen(this.ColorSecondary)}};
                    --dnn-color-secondary-b: {{GetBlue(this.ColorSecondary)}};
                        
                    --dnn-color-tertiary: #{{this.ColorTertiary}};
                    --dnn-color-tertiary-light: #{{this.ColorTertiaryLight}};
                    --dnn-color-tertiary-dark: #{{this.ColorTertiaryDark}};
                    --dnn-color-tertiary-contrast: #{{this.ColorTertiaryContrast}};
                    --dnn-color-tertiary-r: {{GetRed(this.ColorTertiary)}};
                    --dnn-color-tertiary-g: {{GetGreen(this.ColorTertiary)}};
                    --dnn-color-tertiary-b: {{GetBlue(this.ColorTertiary)}};
                        
                    --dnn-color-neutral: #{{this.ColorNeutral}};
                    --dnn-color-neutral-light: #{{this.ColorNeutralLight}};
                    --dnn-color-neutral-dark: #{{this.ColorNeutralDark}};
                    --dnn-color-neutral-contrast: #{{this.ColorNeutralContrast}};
                    --dnn-color-neutral-r: {{GetRed(this.ColorNeutral)}};
                    --dnn-color-neutral-g: {{GetGreen(this.ColorNeutral)}};
                    --dnn-color-neutral-b: {{GetBlue(this.ColorNeutral)}};
                        
                    --dnn-color-background: #{{this.ColorBackground}};
                    --dnn-color-background-light: #{{this.ColorBackgroundLight}};
                    --dnn-color-background-dark: #{{this.ColorBackgroundDark}};
                    --dnn-color-background-contrast: #{{this.ColorBackgroundContrast}};
                    --dnn-color-background-r: {{GetRed(this.ColorBackground)}};
                    --dnn-color-background-g: {{GetGreen(this.ColorBackground)}};
                    --dnn-color-background-b: {{GetBlue(this.ColorBackground)}};
                        
                    --dnn-color-foreground: #{{this.ColorForeground}};
                    --dnn-color-foreground-light: #{{this.ColorForegroundLight}};
                    --dnn-color-foreground-dark: #{{this.ColorForegroundDark}};
                    --dnn-color-foreground-contrast: #{{this.ColorForegroundContrast}};
                    --dnn-color-foreground-r: {{GetRed(this.ColorForeground)}};
                    --dnn-color-foreground-g: {{GetGreen(this.ColorForeground)}};
                    --dnn-color-foreground-b: {{GetBlue(this.ColorForeground)}};
                
                    --dnn-color-info: #{{this.ColorInfo}};
                    --dnn-color-info-light: #{{this.ColorInfoLight}};
                    --dnn-color-info-dark: #{{this.ColorInfoDark}};
                    --dnn-color-info-contrast: #{{this.ColorInfoContrast}};
                    --dnn-color-info-r: {{GetRed(this.ColorInfo)}};
                    --dnn-color-info-g: {{GetGreen(this.ColorInfo)}};
                    --dnn-color-info-b: {{GetBlue(this.ColorInfo)}};
                
                    --dnn-color-success: #{{this.ColorSuccess}};
                    --dnn-color-success-light: #{{this.ColorSuccessLight}};
                    --dnn-color-success-dark: #{{this.ColorSuccessDark}};
                    --dnn-color-success-contrast: #{{this.ColorSuccessContrast}};
                    --dnn-color-success-r: {{GetRed(this.ColorSuccess)}};
                    --dnn-color-success-g: {{GetGreen(this.ColorSuccess)}};
                    --dnn-color-success-b: {{GetBlue(this.ColorSuccess)}};
                
                    --dnn-color-warning: #{{this.ColorWarning}};
                    --dnn-color-warning-light: #{{this.ColorWarningLight}};
                    --dnn-color-warning-dark: #{{this.ColorWarningDark}};
                    --dnn-color-warning-contrast: #{{this.ColorWarningContrast}};
                    --dnn-color-warning-r: {{GetRed(this.ColorWarning)}};
                    --dnn-color-warning-g: {{GetGreen(this.ColorWarning)}};
                    --dnn-color-warning-b: {{GetBlue(this.ColorWarning)}};
                
                    --dnn-color-danger: #{{this.ColorDanger}};
                    --dnn-color-danger-light: #{{this.ColorDangerLight}};
                    --dnn-color-danger-dark: #{{this.ColorDangerDark}};
                    --dnn-color-danger-contrast: #{{this.ColorDangerContrast}};
                    --dnn-color-danger-r: {{GetRed(this.ColorDanger)}};
                    --dnn-color-danger-g: {{GetGreen(this.ColorDanger)}};
                    --dnn-color-danger-b: {{GetBlue(this.ColorDanger)}};
                
                    --dnn-color-surface: #{{this.ColorSurface}};
                    --dnn-color-surface-light: #{{this.ColorSurfaceLight}};
                    --dnn-color-surface-dark: #{{this.ColorSurfaceDark}};
                    --dnn-color-surface-contrast: #{{this.ColorSurfaceContrast}};
                    --dnn-color-surface-r: {{GetRed(this.ColorSurface)}};
                    --dnn-color-surface-g: {{GetGreen(this.ColorSurface)}};
                    --dnn-color-surface-b: {{GetBlue(this.ColorSurface)}};
                
                    --dnn-controls-radius: {{this.ControlsRadius}}px;
                    --dnn-controls-padding: {{this.ControlsPadding}}px;
                    --dnn-base-font-size: {{this.BaseFontSize}}px;
                    --variation-opacity: {{this.VariationOpacity}};
                }
             """;
        }

        private static string GetRed(string hexValue)
        {
            return int.Parse(hexValue.Substring(0, 2), NumberStyles.AllowHexSpecifier).ToString(CultureInfo.InvariantCulture);
        }

        private static string GetGreen(string hexValue)
        {
            return int.Parse(hexValue.Substring(2, 2), NumberStyles.AllowHexSpecifier).ToString(CultureInfo.InvariantCulture);
        }

        private static string GetBlue(string hexValue)
        {
            return int.Parse(hexValue.Substring(4, 2), NumberStyles.AllowHexSpecifier).ToString(CultureInfo.InvariantCulture);
        }
    }
}
