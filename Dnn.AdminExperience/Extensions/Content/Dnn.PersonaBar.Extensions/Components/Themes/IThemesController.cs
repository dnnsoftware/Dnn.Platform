using System.Collections.Generic;
using Dnn.PersonaBar.Themes.Components.DTO;
using DotNetNuke.Entities.Portals;

namespace Dnn.PersonaBar.Themes.Components
{
    public interface IThemesController
    {
        /// <summary>
        /// Get Skins.
        /// </summary>
        /// <param name="portalSettings"></param>
        /// <param name="level">portal level or host level.</param>
        /// <returns></returns>
        IList<ThemeInfo> GetLayouts(PortalSettings portalSettings, ThemeLevel level);

        /// <summary>
        /// Get Containers.
        /// </summary>
        /// <param name="portalSettings"></param>
        /// <param name="level">portal level or host level.</param>
        /// <returns></returns>
        IList<ThemeInfo> GetContainers(PortalSettings portalSettings, ThemeLevel level);

        /// <summary>
        /// get skin files in the skin.
        /// </summary>
        /// <param name="portalSettings"></param>
        /// <param name="theme"></param>
        /// <returns></returns>
        IList<ThemeFileInfo> GetThemeFiles(PortalSettings portalSettings, ThemeInfo theme);

        /// <summary>
        /// update portal skin.
        /// </summary>
        /// <param name="portalId">portal id.</param>
        /// <param name="themeFile">skin info.</param>
        /// <param name="scope">change skin or container.</param>
        void ApplyTheme(int portalId, ThemeFileInfo themeFile, ApplyThemeScope scope);

        /// <summary>
        /// delete a skin or container.
        /// </summary>
        /// <param name="portalSettings"></param>
        /// <param name="themeFile"></param>
        void DeleteTheme(PortalSettings portalSettings, ThemeFileInfo themeFile);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalSettings"></param>
        /// <param name="theme"></param>
        void DeleteThemePackage(PortalSettings portalSettings, ThemeInfo theme);

        /// <summary>
        /// Update Theme Attributes.
        /// </summary>
        /// <param name="portalSettings"></param>
        /// <param name="updateTheme"></param>
        void UpdateTheme(PortalSettings portalSettings, UpdateThemeInfo updateTheme);

        /// <summary>
        /// Parse skin package.
        /// </summary>
        /// <param name="portalSettings"></param>
        /// <param name="theme"></param>
        /// <param name="parseType"></param>
        void ParseTheme(PortalSettings portalSettings, ThemeInfo theme, ParseType parseType);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalSettings"></param>
        /// <param name="filePath"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        ThemeFileInfo GetThemeFile(PortalSettings portalSettings, string filePath, ThemeType type);

        /// <summary>
        /// update portal skin.
        /// </summary>
        /// <param name="portalSettings">portal settings.</param>
        /// <param name="themeName"></param>
        /// <param name="level"></param>
        void ApplyDefaultTheme(PortalSettings portalSettings, string themeName, ThemeLevel level);
    }
}