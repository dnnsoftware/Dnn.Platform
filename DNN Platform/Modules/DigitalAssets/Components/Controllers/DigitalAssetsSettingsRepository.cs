#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
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

        public void SaveDefaultFolderId(int moduleId, int defaultFolderTypeId)
        {
            this.moduleController.UpdateModuleSetting(moduleId, DefaultFolderTypeIdSetting, defaultFolderTypeId.ToString(CultureInfo.InvariantCulture));            
        }

        public void SaveRootFolderId(int moduleId, int rootFolderId)
        {
            this.moduleController.UpdateModuleSetting(moduleId, RootFolderIdSetting, rootFolderId.ToString(CultureInfo.InvariantCulture));
        }

        private string GetSettingByKey(int moduleId, string key)
        {
            var settings = this.moduleController.GetModuleSettings(moduleId);
            return (string)settings[key];               
        }
    }
}