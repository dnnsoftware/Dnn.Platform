// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Components;

using System;
using System.ComponentModel;

using Dnn.Modules.ResourceManager.Components.Models;
using Dnn.Modules.ResourceManager.Helpers;

/// <summary>Defines common constants for the resource manager.</summary>
internal class Constants
{
    /// <summary>The relative path to the shared resource file for this module.</summary>
    public const string ViewResourceFileName = "~/DesktopModules/ResourceManager/App_LocalResources/ResourceManager.resx";

    /// <summary>The module path.</summary>
    public const string ModulePath = "DesktopModules/ResourceManager/";

    /// <summary>The cache key for the resource manager localization.</summary>
    public const string ResourceManagerResxDataCacheKey = "ResourceManagerResxResources:{0}:{1}";

    /// <summary>The cache key for the modified date of the resource files.</summary>
    public const string ResourceManagerResxModifiedDateCacheKey = "ResourceManagerResxModifiedDate:{0}";

    /// <summary>A string indicating the user has no permission to browser a folder.</summary>
    public const string UserHasNoPermissionToBrowseFolderDefaultMessage = "The user has no permission to browse this folder";

    /// <summary>The path the the groups folder.</summary>
    public const string GroupFolderPathStart = "Groups/";

    /// <summary>A key indicating the group icon can't be deleted.</summary>
    public const string GroupIconCantBeDeletedKey = "GroupIconCantBeDeleted.Error";

    /// <summary>A key indicating the user has no permission to download.</summary>
    public const string UserHasNoPermissionToDownloadKey = "UserHasNoPermissionToDownload.Error";

    /// <summary>A key indicating the user has no permission to add folders.</summary>
    public const string UserHasNoPermissionToAddFoldersKey = "UserHasNoPermissionToAddFolders.Error";

    /// <summary>The name of the setting for the resource manager home folder.</summary>
    public const string HomeFolderSettingName = "RM_HomeFolder";

    /// <summary>The resource manager module setting name.</summary>
    public const string ModeSettingName = "RM_Mode";

    /// <summary>How many items to show per page.</summary>
    public const int ItemsPerPage = 20;

    /// <summary>The width of the items.</summary>
    public const int ItemWidth = 176;

    /// <summary>The default module mode, <see cref="ModuleModes"/>.</summary>
    public const int DefaultMode = 0;

    /// <summary>The default field to sort files by.</summary>
    public const string DefaultSortingField = "ItemName";

    /// <summary>A timespan that represents 5 minutes.</summary>
    public static readonly TimeSpan FiveMinutes = TimeSpan.FromMinutes(5);

    /// <summary>Contains the localization info for the resource manager.</summary>
    public static readonly Localization ResourceManagerLocalization = new Localization
    {
        ResxDataCacheKey = ResourceManagerResxDataCacheKey,
        ResxModifiedDateCacheKey = ResourceManagerResxModifiedDateCacheKey,
    };

    /// <summary>The possible sorting fields for files.</summary>
    public static readonly dynamic[] SortingFields =
    {
        new { value = "LastModifiedOnDate", label = LocalizationHelper.GetString("LastModifiedOnDate") },
        new { value = "CreatedOnDate", label = LocalizationHelper.GetString("CreatedOnDate") },
        new { value = "ItemName", label= LocalizationHelper.GetString("ItemName") },
    };

    /// <summary>The localization cache key.</summary>
    internal const string LocalizationDataCacheKey = "LocalizationLocTable:{0}:{1}";

    /// <summary>Enumerates the possible module modes.</summary>
    public enum ModuleModes
    {
        /// <summary>Normal mode is when the module is used to manage site or host files.</summary>
        [Description("Normal")]
        Normal = 0,

        /// <summary>User mode is for when a module is used for a specific user to manage his files.</summary>
        [Description("User")]
        User = 1,

        /// <summary>Group mode is for when the module is used by a social group.</summary>
        [Description("Group")]
        Group = 2,
    }
}
