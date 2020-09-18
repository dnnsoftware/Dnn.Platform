// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Urls
{
    using System;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Portals;

    /// <summary>
    /// Abstract class to allow derived classes of different implementations of Url Rewriter.
    /// </summary>
    public abstract class UrlRewriterBase
    {
        internal abstract void RewriteUrl(object sender, EventArgs e);

        protected static void AutoAddAlias(HttpContext context)
        {
            var portalId = Host.Host.HostPortalID;

            // the domain name was not found so try using the host portal's first alias
            if (portalId > Null.NullInteger)
            {
                var portalAliasInfo = new PortalAliasInfo { PortalID = portalId, HTTPAlias = Globals.GetDomainName(context.Request, true) };
                PortalAliasController.Instance.AddPortalAlias(portalAliasInfo);

                context.Response.Redirect(context.Request.Url.ToString(), true);
            }
        }

        protected static bool CanAutoAddPortalAlias()
        {
            bool autoAddPortalAlias = HostController.Instance.GetBoolean("AutoAddPortalAlias");
            autoAddPortalAlias = autoAddPortalAlias && (PortalController.Instance.GetPortals().Count == 1);
            return autoAddPortalAlias;
        }
    }
}
