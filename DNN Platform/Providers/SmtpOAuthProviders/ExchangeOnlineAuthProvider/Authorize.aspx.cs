// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExchangeOnlineAuthProvider;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI;

using Dnn.ExchangeOnlineAuthProvider.Components;
using DotNetNuke.Abstractions.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Extensions;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Mail.OAuth;
using Microsoft.Extensions.DependencyInjection;

/// <summary>Exchange OAuth callback.</summary>
public abstract class Authorize : Page
{
    private readonly IPortalController portalController;
    private readonly ISmtpOAuthController smtpOAuthController;
    private readonly IHostSettingsService hostSettingsService;
    private readonly IHostSettings hostSettings;

    /// <summary>Initializes a new instance of the <see cref="Authorize"/> class.</summary>
    [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
    protected Authorize()
        : this(null, null, null, null)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="Authorize"/> class.</summary>
    /// <param name="smtpOAuthController">The SMTP OAuth controller.</param>
    /// <param name="hostSettingsService">The host settings service.</param>
    [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
    protected Authorize(ISmtpOAuthController smtpOAuthController, IHostSettingsService hostSettingsService)
        : this(smtpOAuthController, hostSettingsService, null, null)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="Authorize"/> class.</summary>
    /// <param name="smtpOAuthController">The SMTP OAuth controller.</param>
    /// <param name="hostSettingsService">The host settings service.</param>
    /// <param name="hostSettings">The host settings.</param>
    /// <param name="portalController">The portal controller.</param>
    protected Authorize(ISmtpOAuthController smtpOAuthController, IHostSettingsService hostSettingsService, IHostSettings hostSettings, IPortalController portalController)
    {
        this.smtpOAuthController = smtpOAuthController ?? HttpContextSource.Current.GetScope().ServiceProvider.GetRequiredService<ISmtpOAuthController>();
        this.hostSettingsService = hostSettingsService ?? HttpContextSource.Current.GetScope().ServiceProvider.GetRequiredService<IHostSettingsService>();
        this.hostSettings = hostSettings ?? HttpContextSource.Current.GetScope().ServiceProvider.GetRequiredService<IHostSettings>();
        this.portalController = portalController ?? HttpContextSource.Current.GetScope().ServiceProvider.GetRequiredService<IPortalController>();
    }

    /// <summary>OnLoad event.</summary>
    /// <param name="e">Event.</param>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        this.RegisterAsyncTask(new PageAsyncTask(this.OnLoadAsync));
    }

    private async Task OnLoadAsync(CancellationToken cancellationToken)
    {
        var portalId = Null.NullInteger;
        if (this.Request.QueryString["state"] != null)
        {
            var portalIdStr = RegexUtils.GetCachedRegex("portal_([0-9\\-]+?)_", RegexOptions.IgnoreCase).Match(this.Request.QueryString["state"]).Groups[1].Value;
            if (!string.IsNullOrWhiteSpace(portalIdStr))
            {
                portalId = Convert.ToInt32(portalIdStr);
            }
        }

        var authProvider = this.smtpOAuthController.GetOAuthProvider(Constants.Name);
        var clientApplication = ExchangeOnlineOAuthProvider.CreateClientApplication(this.smtpOAuthController, this.hostSettingsService, this.hostSettings, this.portalController, portalId);

        if (clientApplication == null || await authProvider.IsAuthorizedAsync(portalId, cancellationToken))
        {
            this.CloseWindow();
            return;
        }

        var scopes = ExchangeOnlineOAuthProvider.GetAuthenticationScopes();

        var code = this.Request["code"];
        if (code != null)
        {
            _ = await clientApplication.AcquireTokenByAuthorizationCode(scopes, code).ExecuteAsync(cancellationToken);
        }
        else
        {
            var queryStrings = new Dictionary<string, string>
            {
                { "state", this.Request.QueryString["state"] },
            };
            var msUri = await clientApplication.GetAuthorizationRequestUrl(scopes)
                .WithExtraQueryParameters(queryStrings)
                .ExecuteAsync(cancellationToken);
            this.Response.Redirect(msUri.ToString(), true);
        }

        this.CloseWindow();
    }

    private void CloseWindow()
    {
        this.Response.Write("<script type='text/javascript'>window.close();</script>");
    }
}
