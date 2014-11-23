#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
#region Usings

using System;

using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Analytics.Config;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;

#endregion

namespace DotNetNuke.Modules.Admin.Analytics
{
    
    public partial class GoogleAnalyticsSettings : PortalModuleBase
    {

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cmdUpdate.Click += OnUpdateClick;

            try
            {
                if (Page.IsPostBack == false)
                {
                    var config = AnalyticsConfiguration.GetConfig("GoogleAnalytics");
                    if (config != null)
                    {
                        var trackingId = "";
                        var urlParameter = "";
                        var trackForAdmin = true;
                        foreach (AnalyticsSetting setting in config.Settings)
                        {
                            switch (setting.SettingName.ToLower())
                            {
                                case "trackingid":
                                    trackingId = setting.SettingValue;
                                    break;
                                case "urlparameter":
                                    urlParameter = setting.SettingValue;
                                    break;
                                case "trackforadmin":
                                    if (!bool.TryParse(setting.SettingValue, out trackForAdmin))
                                    {
                                        trackForAdmin = true;
                                    }
                                    break;
                            }
                        }
                        txtTrackingId.Text = trackingId;
                        txtUrlParameter.Text = urlParameter;
                        chkTrackForAdmin.Checked = trackForAdmin;
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void OnUpdateClick(object sender, EventArgs e)
        {
            try
            {
                var config = new AnalyticsConfiguration();
                var setting = new AnalyticsSetting();
                config.Settings = new AnalyticsSettingCollection();
                setting.SettingName = "TrackingId";
                setting.SettingValue = txtTrackingId.Text;
                config.Settings.Add(setting);
                setting = new AnalyticsSetting();
                setting.SettingName = "UrlParameter";
                setting.SettingValue = txtUrlParameter.Text;
                config.Settings.Add(setting);
                setting = new AnalyticsSetting();
                setting.SettingName = "TrackForAdmin";
                setting.SettingValue = chkTrackForAdmin.Checked.ToString().ToLowerInvariant();
                config.Settings.Add(setting);
                AnalyticsConfiguration.SaveConfig("GoogleAnalytics", config);
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("Updated", LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

    }
}