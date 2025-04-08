// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Skins;

using System.Collections.Generic;

using DotNetNuke.Abstractions.Portals;

/// <summary>Handles the Business Control Layer for Skins.</summary>
public interface ISkinService
{
    /// <summary>Gets the folder name for the specified <paramref name="packageType"/>.</summary>
    /// <param name="packageType">The type of the skin package.</param>
    /// <returns>The folder name.</returns>
    string GetFolderName(SkinPackageType packageType);

    /// <summary>Gets the global default skin src.</summary>
    /// <param name="packageType">The type of the skin package.</param>
    /// <param name="skinType">The type of the skin.</param>
    /// <returns>The global default edit skin.</returns>
    string GetDefaultSkinSrc(SkinPackageType packageType, SkinType skinType);

    /// <summary>Gets a skin package by its id.</summary>
    /// <param name="packageId">The skin package id.</param>
    /// <returns>The skin package.</returns>
    ISkinPackageInfo GetSkinPackageById(int packageId);

    /// <summary>Gets a skin package by its id.</summary>
    /// <param name="portalId">The portal id.</param>
    /// <param name="skinName">The name of the skin.</param>
    /// <param name="packageType">The type of the skin package.</param>
    /// <returns>The skin package.</returns>
    ISkinPackageInfo GetSkinPackage(int portalId, string skinName, SkinPackageType packageType);

    /// <summary>Creates a new instance of <see cref="ISkinInfo"/>.</summary>
    /// <returns>The skin.</returns>
    ISkinInfo CreateSkin();

    /// <summary>Creates a new instance of <see cref="ISkinPackageInfo"/>.</summary>
    /// <returns>The skin package.</returns>
    ISkinPackageInfo CreateSkinPackage();

    /// <summary>Adds a new skin.</summary>
    /// <param name="skin">The skin to add.</param>
    /// <returns>The skin id.</returns>
    int AddSkin(ISkinInfo skin);

    /// <summary>Adds a skin package.</summary>
    /// <param name="skinPackage">The skin package to add.</param>
    /// <returns>The skin package id.</returns>
    int AddSkinPackage(ISkinPackageInfo skinPackage);

    /// <summary>Checks if a skin can be deleted.</summary>
    /// <param name="folderPath">Path to the skin folder.</param>
    /// <param name="portalHomeDirMapPath">Path to the portal home directory (<see cref="IPortalSettings.HomeDirectoryMapPath"/>).</param>
    /// <returns>True if the skin can be deleted.</returns>
    bool CanDeleteSkinFolder(string folderPath, string portalHomeDirMapPath);

    /// <summary>Deletes a skin.</summary>
    /// <param name="skin">The skin to delete.</param>
    void DeleteSkin(ISkinInfo skin);

    /// <summary>Deletes a skin package.</summary>
    /// <param name="skinPackage">The skin package to delete.</param>
    void DeleteSkinPackage(ISkinPackageInfo skinPackage);

    /// <summary>Gets the skin source path.</summary>
    /// <example>
    /// <c>[G]Skins/Xcillion/Inner.ascx</c> becomes <c>[G]Skins/Xcillion</c>.
    /// </example>
    /// <param name="skinSrc">The input skin source path.</param>
    /// <returns>The skin source path.</returns>
    string FormatSkinPath(string skinSrc);

    /// <summary>Formats the skin source path.</summary>
    /// <remarks>
    /// By default the following tokens are replaced:<br />
    /// <c>[G]</c> - Host path (default: '/Portals/_default/').<br />
    /// <c>[S]</c> - Home system directory (default: '/Portals/[PortalID]-System/').<br />
    /// <c>[L]</c> - Home directory (default: '/Portals/[PortalID]/').
    /// </remarks>
    /// <example>
    /// <c>[G]Skins/Xcillion/Inner.ascx</c> becomes <c>/Portals/_default/Skins/Xcillion/Inner.ascx</c>.
    /// </example>
    /// <param name="skinSrc">The input skin source path.</param>
    /// <param name="portalSettings">The portal settings containing configuration data.</param>
    /// <returns>The formatted skin source path.</returns>
    string FormatSkinSrc(string skinSrc, IPortalSettings portalSettings);

    /// <summary>Determines if a given skin is defined as a global skin.</summary>
    /// <param name="skinSrc">This is the app relative path and filename of the skin to be checked.</param>
    /// <returns>True if the skin is located in the HostPath child directories.</returns>
    /// <remarks>This function performs a quick check to detect the type of skin that is
    /// passed as a parameter.  Using this method abstracts knowledge of the actual location
    /// of skins in the file system.
    /// </remarks>
    bool IsGlobalSkin(string skinSrc);

    /// <summary>Sets the skin for the specified <paramref name="portalId"/> and <paramref name="skinType"/>.</summary>
    /// <param name="packageType">The type of the skin package.</param>
    /// <param name="portalId">The portal to set the skin for or <c>-1</c> for the global skin.</param>
    /// <param name="skinType">The type of the skin.</param>
    /// <param name="skinSrc">The skin source path.</param>
    void SetSkin(SkinPackageType packageType, int portalId, SkinType skinType, string skinSrc);

    /// <summary>Updates a existing skin.</summary>
    /// <param name="skin">The skin to update.</param>
    void UpdateSkin(ISkinInfo skin);

    /// <summary>Updates a existing skin package.</summary>
    /// <param name="skinPackage">The skin package to update.</param>
    void UpdateSkinPackage(ISkinPackageInfo skinPackage);

    /// <summary>Get all skins for the specified <paramref name="portalInfo"/> within the specified <paramref name="folder"/>.</summary>
    /// <param name="portalInfo">The portal to get the skins for.</param>
    /// <param name="skinRoot">The skin type to search for skins. Default: <see cref="SkinPackageType.Skin"/>.</param>
    /// <param name="folder">The scope to search for skins. Default: <see cref="SkinFolder.All"/>.</param>
    /// <returns>A list of skins.</returns>
    IEnumerable<KeyValuePair<string, string>> GetSkinsInFolder(IPortalInfo portalInfo, SkinType skinRoot = SkinType.Site, SkinFolder folder = SkinFolder.All);
}
