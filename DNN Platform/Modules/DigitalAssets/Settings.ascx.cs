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
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI.WebControls;

using DotNetNuke.Entities.Modules;
using DotNetNuke.Modules.DigitalAssets.Components.Controllers;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Web.Client;
using DotNetNuke.Web.Client.ClientResourceManagement;

namespace DotNetNuke.Modules.DigitalAssets
{
    public partial class Settings : ModuleSettingsBase
    {
        private static readonly DigitalAssetsSettingsRepository SettingsRepository = new DigitalAssetsSettingsRepository();

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
                DefaultFolderTypeComboBox.DataSource = new List<FolderMappingInfo>
                    {
                        FolderMappingController.Instance.GetFolderMapping(PortalId, "Standard"),
                        FolderMappingController.Instance.GetFolderMapping(PortalId, "Secure"),
                        FolderMappingController.Instance.GetFolderMapping(PortalId, "Database")
                    };

                DefaultFolderTypeComboBox.DataBind();

                var defaultFolderTypeId = SettingsRepository.GetDefaultFolderTypeId(ModuleId);
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
                DigitalAssestsMode mode;
                Enum.TryParse(ModeComboBox.SelectedValue, true, out mode);
                SettingsRepository.SaveMode(ModuleId, mode);

                UpdateFilterViewSettings();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void ValidateFolderIsSelected(object source, ServerValidateEventArgs args)
        {
            var filterCondition = FilterOptionsRadioButtonsList.SelectedValue;
            if (filterCondition == "FilterByFolder" && FilterByFolderDropDownList.SelectedFolder == null)
            {
                args.IsValid = false;
                return;
            }

            args.IsValid = true;
        }

        private void LoadFilterViewSettings()
        {
            FilterOptionsRadioButtonsList.SelectedValue = SettingsRepository.GetFilterCondition(ModuleId);
            ExcludeSubfoldersRadioButtonList.SelectedValue = SettingsRepository.GetExcludeSubfolders(ModuleId).ToString(CultureInfo.InvariantCulture);
            switch (FilterOptionsRadioButtonsList.SelectedValue)
            {
                case "FilterByFolder":
                    var folderId = SettingsRepository.GetRootFolderId(ModuleId);
                    if (folderId.HasValue)
                    {
                        var folder = FolderManager.Instance.GetFolder(folderId.Value);
                        FilterByFolderDropDownList.SelectedFolder = folder;
                    }
                    break;
            }
        }

        private void UpdateFilterViewSettings()
        {
            SettingsRepository.SaveFilterCondition(ModuleId, FilterOptionsRadioButtonsList.SelectedValue);
            SettingsRepository.SaveExcludeSubfolders(ModuleId, ExcludeSubfoldersRadioButtonList.SelectedValue);
            switch (FilterOptionsRadioButtonsList.SelectedValue)
            {
                case "FilterByFolder":
                    SettingsRepository.SaveRootFolderId(ModuleId, FilterByFolderDropDownList.SelectedFolder.FolderID);
                    break;
            }
        }
    }
}