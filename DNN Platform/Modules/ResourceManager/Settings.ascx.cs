// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Linq;
using System.Web.UI.WebControls;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.FileSystem;
using Dnn.Modules.ResourceManager.Components.Common;
using DnnExceptions = DotNetNuke.Services.Exceptions.Exceptions;
using Constants = Dnn.Modules.ResourceManager.Components.Constants;


namespace Dnn.Modules.ResourceManager
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The Settings class manages Module Settings
    /// 
    /// Typically your settings control would be used to manage settings for your module.
    /// There are two types of settings, ModuleSettings, and TabModuleSettings.
    /// 
    /// ModuleSettings apply to all "copies" of a module on a site, no matter which page the module is on. 
    /// 
    /// TabModuleSettings apply only to the current module on the current page, if you copy that module to
    /// another page the settings are not transferred.
    /// 
    /// If you happen to save both TabModuleSettings and ModuleSettings, TabModuleSettings overrides ModuleSettings.
    /// 
    /// Below we have some examples of how to access these settings but you will need to uncomment to use.
    /// 
    /// Because the control inherits from ResourceManagerSettingsBase you have access to any custom properties
    /// defined there, as well as properties from DNN such as PortalId, ModuleId, TabId, UserId and many more.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class Settings : ModuleSettingsBase
    {
        #region Base Method Implementations

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LoadSettings loads the settings from the Database and displays them
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void LoadSettings()
        {
            try
            {
                if (Page.IsPostBack) return;

                var displayTypesValues = Enum.GetValues(typeof(Constants.ModuleModes)).Cast<Constants.ModuleModes>();
                var displayTypes = displayTypesValues.Select(t => new ListItem(Utils.GetEnumDescription(t), ((int)t).ToString())).ToArray();

                ddlMode.Items.AddRange(displayTypes);

                ddlMode.SelectedValue = Settings.Contains(Constants.ModeSettingName)
                    ? Settings[Constants.ModeSettingName].ToString()
                    : Constants.DefaultMode.ToString();

                IFolderInfo homeFolder = null;
                if (Settings.Contains(Constants.HomeFolderSettingName))
                {
                    int homeFolderId;
                    int.TryParse(Settings[Constants.HomeFolderSettingName].ToString(), out homeFolderId);
                    homeFolder = FolderManager.Instance.GetFolder(homeFolderId);
                }

                if (homeFolder == null)
                {
                    homeFolder = FolderManager.Instance.GetFolder(PortalId, "");
                }

                ddlFolder.SelectedFolder = homeFolder;
            }
            catch (Exception exc) //Module failed to load
            {
                DnnExceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UpdateSettings saves the modified settings to the Database
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void UpdateSettings()
        {
            try
            {
                var modules = new ModuleController();

                modules.UpdateModuleSetting(ModuleId, Constants.ModeSettingName, ddlMode.SelectedValue);
                modules.UpdateModuleSetting(ModuleId, Constants.HomeFolderSettingName, ddlFolder.SelectedFolder.FolderID.ToString());
            }
            catch (Exception exc) //Module failed to load
            {
                DnnExceptions.ProcessModuleLoadException(this, exc);
            }
        }

        #endregion
    }
}