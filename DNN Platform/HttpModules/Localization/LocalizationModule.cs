// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.Localization;

using System;
using System.Web;

using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Localization;

/// <summary>Sets up localization for all requests.</summary>
public class LocalizationModule : IHttpModule
{
    /// <inheritdoc />
    public void Init(HttpApplication context)
    {
        context.PreRequestHandlerExecute += Context_PreRequestHandlerExecute;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // intentionally left empty
    }

    /// <summary>
    /// The PreRequestHandlerExecute event happens after AuthenticateRequest. This means this code will
    /// run after we know the identity of the user. This allows for the accurate retrieval of the locale
    /// which depends not just on the site and page locale, but also on the user's preference.
    /// </summary>
    private static void Context_PreRequestHandlerExecute(object sender, EventArgs e)
    {
        // We need to ensure that we don't run this code when not requesting a page as otherwise we may not have
        // an accurate portal identified (e.g. resources)
        var isPage = HttpContext.Current.CurrentHandler is Framework.PageBase;
        if (!isPage)
        {
            return;
        }

        var isInstallPage = HttpContext.Current.Request.Url.LocalPath.ToLowerInvariant().Contains("installwizard.aspx");
        if (isInstallPage)
        {
            return;
        }

        // The portalSettings should always be correct at this point, but in case we can't find a portal
        // we need to insure we're not setting the thread locale.
        var portalSettings = PortalController.Instance.GetCurrentSettings();
        if (portalSettings != null)
        {
            Localization.SetThreadCultures(Localization.GetPageLocale(portalSettings), portalSettings);
        }
    }
}
