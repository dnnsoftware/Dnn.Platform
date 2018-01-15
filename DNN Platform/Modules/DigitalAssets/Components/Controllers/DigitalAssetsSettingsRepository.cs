#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Globalization;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Assets;

namespace DotNetNuke.Modules.DigitalAssets.Components.Controllers
{
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

            if (!Enum.TryParse(GetSettingByKey(moduleId, ModeSetting), true, out mode))
            {
                return this.IsGroupMode(moduleId) ? DigitalAssestsMode.Group : DigitalAssestsMode.Normal;
            }

            return mode;
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

        private string GetSettingByKey(int moduleId, string key)
        {
            var module = ModuleController.Instance.GetModule(moduleId, Null.NullInteger, true);
            var moduleSettings = module.ModuleSettings; 
            return (string)moduleSettings[key];               
        }

        internal bool SettingExists(int moduleId, string settingName)
        {
            return !String.IsNullOrEmpty(GetSettingByKey(moduleId, settingName));
        }

        internal void SetDefaultFilterCondition(int moduleId)
        {
            //handle upgrades where FilterCondition didn't exist
            if (this.SettingExists(moduleId, "RootFolderId") && !this.SettingExists(moduleId, "FilterCondition"))
                this.SaveFilterCondition(moduleId, FilterCondition.FilterByFolder);
        }
    }

    public enum DigitalAssestsMode
    {
        Normal,
        Group,
        User
    }

    public enum FilterCondition
    {
        NotSet,
        FilterByFolder
    }
}