// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.DigitalAssets
{
    using System;
    using System.Collections.Generic;
    using System.Web.UI.WebControls;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Modules.DigitalAssets.Components.Controllers;
    using DotNetNuke.Services.Assets;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    public partial class Settings : ModuleSettingsBase
    {
        private static readonly DigitalAssetsSettingsRepository SettingsRepository = new DigitalAssetsSettingsRepository();

        private DigitalAssestsMode SelectedDigitalAssestsMode
        {
            get
            {
                DigitalAssestsMode mode;
                Enum.TryParse(this.ModeComboBox.SelectedValue, true, out mode);
                return mode;
            }
        }

        private FilterCondition SelectedFilterCondition
        {
            get
            {
                FilterCondition filterCondition;
                Enum.TryParse(this.FilterOptionsRadioButtonsList.SelectedValue, true, out filterCondition);
                return filterCondition;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LoadSettings loads the settings from the Database and displays them.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void LoadSettings()
        {
            if (this.Page.IsPostBack)
            {
                return;
            }

            try
            {
                this.DefaultFolderTypeComboBox.DataSource = FolderMappingController.Instance.GetFolderMappings(this.PortalId);
                this.DefaultFolderTypeComboBox.DataBind();

                var defaultFolderTypeId = new DigitalAssetsController().GetDefaultFolderTypeId(this.ModuleId);
                if (defaultFolderTypeId.HasValue)
                {
                    this.DefaultFolderTypeComboBox.SelectedValue = defaultFolderTypeId.ToString();
                }

                this.ModeComboBox.SelectedValue = SettingsRepository.GetMode(this.ModuleId).ToString();

                this.LoadFilterViewSettings();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UpdateSettings saves the modified settings to the Database.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void UpdateSettings()
        {
            this.Page.Validate();
            if (!this.Page.IsValid)
            {
                return;
            }

            try
            {
                SettingsRepository.SaveDefaultFolderTypeId(this.ModuleId, Convert.ToInt32(this.DefaultFolderTypeComboBox.SelectedValue));

                SettingsRepository.SaveMode(this.ModuleId, this.SelectedDigitalAssestsMode);

                this.UpdateFilterViewSettings();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected override void OnInit(EventArgs e)
        {
            ClientResourceManager.RegisterScript(this.Page, "~/DesktopModules/DigitalAssets/ClientScripts/dnn.DigitalAssets.FilterViewSettings.js", FileOrder.Js.DefaultPriority);
        }

        protected void ValidateFolderIsSelected(object source, ServerValidateEventArgs args)
        {
            if (this.SelectedFilterCondition == FilterCondition.FilterByFolder && this.FilterByFolderDropDownList.SelectedFolder == null)
            {
                args.IsValid = false;
                return;
            }

            args.IsValid = true;
        }

        private void LoadFilterViewSettings()
        {
            // handle upgrades where FilterCondition didn't exist
            SettingsRepository.SetDefaultFilterCondition(this.ModuleId);

            this.FilterOptionsRadioButtonsList.SelectedValue = SettingsRepository.GetFilterCondition(this.ModuleId).ToString();
            this.SubfolderFilterRadioButtonList.SelectedValue = SettingsRepository.GetSubfolderFilter(this.ModuleId).ToString();

            if (this.FilterOptionsRadioButtonsList.SelectedValue == FilterCondition.FilterByFolder.ToString())
            {
                var folderId = SettingsRepository.GetRootFolderId(this.ModuleId);
                if (folderId.HasValue)
                {
                    var folder = FolderManager.Instance.GetFolder(folderId.Value);
                    this.FilterByFolderDropDownList.SelectedFolder = folder;
                }
            }
        }

        private void UpdateFilterViewSettings()
        {
            var filterCondition = this.SelectedDigitalAssestsMode != DigitalAssestsMode.Normal ? FilterCondition.NotSet : this.SelectedFilterCondition;

            SettingsRepository.SaveFilterCondition(this.ModuleId, filterCondition);

            switch (filterCondition)
            {
                case FilterCondition.NotSet:
                    SettingsRepository.SaveExcludeSubfolders(this.ModuleId, SubfolderFilter.IncludeSubfoldersFolderStructure);
                    break;
                case FilterCondition.FilterByFolder:
                    SubfolderFilter subfolderFilter;
                    Enum.TryParse(this.SubfolderFilterRadioButtonList.SelectedValue, true, out subfolderFilter);
                    SettingsRepository.SaveExcludeSubfolders(this.ModuleId, subfolderFilter);
                    SettingsRepository.SaveRootFolderId(this.ModuleId, this.FilterByFolderDropDownList.SelectedFolder.FolderID);
                    break;
            }
        }
    }
}
