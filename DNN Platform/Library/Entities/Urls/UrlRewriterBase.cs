#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Web;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Portals;

#endregion

namespace DotNetNuke.Entities.Urls
{
    /// <summary>
    /// Abstract class to allow derived classes of different implementations of Url Rewriter
    /// </summary>
    public abstract class UrlRewriterBase
    {
        internal abstract void RewriteUrl(object sender, EventArgs e);

        protected static void AutoAddAlias(HttpContext context)
        {
            var portalId = Host.Host.HostPortalID;
            //the domain name was not found so try using the host portal's first alias
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