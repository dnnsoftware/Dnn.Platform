// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExchangeOnlineAuthProvider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web.UI;

    using Dnn.ExchangeOnlineAuthProvider.Components;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Mail.OAuth;
    using Microsoft.Identity.Client;
    using Microsoft.Identity.Client.Extensibility;

    /// <summary>
    /// Google OAuth callback.
    /// </summary>
    public partial class Authorize : Page
    {
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

            var authProvider = SmtpOAuthController.Instance.GetOAuthProvider(Constants.Name);
            var clientApplication = ExchangeOnlineOAuthProvider.CreateClientApplication(portalId);

            if (clientApplication == null || authProvider.IsAuthorized(portalId))
            {
                this.CloseWindow();
                return;
            }

            var scopes = ExchangeOnlineOAuthProvider.GetAuthenticationScopes();

            var code = this.Request["code"];
            if (code != null)
            {
                var result = clientApplication.AcquireTokenByAuthorizationCode(scopes, code).ExecuteAsync().Result;
            }
            else
            {
                var queryStrings = new Dictionary<string, string>
                {
                    { "state", this.Request.QueryString["state"] },
                };
                Uri msUri = clientApplication.GetAuthorizationRequestUrl(scopes).WithExtraQueryParameters(queryStrings).ExecuteAsync().Result;
                this.Response.Redirect(msUri.ToString(), true);
            }

            this.CloseWindow();
        }

        private void CloseWindow()
        {
            this.Response.Write("<script type='text/javascript'>window.close();</script>");
        }
    }
}
