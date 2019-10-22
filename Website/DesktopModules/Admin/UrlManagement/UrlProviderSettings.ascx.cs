﻿#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Linq;

using DotNetNuke.Common;
using DotNetNuke.Abstractions;
using DotNetNuke.Entities.Urls;
using DotNetNuke.UI.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetNuke.Modules.UrlManagement
{
    public partial class ProviderSettings : ModuleUserControlBase
    {
        private int _providerId;
        private IExtensionUrlProviderSettingsControl _providerSettingsControl;
        private string DisplayMode => (Request.QueryString["Display"] ?? "").ToLowerInvariant();
        private readonly INavigationManager _navigationManager;

        public ProviderSettings()
        {
            _navigationManager = Globals.DependencyProvider.GetService<INavigationManager>();
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            cmdUpdate.Click += cmdUpdate_Click;
            cmdCancel.Click += cmdCancel_Click;

            _providerId = Convert.ToInt32(Request.Params["ProviderId"]);

            var provider = ExtensionUrlProviderController.GetModuleProviders(ModuleContext.PortalId)
                                .SingleOrDefault(p => p.ProviderConfig.ExtensionUrlProviderId == _providerId);

            if (provider != null)
            {
                var settingsControlSrc = provider.ProviderConfig.SettingsControlSrc;

                var settingsControl = Page.LoadControl(settingsControlSrc);

                providerSettingsPlaceHolder.Controls.Add(settingsControl);

// ReSharper disable SuspiciousTypeConversion.Global
                _providerSettingsControl = settingsControl as IExtensionUrlProviderSettingsControl;
// ReSharper restore SuspiciousTypeConversion.Global
                if (_providerSettingsControl != null)
                {
                    _providerSettingsControl.Provider = provider.ProviderConfig;
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (_providerSettingsControl != null)
            {
                _providerSettingsControl.LoadSettings();
            }

            if (DisplayMode == "editor" || DisplayMode == "settings")
            {
                cmdCancel.Visible = false;
            }
        }

        void cmdCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect(_navigationManager.NavigateURL(ModuleContext.PortalSettings.ActiveTab.TabID));
        }

        void cmdUpdate_Click(object sender, EventArgs e)
        {
            if (!this.Page.IsValid)
            {
                return;
            }

            if (_providerSettingsControl != null)
            {
                var settings = _providerSettingsControl.SaveSettings();
                foreach (var setting in settings)
                {
                    ExtensionUrlProviderController.SaveSetting(_providerId, ModuleContext.PortalId, setting.Key, setting.Value);
                }
            }

            if (DisplayMode != "editor" && DisplayMode != "settings")
            {
                Response.Redirect(_navigationManager.NavigateURL(ModuleContext.PortalSettings.ActiveTab.TabID));
            }
        }
    }
}