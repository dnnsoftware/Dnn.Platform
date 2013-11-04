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

        public bool IsGroupMode(int moduleId)
        {
            var groupMode = this.GetSettingByKey(moduleId, GroupModeSetting);

            if (string.IsNullOrEmpty(groupMode))
            {
                return false;
            }

            return Convert.ToBoolean(groupMode);
        }

        public void SaveDefaultFolderId(int moduleId, int defaultFolderTypeId)
        {
            this.moduleController.UpdateModuleSetting(moduleId, DefaultFolderTypeIdSetting, defaultFolderTypeId.ToString(CultureInfo.InvariantCulture));            
        }

        public void SaveRootFolderId(int moduleId, int rootFolderId)
        {
            this.moduleController.UpdateModuleSetting(moduleId, RootFolderIdSetting, rootFolderId.ToString(CultureInfo.InvariantCulture));
        }

        public void SaveGroupMode(int moduleId, bool groupMode)
        {
            this.moduleController.UpdateModuleSetting(moduleId, GroupModeSetting, groupMode.ToString(CultureInfo.InvariantCulture));
        }

        private string GetSettingByKey(int moduleId, string key)
        {
            var settings = this.moduleController.GetModuleSettings(moduleId);
            return (string)settings[key];               
        }
    }
}