// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Themes.Components;

using System.Collections.Generic;

using Dnn.PersonaBar.Themes.Components.DTO;
using DotNetNuke.Entities.Portals;

public interface IThemesController
{
    /// <summary>Get Skins.</summary>
    /// <param name="portalSettings">The portal settings.</param>
    /// <param name="level">portal level or host level.</param>
    /// <returns>A list of <see cref="ThemeInfo"/> instances.</returns>
    IList<ThemeInfo> GetLayouts(PortalSettings portalSettings, ThemeLevel level);

    /// <summary>Get Containers.</summary>
    /// <param name="portalSettings">The portal settings.</param>
    /// <param name="level">portal level or host level.</param>
    /// <returns>A list of <see cref="ThemeInfo"/> instances.</returns>
    IList<ThemeInfo> GetContainers(PortalSettings portalSettings, ThemeLevel level);

    /// <summary>get skin files in the skin.</summary>
    /// <param name="portalSettings">The portal settings.</param>
    /// <param name="theme">The theme.</param>
    /// <returns>A list of new <see cref="ThemeFileInfo"/> instances.</returns>
    IList<ThemeFileInfo> GetThemeFiles(PortalSettings portalSettings, ThemeInfo theme);

    /// <summary>update portal skin.</summary>
    /// <param name="portalId">portal ID.</param>
    /// <param name="themeFile">skin info.</param>
    /// <param name="scope">change skin or container.</param>
    void ApplyTheme(int portalId, ThemeFileInfo themeFile, ApplyThemeScope scope);

    /// <summary>delete a skin or container.</summary>
    /// <param name="portalSettings">The portal settings.</param>
    /// <param name="themeFile">The theme file info.</param>
    void DeleteTheme(PortalSettings portalSettings, ThemeFileInfo themeFile);

    /// <summary>Deletes a theme package.</summary>
    /// <param name="portalSettings">The portal settings.</param>
    /// <param name="theme">The theme info.</param>
    void DeleteThemePackage(PortalSettings portalSettings, ThemeInfo theme);

    /// <summary>Update Theme Attributes.</summary>
    /// <param name="portalSettings">The portal settings.</param>
    /// <param name="updateTheme">Information about the update.</param>
    void UpdateTheme(PortalSettings portalSettings, UpdateThemeInfo updateTheme);

    /// <summary>Parse skin package.</summary>
    /// <param name="portalSettings">The portal settings.</param>
    /// <param name="theme">The theme info.</param>
    /// <param name="parseType">Localized or portable.</param>
    void ParseTheme(PortalSettings portalSettings, ThemeInfo theme, ParseType parseType);

    /// <summary>Gets the theme file info.</summary>
    /// <param name="portalSettings">The portal settings.</param>
    /// <param name="filePath">The file path.</param>
    /// <param name="type">Skin or container.</param>
    /// <returns>A <see cref="ThemeFileInfo"/> instance or <see langword="null"/>.</returns>
    ThemeFileInfo GetThemeFile(PortalSettings portalSettings, string filePath, ThemeType type);

    /// <summary>update portal skin.</summary>
    /// <param name="portalSettings">The portal settings.</param>
    /// <param name="themeName">The theme name.</param>
    /// <param name="level">portal level or host level.</param>
    void ApplyDefaultTheme(PortalSettings portalSettings, string themeName, ThemeLevel level);
}
