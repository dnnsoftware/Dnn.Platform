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
        private readonly INavigationManager navigationManager;

        private int providerId;
        private IExtensionUrlProviderSettingsControl providerSettingsControl;

        /// <summary>Initializes a new instance of the <see cref="ProviderSettings"/> class.</summary>
        public ProviderSettings()
        {
            this.navigationManager = Globals.DependencyProvider.GetService<INavigationManager>();
        }

        private string DisplayMode => (this.Request.QueryString["Display"] ?? string.Empty).ToLowerInvariant();

        /// <inheritdoc/>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.cmdUpdate.Click += this.CmdUpdate_Click;
            this.cmdCancel.Click += this.CmdCancel_Click;

            this.providerId = Convert.ToInt32(this.Request.Params["ProviderId"]);

            var provider = ExtensionUrlProviderController.GetModuleProviders(this.ModuleContext.PortalId)
                                .SingleOrDefault(p => p.ProviderConfig.ExtensionUrlProviderId == this.providerId);

            if (provider != null)
            {
                var settingsControlSrc = provider.ProviderConfig.SettingsControlSrc;

                var settingsControl = this.Page.LoadControl(settingsControlSrc);

                this.providerSettingsPlaceHolder.Controls.Add(settingsControl);

                // ReSharper disable SuspiciousTypeConversion.Global
                this.providerSettingsControl = settingsControl as IExtensionUrlProviderSettingsControl;

                // ReSharper restore SuspiciousTypeConversion.Global
                if (this.providerSettingsControl != null)
                {
                    this.providerSettingsControl.Provider = provider.ProviderConfig;
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (this.providerSettingsControl != null)
            {
                this.providerSettingsControl.LoadSettings();
            }

            if (this.DisplayMode == "editor" || this.DisplayMode == "settings")
            {
                this.cmdCancel.Visible = false;
            }
        }

        private void CmdCancel_Click(object sender, EventArgs e)
        {
            this.Response.Redirect(this.navigationManager.NavigateURL(this.ModuleContext.PortalSettings.ActiveTab.TabID));
        }

        private void CmdUpdate_Click(object sender, EventArgs e)
        {
            if (!this.Page.IsValid)
            {
                return;
            }

            if (this.providerSettingsControl != null)
            {
                var settings = this.providerSettingsControl.SaveSettings();
                foreach (var setting in settings)
                {
                    ExtensionUrlProviderController.SaveSetting(this.providerId, this.ModuleContext.PortalId, setting.Key, setting.Value);
                }
            }

            if (this.DisplayMode != "editor" && this.DisplayMode != "settings")
            {
                this.Response.Redirect(this.navigationManager.NavigateURL(this.ModuleContext.PortalSettings.ActiveTab.TabID));
            }
        }
    }
}
