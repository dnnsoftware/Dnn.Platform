// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.DigitalAssets.Components.Controllers
{
    using System;
    using System.Globalization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Assets;

    public enum DigitalAssestsMode
    {
        Normal,
        Group,
        User,
    }

    public enum FilterCondition
    {
        NotSet,
        FilterByFolder,
    }

    public class DigitalAssetsSettingsRepository
    {
        private const string DefaultFolderTypeIdSetting = "DefaultFolderTypeId";
        private const string RootFolderIdSetting = "RootFolderId";
        private const string GroupModeSetting = "GroupMode";
        private const string ModeSetting = "Mode";
        private const string FilterConditionSetting = "FilterCondition";
        private const string SubfolderFilterSetting = "SubfolderFilter";

        public int? GetDefaultFolderTypeId(int moduleId)
        {
            var defaultFolderTypeId = this.GetSettingByKey(moduleId, DefaultFolderTypeIdSetting);

            if (string.IsNullOrEmpty(defaultFolderTypeId))
            {
                return null;
            }

            return Convert.ToInt32(defaultFolderTypeId);
        }

        public int? GetRootFolderId(int moduleId)
        {
            var rootFolderId = this.GetSettingByKey(moduleId, RootFolderIdSetting);

            if (string.IsNullOrEmpty(rootFolderId))
            {
                return null;
            }

            return Convert.ToInt32(rootFolderId);
        }

        public DigitalAssestsMode GetMode(int moduleId)
        {
            DigitalAssestsMode mode;

            if (!Enum.TryParse(this.GetSettingByKey(moduleId, ModeSetting), true, out mode))
            {
                return this.IsGroupMode(moduleId) ? DigitalAssestsMode.Group : DigitalAssestsMode.Normal;
            }

            return mode;
        }

        public FilterCondition GetFilterCondition(int moduleId)
        {
            var setting = this.GetSettingByKey(moduleId, FilterConditionSetting);
            FilterCondition filterCondition;
            return !Enum.TryParse(setting, true, out filterCondition) ? FilterCondition.NotSet : filterCondition;
        }

        public SubfolderFilter GetSubfolderFilter(int moduleId)
        {
            var setting = this.GetSettingByKey(moduleId, SubfolderFilterSetting);
            SubfolderFilter excludeSubfolders;
            return !Enum.TryParse(setting, true, out excludeSubfolders) ? SubfolderFilter.IncludeSubfoldersFolderStructure : excludeSubfolders;
        }

        public void SaveDefaultFolderTypeId(int moduleId, int defaultFolderTypeId)
        {
            ModuleController.Instance.UpdateModuleSetting(moduleId, DefaultFolderTypeIdSetting, defaultFolderTypeId.ToString(CultureInfo.InvariantCulture));
        }

        public void SaveMode(int moduleId, DigitalAssestsMode mode)
        {
            ModuleController.Instance.UpdateModuleSetting(moduleId, ModeSetting, mode.ToString());
        }

        public void SaveRootFolderId(int moduleId, int rootFolderId)
        {
            ModuleController.Instance.UpdateModuleSetting(moduleId, RootFolderIdSetting, rootFolderId.ToString(CultureInfo.InvariantCulture));
        }

        public void SaveExcludeSubfolders(int moduleId, SubfolderFilter subfolderFilter)
        {
            ModuleController.Instance.UpdateModuleSetting(moduleId, SubfolderFilterSetting, subfolderFilter.ToString());
        }

        public void SaveFilterCondition(int moduleId, FilterCondition filterCondition)
        {
            ModuleController.Instance.UpdateModuleSetting(moduleId, FilterConditionSetting, filterCondition.ToString());
        }

        internal bool SettingExists(int moduleId, string settingName)
        {
            return !string.IsNullOrEmpty(this.GetSettingByKey(moduleId, settingName));
        }

        internal void SetDefaultFilterCondition(int moduleId)
        {
            // handle upgrades where FilterCondition didn't exist
            if (this.SettingExists(moduleId, "RootFolderId") && !this.SettingExists(moduleId, "FilterCondition"))
            {
                this.SaveFilterCondition(moduleId, FilterCondition.FilterByFolder);
            }
        }

        private bool IsGroupMode(int moduleId)
        {
            var groupMode = this.GetSettingByKey(moduleId, GroupModeSetting);

            if (string.IsNullOrEmpty(groupMode))
            {
                return false;
            }

            return Convert.ToBoolean(groupMode);
        }

        private string GetSettingByKey(int moduleId, string key)
        {
            var module = ModuleController.Instance.GetModule(moduleId, Null.NullInteger, true);
            var moduleSettings = module.ModuleSettings;
            return (string)moduleSettings[key];
        }
    }
}
