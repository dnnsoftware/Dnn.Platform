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
using System.Collections.Generic;
using System.Web.UI.WebControls;

using DotNetNuke.Entities.Modules;
using DotNetNuke.Modules.DigitalAssets.Components.Controllers;
using DotNetNuke.Services.Assets;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Web.Client;
using DotNetNuke.Web.Client.ClientResourceManagement;

namespace DotNetNuke.Modules.DigitalAssets
{
    public partial class Settings : ModuleSettingsBase
    {
        private static readonly DigitalAssetsSettingsRepository SettingsRepository = new DigitalAssetsSettingsRepository();

        private DigitalAssestsMode SelectedDigitalAssestsMode
        {
            get
            {
                DigitalAssestsMode mode;
                Enum.TryParse(ModeComboBox.SelectedValue, true, out mode);
                return mode;
            }
        }

        private FilterCondition SelectedFilterCondition
        {
            get
            {
                FilterCondition filterCondition;
                Enum.TryParse(FilterOptionsRadioButtonsList.SelectedValue, true, out filterCondition);
                return filterCondition;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            ClientResourceManager.RegisterScript(Page, "~/DesktopModules/DigitalAssets/ClientScripts/dnn.DigitalAssets.FilterViewSettings.js", FileOrder.Js.DefaultPriority);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LoadSettings loads the settings from the Database and displays them
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void LoadSettings()
        {
            if (Page.IsPostBack)
            {
                return;
            }

            try
            {
                DefaultFolderTypeComboBox.DataSource = FolderMappingController.Instance.GetFolderMappings(PortalId);
                DefaultFolderTypeComboBox.DataBind();

                var defaultFolderTypeId = new DigitalAssetsController().GetDefaultFolderTypeId(ModuleId);
                if (defaultFolderTypeId.HasValue)
                {
                    DefaultFolderTypeComboBox.SelectedValue = defaultFolderTypeId.ToString();
                }

                ModeComboBox.SelectedValue = SettingsRepository.GetMode(ModuleId).ToString();

                LoadFilterViewSettings();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UpdateSettings saves the modified settings to the Database
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void UpdateSettings()
        {
            Page.Validate();
            if (!Page.IsValid) return;

            try
            {
                SettingsRepository.SaveDefaultFolderTypeId(ModuleId, Convert.ToInt32(DefaultFolderTypeComboBox.SelectedValue));
                
                SettingsRepository.SaveMode(ModuleId, SelectedDigitalAssestsMode);

                UpdateFilterViewSettings();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void ValidateFolderIsSelected(object source, ServerValidateEventArgs args)
        {
            if (SelectedFilterCondition == FilterCondition.FilterByFolder && FilterByFolderDropDownList.SelectedFolder == null)
            {
                args.IsValid = false;
                return;
            }

            args.IsValid = true;
        }

        private void LoadFilterViewSettings()
        {
            //handle upgrades where FilterCondition didn't exist
            SettingsRepository.SetDefaultFilterCondition(ModuleId);

            FilterOptionsRadioButtonsList.SelectedValue = SettingsRepository.GetFilterCondition(ModuleId).ToString();
            SubfolderFilterRadioButtonList.SelectedValue = SettingsRepository.GetSubfolderFilter(ModuleId).ToString();
            
            if (FilterOptionsRadioButtonsList.SelectedValue == FilterCondition.FilterByFolder.ToString())
            {
                var folderId = SettingsRepository.GetRootFolderId(ModuleId);
                if (folderId.HasValue)
                {
                    var folder = FolderManager.Instance.GetFolder(folderId.Value);
                    FilterByFolderDropDownList.SelectedFolder = folder;
                }
            }
        }

        private void UpdateFilterViewSettings()
        {
            var filterCondition = SelectedDigitalAssestsMode != DigitalAssestsMode.Normal ? FilterCondition.NotSet : SelectedFilterCondition;

            SettingsRepository.SaveFilterCondition(ModuleId, filterCondition);

            switch (filterCondition)
            {
                case FilterCondition.NotSet:
                    SettingsRepository.SaveExcludeSubfolders(ModuleId, SubfolderFilter.IncludeSubfoldersFolderStructure);
                    break;
                case FilterCondition.FilterByFolder:
                    SubfolderFilter subfolderFilter;
                    Enum.TryParse(SubfolderFilterRadioButtonList.SelectedValue, true, out subfolderFilter);
                    SettingsRepository.SaveExcludeSubfolders(ModuleId, subfolderFilter);
                    SettingsRepository.SaveRootFolderId(ModuleId, FilterByFolderDropDownList.SelectedFolder.FolderID);
                    break;
            }
        }
    }
}