// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.GoogleMailAuthProvider
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web.UI;

    using Dnn.GoogleMailAuthProvider.Components;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Mail.OAuth;
    using Google.Apis.Auth.OAuth2.Web;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Google OAuth callback.
    /// </summary>
    public partial class Authorize : Page
    {
        private readonly ISmtpOAuthController smtpOAuthController;
        private readonly IHostSettingsService hostSettingsService;

        /// <summary>Initializes a new instance of the <see cref="Authorize"/> class.</summary>
        protected Authorize()
            : this(null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Authorize"/> class.</summary>
        /// <param name="smtpOAuthController">The SMTP OAuth controller.</param>
        /// <param name="hostSettingsService">The host settings service.</param>
        protected Authorize(ISmtpOAuthController smtpOAuthController, IHostSettingsService hostSettingsService)
        {
            if (smtpOAuthController != null)
            {
                this.smtpOAuthController = smtpOAuthController;
                this.hostSettingsService = hostSettingsService;
            }
            else
            {
                var serviceProvider = HttpContextSource.Current.GetScope().ServiceProvider;
                this.smtpOAuthController = serviceProvider.GetRequiredService<ISmtpOAuthController>();
                this.hostSettingsService = hostSettingsService ?? serviceProvider.GetRequiredService<IHostSettingsService>();
            }
        }

        /// <summary>
        /// OnLoad event.
        /// </summary>
        /// <param name="e">Event.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var portalId = Null.NullInteger;
            if (this.Request.QueryString["state"] != null)
            {
                var portalIdStr = RegexUtils.GetCachedRegex("portal_([0-9\\-]+?)_", RegexOptions.IgnoreCase).Match(this.Request.QueryString["state"])?.Groups[1].Value;
                if (!string.IsNullOrWhiteSpace(portalIdStr))
                {
                    portalId = Convert.ToInt32(portalIdStr);
                }
            }

            var authProvider = this.smtpOAuthController.GetOAuthProvider(Constants.Name);

            var settings = authProvider.GetSettings(portalId);
            var accountEmail = settings.FirstOrDefault(i => i.Name == Constants.AccountEmailSettingName)?.Value ?? string.Empty;

            var codeFlow = GoogleMailOAuthProvider.CreateAuthorizationCodeFlow(this.smtpOAuthController, this.hostSettingsService, portalId);
            if (codeFlow == null || authProvider.IsAuthorized(portalId))
            {
                this.CloseWindow();
                return;
            }

            var uri = this.Request.Url.ToString();
            if (uri.Contains("?"))
            {
                uri = uri.Substring(0, uri.IndexOf("?"));
            }

            var code = this.Request["code"];
            if (code != null)
            {
                var token = codeFlow.ExchangeCodeForTokenAsync(accountEmail, code, uri, CancellationToken.None).Result;
            }
            else
            {
                var result = new AuthorizationCodeWebApp(codeFlow, uri, this.Request.QueryString["state"]).AuthorizeAsync(accountEmail, CancellationToken.None).Result;
                if (result.RedirectUri != null)
                {
                    // Redirect the user to the authorization server.
                    this.Response.Redirect(result.RedirectUri, true);
                }
            }

            this.CloseWindow();
        }

        private void CloseWindow()
        {
            this.Response.Write("<script type='text/javascript'>window.close();</script>");
        }
    }
}
