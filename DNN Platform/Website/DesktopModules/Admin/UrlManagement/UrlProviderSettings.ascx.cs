// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.UrlManagement
{
    using System;
    using System.Linq;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Urls;
    using DotNetNuke.UI.Modules;
    using Microsoft.Extensions.DependencyInjection;

    public partial class ProviderSettings : ModuleUserControlBase
    {
        private readonly INavigationManager _navigationManager;

        private int _providerId;
        private IExtensionUrlProviderSettingsControl _providerSettingsControl;

        public ProviderSettings()
        {
            this._navigationManager = Globals.DependencyProvider.GetService<INavigationManager>();
        }

        private string DisplayMode => (this.Request.QueryString["Display"] ?? string.Empty).ToLowerInvariant();

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.cmdUpdate.Click += this.cmdUpdate_Click;
            this.cmdCancel.Click += this.cmdCancel_Click;

            this._providerId = Convert.ToInt32(this.Request.Params["ProviderId"]);

            var provider = ExtensionUrlProviderController.GetModuleProviders(this.ModuleContext.PortalId)
                                .SingleOrDefault(p => p.ProviderConfig.ExtensionUrlProviderId == this._providerId);

            if (provider != null)
            {
                var settingsControlSrc = provider.ProviderConfig.SettingsControlSrc;

                var settingsControl = this.Page.LoadControl(settingsControlSrc);

                this.providerSettingsPlaceHolder.Controls.Add(settingsControl);

                // ReSharper disable SuspiciousTypeConversion.Global
                this._providerSettingsControl = settingsControl as IExtensionUrlProviderSettingsControl;

                // ReSharper restore SuspiciousTypeConversion.Global
                if (this._providerSettingsControl != null)
                {
                    this._providerSettingsControl.Provider = provider.ProviderConfig;
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (this._providerSettingsControl != null)
            {
                this._providerSettingsControl.LoadSettings();
            }

            if (this.DisplayMode == "editor" || this.DisplayMode == "settings")
            {
                this.cmdCancel.Visible = false;
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            this.Response.Redirect(this._navigationManager.NavigateURL(this.ModuleContext.PortalSettings.ActiveTab.TabID));
        }

        private void cmdUpdate_Click(object sender, EventArgs e)
        {
            if (!this.Page.IsValid)
            {
                return;
            }

            if (this._providerSettingsControl != null)
            {
                var settings = this._providerSettingsControl.SaveSettings();
                foreach (var setting in settings)
                {
                    ExtensionUrlProviderController.SaveSetting(this._providerId, this.ModuleContext.PortalId, setting.Key, setting.Value);
                }
            }

            if (this.DisplayMode != "editor" && this.DisplayMode != "settings")
            {
                this.Response.Redirect(this._navigationManager.NavigateURL(this.ModuleContext.PortalSettings.ActiveTab.TabID));
            }
        }
    }
}
