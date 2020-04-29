/*
' Copyright (c) 2017 DNN Software, Inc.
'  All rights reserved.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
' DEALINGS IN THE SOFTWARE.
' 
*/

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
        internal const string LocalizationDataModifiedDateCacheKey = "LocalizationDataModifiedDate:{0}";

        public const string GroupIconCantBeDeletedKey = "GroupIconCantBeDeleted.Error";
        public const string UserHasNoPermissionToDownloadKey = "UserHasNoPermissionToDownload.Error";
        public const string UserHasNoPermissionToAddFoldersKey = "UserHasNoPermissionToAddFolders.Error";

        public static readonly TimeSpan FiveMinutes = TimeSpan.FromMinutes(5);

        #endregion

        #region Module settings

        public const string AddFileContentType = "File";
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
