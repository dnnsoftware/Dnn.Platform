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
using System.Linq;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.FileSystem.Internal;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;

namespace DotNetNuke.Modules.DigitalAssets
{
    public partial class EditFolderMapping : PortalModuleBase
    {
        #region Private Variables

        private readonly IFolderMappingController _folderMappingController = FolderMappingController.Instance;
        private int _folderMappingID = Null.NullInteger;

        #endregion

        #region Properties

        public int FolderPortalID
        {
            get
            {
                return IsHostMenu ? Null.NullInteger : PortalId;
            }
        }

        public int FolderMappingID
        {
            get
            {
                if (_folderMappingID == Null.NullInteger)
                {
                    if (!string.IsNullOrEmpty(Request.QueryString["ItemID"]))
                    {
                        int.TryParse(Request.QueryString["ItemID"], out _folderMappingID);
                    }
                }
                return _folderMappingID;
            }
        }

        #endregion

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (!UserInfo.IsSuperUser && !UserInfo.IsInRole(PortalSettings.AdministratorRoleName))
            {
                Response.Redirect(Globals.AccessDeniedURL(), true);
            }

            UpdateButton.Text = (FolderMappingID == Null.NullInteger) ? Localization.GetString("Add") : Localization.GetString("Update", LocalResourceFile);
            CancelHyperLink.NavigateUrl = EditUrl("FolderMappings");

            var controlTitle = Localization.GetString("ControlTitle", LocalResourceFile);
            var controlTitlePrefix = (FolderMappingID == Null.NullInteger) ? Localization.GetString("New") : Localization.GetString("Edit");

            SyncWarningPlaceHolder.Visible = (FolderMappingID != Null.NullInteger);

            ModuleConfiguration.ModuleControl.ControlTitle = string.Format(controlTitle, controlTitlePrefix);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            UpdateButton.Click += cmdUpdate_Click;

            try
            {
                BindFolderMappingSettings();

                if (!IsPostBack)
                {
                    BindFolderProviders();

                    if (FolderMappingID != Null.NullInteger)
                    {
                        BindFolderMapping();

                        if (ProviderSettingsPlaceHolder.Controls.Count > 0 && ProviderSettingsPlaceHolder.Controls[0] is FolderMappingSettingsControlBase)
                        {
                            var folderMapping = _folderMappingController.GetFolderMapping(FolderMappingID);
                            var settingsControl = (FolderMappingSettingsControlBase)ProviderSettingsPlaceHolder.Controls[0];
                            settingsControl.LoadSettings(folderMapping.FolderMappingSettings);
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void cmdUpdate_Click(object sender, EventArgs e)
        {
            Page.Validate("vgEditFolderMapping");
            
            if (!Page.IsValid) return;

            try
            {
                var folderMapping = new FolderMappingInfo();

                if (FolderMappingID != Null.NullInteger)
                {
                    folderMapping = _folderMappingController.GetFolderMapping(FolderMappingID) ?? new FolderMappingInfo();
                }

                folderMapping.FolderMappingID = FolderMappingID;
                folderMapping.MappingName = NameTextbox.Text;
                folderMapping.FolderProviderType = FolderProvidersComboBox.SelectedValue;
                folderMapping.PortalID = FolderPortalID;

                var originalSettings = folderMapping.FolderMappingSettings;

                try
                {
                    var folderMappingID = FolderMappingID;

                    if (folderMappingID == Null.NullInteger)
                    {
                        folderMappingID = _folderMappingController.AddFolderMapping(folderMapping);
                    }
                    else
                    {
                        _folderMappingController.UpdateFolderMapping(folderMapping);
                    }

                    if (ProviderSettingsPlaceHolder.Controls.Count > 0 && ProviderSettingsPlaceHolder.Controls[0] is FolderMappingSettingsControlBase)
                    {
                        var settingsControl = (FolderMappingSettingsControlBase)ProviderSettingsPlaceHolder.Controls[0];

                        try
                        {
                            settingsControl.UpdateSettings(folderMappingID);
                        }
                        catch
                        {
                            if (FolderMappingID == Null.NullInteger)
                            {
                                _folderMappingController.DeleteFolderMapping(FolderPortalID, folderMappingID);
                            }
                            return;
                        }
                    }

                    if (FolderMappingID != Null.NullInteger)
                    {
                        // Check if some setting has changed
                        var updatedSettings = _folderMappingController.GetFolderMappingSettings(FolderMappingID);

                        if (originalSettings.Keys.Cast<object>().Any(key => updatedSettings.ContainsKey(key) && !originalSettings[key].ToString().Equals(updatedSettings[key].ToString())))
                        {
                            // Re-synchronize folders using the existing mapping. It's important to synchronize them in descending order
                            var folders = FolderManager.Instance.GetFolders(FolderPortalID).Where(f => f.FolderMappingID == FolderMappingID).OrderByDescending(f => f.FolderPath);

                            foreach (var folder in folders)
                            {
                                FolderManager.Instance.Synchronize(FolderPortalID, folder.FolderPath, false, true);
                            }
                        }
                    }
                }
                catch
                {
                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("DuplicateMappingName", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                    return;
                }

                if (!Response.IsRequestBeingRedirected) 
                    Response.Redirect(Globals.NavigateURL(TabId, "FolderMappings", "mid=" + ModuleId, "popUp=true"));
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void cboFolderProviders_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindFolderMappingSettings();
        }

        #endregion

        #region Private Methods

        private void BindFolderProviders()
        {
            var defaultProviders = DefaultFolderProviders.GetDefaultProviders();

            foreach (var provider in FolderProvider.GetProviderList().Keys.Where(provider => !defaultProviders.Contains(provider)).OrderBy(provider => provider))
            {
                FolderProvidersComboBox.AddItem(provider, provider);
            }
            FolderProvidersComboBox.InsertItem(0, "", "");
        }

        private void BindFolderMapping()
        {
            var folderMapping = _folderMappingController.GetFolderMapping(FolderMappingID);

            NameTextbox.Text = folderMapping.MappingName;

            FolderProvidersComboBox.SelectedValue = folderMapping.FolderProviderType;
            FolderProvidersComboBox.Enabled = false;
        }

        private void BindFolderMappingSettings()
        {
            string folderProviderType;

            if (FolderMappingID != Null.NullInteger)
            {
                var folderMapping = _folderMappingController.GetFolderMapping(FolderMappingID);
                folderProviderType = folderMapping.FolderProviderType;
            }
            else
            {
                folderProviderType = FolderProvidersComboBox.SelectedValue;
            }

            if (string.IsNullOrEmpty(folderProviderType)) return;
            
            var settingsControlVirtualPath = FolderProvider.Instance(folderProviderType).GetSettingsControlVirtualPath();
            if (String.IsNullOrEmpty(settingsControlVirtualPath)) return;

            var settingsControl = LoadControl(settingsControlVirtualPath);
            if (settingsControl == null || !(settingsControl is FolderMappingSettingsControlBase)) return;

            // This is important to allow settings control to be localizable
            var baseType = settingsControl.GetType().BaseType;
            if (baseType != null)
            {
                settingsControl.ID = baseType.Name;
            }

            ProviderSettingsPlaceHolder.Controls.Clear();
            ProviderSettingsPlaceHolder.Controls.Add(settingsControl);
        }

        #endregion
    }    
}