// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager;

using System;
using System.Linq;
using System.Web.UI.WebControls;

using Dnn.Modules.ResourceManager.Components.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.FileSystem;

using Constants = Dnn.Modules.ResourceManager.Components.Constants;
using DnnExceptions = DotNetNuke.Services.Exceptions.Exceptions;

/// <summary>Provides module settings control.</summary>
public partial class Settings : ModuleSettingsBase
{
    /// <inheritdoc/>
    public override void LoadSettings()
    {
        try
        {
            if (this.Page.IsPostBack)
            {
                return;
            }

            var displayTypesValues = Enum.GetValues(typeof(Constants.ModuleModes)).Cast<Constants.ModuleModes>();
            var displayTypes = displayTypesValues.Select(t => new ListItem(Utils.GetEnumDescription(t), ((int)t).ToString())).ToArray();

            this.ddlMode.Items.AddRange(displayTypes);

            this.ddlMode.SelectedValue = this.Settings.Contains(Constants.ModeSettingName)
                ? this.Settings[Constants.ModeSettingName].ToString()
                : Constants.DefaultMode.ToString();

            IFolderInfo homeFolder = null;
            if (this.Settings.Contains(Constants.HomeFolderSettingName))
            {
                int.TryParse(this.Settings[Constants.HomeFolderSettingName].ToString(), out var homeFolderId);
                homeFolder = FolderManager.Instance.GetFolder(homeFolderId);
            }

            if (homeFolder == null)
            {
                homeFolder = FolderManager.Instance.GetFolder(this.PortalId, string.Empty);
            }

            this.ddlFolder.SelectedFolder = homeFolder;
        }
        catch (Exception exc)
        {
            DnnExceptions.ProcessModuleLoadException(this, exc);
        }
    }

    /// <inheritdoc/>
    public override void UpdateSettings()
    {
        try
        {
            var modules = new ModuleController();

            modules.UpdateModuleSetting(this.ModuleId, Constants.ModeSettingName, this.ddlMode.SelectedValue);
            modules.UpdateModuleSetting(this.ModuleId, Constants.HomeFolderSettingName, this.ddlFolder.SelectedFolder.FolderID.ToString());
        }
        catch (Exception exc)
        {
            DnnExceptions.ProcessModuleLoadException(this, exc);
        }
    }
}
