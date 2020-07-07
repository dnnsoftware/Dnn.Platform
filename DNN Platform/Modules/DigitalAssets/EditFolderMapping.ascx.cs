// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.DigitalAssets
{
    using System;
    using System.Linq;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.FileSystem.Internal;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins.Controls;
    using Microsoft.Extensions.DependencyInjection;

    public partial class EditFolderMapping : PortalModuleBase
    {
        private readonly INavigationManager _navigationManager;

        private readonly IFolderMappingController _folderMappingController = FolderMappingController.Instance;
        private int _folderMappingID = Null.NullInteger;

        public EditFolderMapping()
        {
            this._navigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        public int FolderPortalID
        {
            get
            {
                return this.IsHostMenu ? Null.NullInteger : this.PortalId;
            }
        }

        public int FolderMappingID
        {
            get
            {
                if (this._folderMappingID == Null.NullInteger)
                {
                    if (!string.IsNullOrEmpty(this.Request.QueryString["ItemID"]))
                    {
                        int.TryParse(this.Request.QueryString["ItemID"], out this._folderMappingID);
                    }
                }

                return this._folderMappingID;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (!this.UserInfo.IsSuperUser && !this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName))
            {
                this.Response.Redirect(Globals.AccessDeniedURL(), true);
            }

            this.UpdateButton.Text = (this.FolderMappingID == Null.NullInteger) ? Localization.GetString("Add") : Localization.GetString("Update", this.LocalResourceFile);
            this.CancelHyperLink.NavigateUrl = this.EditUrl("FolderMappings");

            var controlTitle = Localization.GetString("ControlTitle", this.LocalResourceFile);
            var controlTitlePrefix = (this.FolderMappingID == Null.NullInteger) ? Localization.GetString("New") : Localization.GetString("Edit");

            this.SyncWarningPlaceHolder.Visible = this.FolderMappingID != Null.NullInteger;

            this.ModuleConfiguration.ModuleControl.ControlTitle = string.Format(controlTitle, controlTitlePrefix);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.UpdateButton.Click += this.cmdUpdate_Click;

            try
            {
                this.BindFolderMappingSettings();

                if (!this.IsPostBack)
                {
                    this.BindFolderProviders();

                    if (this.FolderMappingID != Null.NullInteger)
                    {
                        this.BindFolderMapping();

                        if (this.ProviderSettingsPlaceHolder.Controls.Count > 0 && this.ProviderSettingsPlaceHolder.Controls[0] is FolderMappingSettingsControlBase)
                        {
                            var folderMapping = this._folderMappingController.GetFolderMapping(this.FolderMappingID);
                            var settingsControl = (FolderMappingSettingsControlBase)this.ProviderSettingsPlaceHolder.Controls[0];
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

        protected void cboFolderProviders_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.BindFolderMappingSettings();
        }

        private void cmdUpdate_Click(object sender, EventArgs e)
        {
            this.Page.Validate("vgEditFolderMapping");

            if (!this.Page.IsValid)
            {
                return;
            }

            try
            {
                var folderMapping = new FolderMappingInfo();

                if (this.FolderMappingID != Null.NullInteger)
                {
                    folderMapping = this._folderMappingController.GetFolderMapping(this.FolderMappingID) ?? new FolderMappingInfo();
                }

                folderMapping.FolderMappingID = this.FolderMappingID;
                folderMapping.MappingName = this.NameTextbox.Text;
                folderMapping.FolderProviderType = this.FolderProvidersComboBox.SelectedValue;
                folderMapping.PortalID = this.FolderPortalID;

                var originalSettings = folderMapping.FolderMappingSettings;

                try
                {
                    var folderMappingID = this.FolderMappingID;

                    if (folderMappingID == Null.NullInteger)
                    {
                        folderMappingID = this._folderMappingController.AddFolderMapping(folderMapping);
                    }
                    else
                    {
                        this._folderMappingController.UpdateFolderMapping(folderMapping);
                    }

                    if (this.ProviderSettingsPlaceHolder.Controls.Count > 0 && this.ProviderSettingsPlaceHolder.Controls[0] is FolderMappingSettingsControlBase)
                    {
                        var settingsControl = (FolderMappingSettingsControlBase)this.ProviderSettingsPlaceHolder.Controls[0];

                        try
                        {
                            settingsControl.UpdateSettings(folderMappingID);
                        }
                        catch
                        {
                            if (this.FolderMappingID == Null.NullInteger)
                            {
                                this._folderMappingController.DeleteFolderMapping(this.FolderPortalID, folderMappingID);
                            }

                            return;
                        }
                    }

                    if (this.FolderMappingID != Null.NullInteger)
                    {
                        // Check if some setting has changed
                        var updatedSettings = this._folderMappingController.GetFolderMappingSettings(this.FolderMappingID);

                        if (originalSettings.Keys.Cast<object>().Any(key => updatedSettings.ContainsKey(key) && !originalSettings[key].ToString().Equals(updatedSettings[key].ToString())))
                        {
                            // Re-synchronize folders using the existing mapping. It's important to synchronize them in descending order
                            var folders = FolderManager.Instance.GetFolders(this.FolderPortalID).Where(f => f.FolderMappingID == this.FolderMappingID).OrderByDescending(f => f.FolderPath);

                            foreach (var folder in folders)
                            {
                                FolderManager.Instance.Synchronize(this.FolderPortalID, folder.FolderPath, false, true);
                            }
                        }
                    }
                }
                catch
                {
                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("DuplicateMappingName", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                    return;
                }

                if (!this.Response.IsRequestBeingRedirected)
                {
                    this.Response.Redirect(this._navigationManager.NavigateURL(this.TabId, "FolderMappings", "mid=" + this.ModuleId, "popUp=true"));
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void BindFolderProviders()
        {
            var defaultProviders = DefaultFolderProviders.GetDefaultProviders();

            foreach (var provider in FolderProvider.GetProviderList().Keys.Where(provider => !defaultProviders.Contains(provider)).OrderBy(provider => provider))
            {
                this.FolderProvidersComboBox.AddItem(provider, provider);
            }

            this.FolderProvidersComboBox.InsertItem(0, string.Empty, string.Empty);
        }

        private void BindFolderMapping()
        {
            var folderMapping = this._folderMappingController.GetFolderMapping(this.FolderMappingID);

            this.NameTextbox.Text = folderMapping.MappingName;

            this.FolderProvidersComboBox.SelectedValue = folderMapping.FolderProviderType;
            this.FolderProvidersComboBox.Enabled = false;
        }

        private void BindFolderMappingSettings()
        {
            string folderProviderType;

            if (this.FolderMappingID != Null.NullInteger)
            {
                var folderMapping = this._folderMappingController.GetFolderMapping(this.FolderMappingID);
                folderProviderType = folderMapping.FolderProviderType;
            }
            else
            {
                folderProviderType = this.FolderProvidersComboBox.SelectedValue;
            }

            if (string.IsNullOrEmpty(folderProviderType))
            {
                return;
            }

            var settingsControlVirtualPath = FolderProvider.Instance(folderProviderType).GetSettingsControlVirtualPath();
            if (string.IsNullOrEmpty(settingsControlVirtualPath))
            {
                return;
            }

            var settingsControl = this.LoadControl(settingsControlVirtualPath);
            if (settingsControl == null || !(settingsControl is FolderMappingSettingsControlBase))
            {
                return;
            }

            // This is important to allow settings control to be localizable
            var baseType = settingsControl.GetType().BaseType;
            if (baseType != null)
            {
                settingsControl.ID = baseType.Name;
            }

            this.ProviderSettingsPlaceHolder.Controls.Clear();
            this.ProviderSettingsPlaceHolder.Controls.Add(settingsControl);
        }
    }
}
