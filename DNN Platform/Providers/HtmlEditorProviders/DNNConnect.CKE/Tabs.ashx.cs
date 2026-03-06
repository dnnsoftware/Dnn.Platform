// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNNConnect.CKEditorProvider
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.JScript;

    using Globals = DotNetNuke.Common.Globals;

    /// <summary>Renders the Tab JavaScript.</summary>
    /// <param name="portalController">The portal controller.</param>
    /// <param name="hostSettings">The host settings.</param>
    /// <param name="appStatus">The application status.</param>
    public class Tabs(IPortalController portalController, IHostSettings hostSettings, IApplicationStatusInfo appStatus)
        : PortalModuleBase, IHttpHandler
    {
        private readonly IPortalController portalController = portalController ?? HttpContextSource.Current.GetScope().ServiceProvider.GetRequiredService<IPortalController>();
        private readonly IHostSettings hostSettings = hostSettings ?? HttpContextSource.Current.GetScope().ServiceProvider.GetRequiredService<IHostSettings>();
        private readonly IApplicationStatusInfo appStatus = appStatus ?? HttpContextSource.Current.GetScope().ServiceProvider.GetRequiredService<IApplicationStatusInfo>();

        /// <summary>Initializes a new instance of the <see cref="Tabs"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IPortalController. Scheduled removal in v12.0.0.")]
        public Tabs()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Tabs"/> class.</summary>
        /// <param name="portalController">The portal controller.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public Tabs(IPortalController portalController)
            : this(portalController, null, null)
        {
        }

        /// <summary>Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler"/> instance.</summary>
        /// <returns>true if the <see cref="T:System.Web.IHttpHandler"/> instance is reusable; otherwise, false.</returns>
        public bool IsReusable => false;

        /// <summary>Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler"/> interface.</summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext"/> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        public void ProcessRequest(HttpContext context)
        {
            var portalId = this.PortalSettings.PortalId;

            // Generate Pages Array
            var pagesArray = new StringBuilder();

            pagesArray.Append("var dnnpagesSelectBox = new Array(");

            var domainName = $"http://{Globals.GetDomainName(context.Request, true)}";

            foreach (TabInfo tab in TabController.GetPortalTabs(this.hostSettings, this.appStatus, portalId, -1, false, null, true, false, true, true, true))
            {
                var tabUrl = PortalController.GetPortalSettingAsBoolean(this.portalController, "ContentLocalizationEnabled", portalId, false)
                                && !string.IsNullOrEmpty(tab.CultureCode)
                                    ? Globals.FriendlyUrl(tab, $"{Globals.ApplicationURL(tab.TabID)}&language={tab.CultureCode}")
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
