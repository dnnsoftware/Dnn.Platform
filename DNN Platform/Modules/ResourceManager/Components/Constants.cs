// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.ComponentModel;
using Dnn.Modules.ResourceManager.Components.Models;
using Dnn.Modules.ResourceManager.Helpers;

namespace Dnn.Modules.ResourceManager.Components
{
    internal class Constants
    {
        #region Misc.
        /// <summary>
        /// The relative path to the shared resource file for this module.
        /// </summary>
        public const string ViewResourceFileName = "~/DesktopModules/ResourceManager/App_LocalResources/ResourceManager.resx";
        public const string ModulePath = "DesktopModules/ResourceManager/";

        public const string ResourceManagerResxDataCacheKey = "ResourceManagerResxResources:{0}:{1}";
        public const string ResourceManagerResxModifiedDateCacheKey = "ResourceManagerResxModifiedDate:{0}";

        public static readonly Localization ResourceManagerLocalization = new Localization
        {
            ResxDataCacheKey = ResourceManagerResxDataCacheKey,
            ResxModifiedDateCacheKey = ResourceManagerResxModifiedDateCacheKey
        };

        public const string UserHasNoPermissionToBrowseFolderDefaultMessage = "The user has no permission to browse this folder";
        public const string GroupFolderPathStart = "Groups/";

        #endregion

        #region Localization

        internal const string LocalizationDataCacheKey = "LocalizationLocTable:{0}:{1}";

        public const string GroupIconCantBeDeletedKey = "GroupIconCantBeDeleted.Error";
        public const string UserHasNoPermissionToDownloadKey = "UserHasNoPermissionToDownload.Error";
        public const string UserHasNoPermissionToAddFoldersKey = "UserHasNoPermissionToAddFolders.Error";

        public static readonly TimeSpan FiveMinutes = TimeSpan.FromMinutes(5);

        #endregion

        #region Module settings

        public const string HomeFolderSettingName = "RM_HomeFolder";
        public const string ModeSettingName = "RM_Mode";

        public const int ItemsPerPage = 20;
        public const int ItemWidth = 176;

        public enum ModuleModes
        {
            [Description("Normal")]
            Normal = 0,
            [Description("User")]
            User = 1,
            [Description("Group")]
            Group = 2
        }

        // Default values
        public const int DefaultMode = 0;
        public static readonly dynamic[] SortingFields = {
            new {value = "LastModifiedOnDate", label = LocalizationHelper.GetString("LastModifiedOnDate")},
            new {value = "CreatedOnDate", label = LocalizationHelper.GetString("CreatedOnDate")},
            new {value = "ItemName", label= LocalizationHelper.GetString("ItemName")}
        };
        public const string DefaultSortingField = "ItemName";

        #endregion
    }
}
