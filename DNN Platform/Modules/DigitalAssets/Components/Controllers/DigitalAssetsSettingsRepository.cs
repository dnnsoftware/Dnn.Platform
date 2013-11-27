#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using DotNetNuke.Entities.Modules;

namespace DotNetNuke.Modules.DigitalAssets.Components.Controllers
{
    public class DigitalAssetsSettingsRepository
    {
        private const string DefaultFolderTypeIdSetting = "DefaultFolderTypeId";
        private const string RootFolderIdSetting = "RootFolderId";
        private const string GroupModeSetting = "GroupMode";
        private const string ModeSetting = "Mode";
        private const string FilterConditionSetting = "FilterCondition";
        private const string ExcludeSubfoldersSetting = "ExcludeSubfolders";

        private const string DefaultFilterCondition = "NoSet";

        private readonly ModuleController moduleController;

        public DigitalAssetsSettingsRepository()
        {
            this.moduleController = new ModuleController();
        }

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

        public string GetFilterCondition(int moduleId)
        {
            var viewCondition = this.GetSettingByKey(moduleId, FilterConditionSetting);
            return viewCondition ?? DefaultFilterCondition;
        }

        public int GetExcludeSubfolders(int moduleId)
        {
            var value = this.GetSettingByKey(moduleId, ExcludeSubfoldersSetting);
            int settingValue;
            return int.TryParse(value, out settingValue) ? settingValue : (int)ExcludeSubfolderSettingValues.ExcludeSubfoldersValue;
        }
        
        public void SaveDefaultFolderTypeId(int moduleId, int defaultFolderTypeId)
        {
            this.moduleController.UpdateModuleSetting(moduleId, DefaultFolderTypeIdSetting, defaultFolderTypeId.ToString(CultureInfo.InvariantCulture));            
        }

        public void SaveMode(int moduleId, DigitalAssestsMode mode)
        {
            this.moduleController.UpdateModuleSetting(moduleId, ModeSetting, mode.ToString());
        }

        public void SaveRootFolderId(int moduleId, int rootFolderId)
        {
            this.moduleController.UpdateModuleSetting(moduleId, RootFolderIdSetting, rootFolderId.ToString(CultureInfo.InvariantCulture));
        }

        public void SaveExcludeSubfolders(int moduleId, string excludeSubfolders)
        {
            moduleController.UpdateModuleSetting(moduleId, ExcludeSubfoldersSetting, excludeSubfolders);
        }

        public void SaveFilterCondition(int moduleId, string filterCondition)
        {
            moduleController.UpdateModuleSetting(moduleId, FilterConditionSetting, filterCondition);
        }

        private string GetSettingByKey(int moduleId, string key)
        {
            var settings = this.moduleController.GetModuleSettings(moduleId);
            return (string)settings[key];               
        }
    }

    public enum DigitalAssestsMode
    {
        Normal,
        Group,
        User
    }

    public enum ExcludeSubfolderSettingValues
    {
        ExcludeSubfoldersValue = 0,
        IncludeSubfoldersFilesOnlyValue = 1,
        IncludeSubfoldersFolderStructureValue = 2
    }
}