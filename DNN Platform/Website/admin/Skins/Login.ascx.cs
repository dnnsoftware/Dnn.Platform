// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;
    using System.Web;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using Microsoft.Extensions.DependencyInjection;

    /// -----------------------------------------------------------------------------
    /// <summary></summary>
    /// <remarks></remarks>
    /// -----------------------------------------------------------------------------
    public partial class Login : SkinObjectBase
    {
        private const string MyFileName = "Login.ascx";

        private readonly INavigationManager _navigationManager;

        public Login()
        {
            this._navigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
            this.LegacyMode = true;
        }

        public string Text { get; set; }

        public string CssClass { get; set; }

        public string LogoffText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether set this to false in the skin to take advantage of the enhanced markup.
        /// </summary>
        public bool LegacyMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether set this to true to show in custom 404/500 page.
        /// </summary>
        public bool ShowInErrorPage { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.Visible = (!this.PortalSettings.HideLoginControl || this.Request.IsAuthenticated)
                        && (!this.PortalSettings.InErrorPageRequest() || this.ShowInErrorPage);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (this.Visible)
            {
                try
                {
                    if (this.LegacyMode)
                    {
                        this.loginLink.Visible = true;
                        this.loginGroup.Visible = false;
                    }
                    else
                    {
                        this.loginLink.Visible = false;
                        this.loginGroup.Visible = true;
                    }

                    if (!string.IsNullOrEmpty(this.CssClass))
                    {
                        this.loginLink.CssClass = this.CssClass;
                        this.enhancedLoginLink.CssClass = this.CssClass;
                    }

                    if (this.Request.IsAuthenticated)
                    {
                        if (!string.IsNullOrEmpty(this.LogoffText))
                        {
                            if (this.LogoffText.IndexOf("src=") != -1)
                            {
                                this.LogoffText = this.LogoffText.Replace("src=\"", "src=\"" + this.PortalSettings.ActiveTab.SkinPath);
                            }

                            this.loginLink.Text = this.LogoffText;
                            this.enhancedLoginLink.Text = this.LogoffText;
                        }
                        else
                        {
                            this.loginLink.Text = Localization.GetString("Logout", Localization.GetResourceFile(this, MyFileName));
                            this.enhancedLoginLink.Text = this.loginLink.Text;
                            this.loginLink.ToolTip = this.loginLink.Text;
                            this.enhancedLoginLink.ToolTip = this.loginLink.Text;
                        }

                        this.loginLink.NavigateUrl = this._navigationManager.NavigateURL(this.PortalSettings.ActiveTab.TabID, "Logoff");
                        this.enhancedLoginLink.NavigateUrl = this.loginLink.NavigateUrl;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(this.Text))
                        {
                            if (this.Text.IndexOf("src=") != -1)
                            {
                                this.Text = this.Text.Replace("src=\"", "src=\"" + this.PortalSettings.ActiveTab.SkinPath);
                            }

                            this.loginLink.Text = this.Text;
                            this.enhancedLoginLink.Text = this.Text;
                        }
                        else
                        {
                            this.loginLink.Text = Localization.GetString("Login", Localization.GetResourceFile(this, MyFileName));
                            this.enhancedLoginLink.Text = this.loginLink.Text;
                            this.loginLink.ToolTip = this.loginLink.Text;
                            this.enhancedLoginLink.ToolTip = this.loginLink.Text;
                        }

                        string returnUrl = HttpContext.Current.Request.RawUrl;
                        if (returnUrl.IndexOf("?returnurl=") != -1)
                        {
                            returnUrl = returnUrl.Substring(0, returnUrl.IndexOf("?returnurl="));
                        }

                        returnUrl = HttpUtility.UrlEncode(returnUrl);

                        this.loginLink.NavigateUrl = Globals.LoginURL(returnUrl, this.Request.QueryString["override"] != null);
                        this.enhancedLoginLink.NavigateUrl = this.loginLink.NavigateUrl;

                        // avoid issues caused by multiple clicks of login link
                        var oneclick = "this.disabled=true;";
                        if (this.Request.UserAgent != null && this.Request.UserAgent.Contains("MSIE 8.0") == false)
                        {
                            this.loginLink.Attributes.Add("onclick", oneclick);
                            this.enhancedLoginLink.Attributes.Add("onclick", oneclick);
                        }

                        if (this.PortalSettings.EnablePopUps && this.PortalSettings.LoginTabId == Null.NullInteger && !AuthenticationController.HasSocialAuthenticationEnabled(this))
                        {
                            // To avoid duplicated encodes of URL
                            var clickEvent = "return " + UrlUtils.PopUpUrl(HttpUtility.UrlDecode(this.loginLink.NavigateUrl), this, this.PortalSettings, true, false, 300, 650);
                            this.loginLink.Attributes.Add("onclick", clickEvent);
                            this.enhancedLoginLink.Attributes.Add("onclick", clickEvent);
                        }
                    }
                }
                catch (Exception exc)
                {
                    Exceptions.ProcessModuleLoadException(this, exc);
                }
            }
        }
    }
}
