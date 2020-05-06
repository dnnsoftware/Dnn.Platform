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
    public partial class Settings : ModuleSettingsBase
    {
        #region Base Method Implementations

        public override void LoadSettings()
        {
            try
            {
                if (Page.IsPostBack) 
                    return;

                var displayTypesValues = Enum.GetValues(typeof(Constants.ModuleModes)).Cast<Constants.ModuleModes>();
                var displayTypes = displayTypesValues.Select(t => new ListItem(Utils.GetEnumDescription(t), ((int)t).ToString())).ToArray();

                ddlMode.Items.AddRange(displayTypes);

                ddlMode.SelectedValue = Settings.Contains(Constants.ModeSettingName)
                    ? Settings[Constants.ModeSettingName].ToString()
                    : Constants.DefaultMode.ToString();

                IFolderInfo homeFolder = null;
                if (Settings.Contains(Constants.HomeFolderSettingName))
                {
                    int.TryParse(Settings[Constants.HomeFolderSettingName].ToString(), out var homeFolderId);
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