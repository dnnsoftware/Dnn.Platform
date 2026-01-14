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
        private const string Prefix = "DnnCssVars_";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public bool AllowAdminEdits { get; set; } = false;

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorPrimary { get; set; } = "00A5E0";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorPrimaryLight { get; set; } = "1AAEE3";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorPrimaryDark { get; set; } = "0091C5";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorPrimaryContrast { get; set; } = "FFFFFF";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorSecondary { get; set; } = "ED3D46";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorSecondaryLight { get; set; } = "EF5059";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorSecondaryDark { get; set; } = "D1363E";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorSecondaryContrast { get; set; } = "FFFFFF";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorTertiary { get; set; } = "0E2936";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorTertiaryLight { get; set; } = "3C7A9A";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorTertiaryDark { get; set; } = "0B1C24";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorTertiaryContrast { get; set; } = "FFFFFF";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorNeutral { get; set; } = "DCDCDC";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorNeutralLight { get; set; } = "F0F0F0";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorNeutralDark { get; set; } = "999999";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorNeutralContrast { get; set; } = "000000";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorBackground { get; set; } = "FFFFFF";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorBackgroundLight { get; set; } = "F5F5F5";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorBackgroundDark { get; set; } = "CCCCCC";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorBackgroundContrast { get; set; } = "000000";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorForeground { get; set; } = "472A2B";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorForegroundLight { get; set; } = "673D3E";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorForegroundDark { get; set; } = "231717";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorForegroundContrast { get; set; } = "FFFFFF";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorInfo { get; set; } = "17A2B8";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorInfoLight { get; set; } = "23B8CF";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorInfoDark { get; set; } = "00889E";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorInfoContrast { get; set; } = "FFFFFF";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorSuccess { get; set; } = "28A745";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorSuccessLight { get; set; } = "49C25D";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorSuccessDark { get; set; } = "00902F";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorSuccessContrast { get; set; } = "FFFFFF";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorWarning { get; set; } = "FFC107";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorWarningLight { get; set; } = "FFD42E";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorWarningDark { get; set; } = "E9AD00";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorWarningContrast { get; set; } = "FFFFFF";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorDanger { get; set; } = "DC3545";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorDangerLight { get; set; } = "F14954";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorDangerDark { get; set; } = "C51535";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorDangerContrast { get; set; } = "FFFFFF";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorSurface { get; set; } = "DDDDDD";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorSurfaceLight { get; set; } = "EEEEEE";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorSurfaceDark { get; set; } = "CCCCCC";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public string ColorSurfaceContrast { get; set; } = "000000";

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public double ControlsRadius { get; set; } = 0;

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public double ControlsPadding { get; set; } = 9;

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
        public double BaseFontSize { get; set; } = 16;

        /// <inheritdoc/>
        [PortalSetting(Prefix = Prefix)]
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
                    --dnn-variation-opacity: {{this.VariationOpacity}};
                }
             """;
        }

        private static string GetRed(string hexValue)
        {
            return GetColorValue(hexValue, 0);
        }

        private static string GetGreen(string hexValue)
        {
            return GetColorValue(hexValue, 2);
        }

        private static string GetBlue(string hexValue)
        {
            return GetColorValue(hexValue, 4);
        }

        private static string GetColorValue(string hexValue, int startIndex)
        {
            return int.Parse(
                    hexValue.Substring(startIndex, 2),
                    NumberStyles.AllowHexSpecifier,
                    CultureInfo.InvariantCulture)
                .ToString(CultureInfo.InvariantCulture);
        }
    }
}
