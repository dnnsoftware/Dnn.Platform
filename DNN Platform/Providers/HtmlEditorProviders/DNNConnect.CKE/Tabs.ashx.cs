// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNNConnect.CKEditorProvider
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using Microsoft.JScript;

    using Globals = DotNetNuke.Common.Globals;

    /// <summary>Renders the Tab Java Script.</summary>
    public class Tabs : PortalModuleBase, IHttpHandler
    {
        /// <summary>Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler"/> instance.</summary>
        /// <returns>true if the <see cref="T:System.Web.IHttpHandler"/> instance is reusable; otherwise, false.</returns>
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        /// <summary>Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler"/> interface.</summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext"/> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        public void ProcessRequest(HttpContext context)
        {
            var portalId = this.PortalSettings.PortalId;

            // Generate Pages Array
            var pagesArray = new StringBuilder();

            pagesArray.Append("var dnnpagesSelectBox = new Array(");

            var domainName = $"http://{Globals.GetDomainName(context.Request, true)}";

            foreach (TabInfo tab in TabController.GetPortalTabs(
                    portalId, -1, false, null, true, false, true, true, true))
            {
                var tabUrl = PortalController.GetPortalSettingAsBoolean("ContentLocalizationEnabled", portalId, false)
                                && !string.IsNullOrEmpty(tab.CultureCode)
                                    ? Globals.FriendlyUrl(
                                        tab,
                                        $"{Globals.ApplicationURL(tab.TabID)}&language={tab.CultureCode}")
                                    : Globals.FriendlyUrl(tab, Globals.ApplicationURL(tab.TabID));

                tabUrl = Globals.ResolveUrl(Regex.Replace(tabUrl, domainName, "~", RegexOptions.IgnoreCase));

                var tabName = GlobalObject.escape(tab.TabName);

                if (tab.Level.Equals(0))
                {
                    pagesArray.AppendFormat("new Array('| {0}','{1}'),", HttpUtility.JavaScriptStringEncode(tabName), HttpUtility.JavaScriptStringEncode(tabUrl));
                }
                else
                {
                    var separator = new StringBuilder();

                    for (int index = 0; index < tab.Level; index++)
                    {
                        separator.Append("--");
                    }

                    pagesArray.AppendFormat(
                        "new Array('|{0} {1}','{2}'),",
                        HttpUtility.JavaScriptStringEncode(separator.ToString()),
                        HttpUtility.JavaScriptStringEncode(tabName),
                        HttpUtility.JavaScriptStringEncode(tabUrl));
                }
            }

            if (pagesArray.ToString().EndsWith(",", StringComparison.Ordinal))
            {
                pagesArray.Remove(pagesArray.Length - 1, 1);
            }

            pagesArray.Append(");");

            context.Response.ContentType = "text/javascript";
            context.Response.Write(pagesArray.ToString());
        }
    }
}
