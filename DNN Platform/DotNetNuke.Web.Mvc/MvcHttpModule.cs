// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Helpers;
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Xml;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework.Reflections;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Web.Mvc.Framework;
    using DotNetNuke.Web.Mvc.Framework.Modules;

    using Microsoft.Extensions.DependencyInjection;

    public class MvcHttpModule : IHttpModule
    {
        public static readonly Regex MvcServicePath = new Regex(@"DesktopModules/MVC/", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        static MvcHttpModule()
        {
            var engines = ViewEngines.Engines;
            engines.Clear();
            engines.Add(
                new ModuleDelegatingViewEngine(
                    new HostSettings(
                        new HostController(
#pragma warning disable CS0618 // Type or member is obsolete
                            new EventLogController(),
#pragma warning restore CS0618 // Type or member is obsolete
                            new Lazy<IPortalController>(() => PortalController.Instance)))));
            engines.Add(new RazorViewEngine());
        }

        public void Init(HttpApplication context)
        {
            SuppressXFrameOptionsHeaderIfPresentInConfig(context.Context.GetScope().ServiceProvider.GetRequiredService<IApplicationStatusInfo>());
            ComponentFactory.RegisterComponentInstance<IModuleExecutionEngine>(new ModuleExecutionEngine());
            context.BeginRequest += InitDnn;
        }

        public void Dispose()
        {
        }

        private static void InitDnn(object sender, EventArgs e)
        {
            if (sender is HttpApplication app && MvcServicePath.IsMatch(app.Context.Request.RawUrl))
            {
                Initialize.Init(app);
            }
        }

        /// <summary>Suppress X-Frame-Options Header if there is configuration specified in web.config for it.</summary>
        private static void SuppressXFrameOptionsHeaderIfPresentInConfig(IApplicationStatusInfo appStatus)
        {
            var xmlConfig = Config.Load(appStatus);
            var xmlCustomHeaders =
                xmlConfig.SelectSingleNode("configuration/system.webServer/httpProtocol/customHeaders") ??
                xmlConfig.SelectSingleNode("configuration/location/system.webServer/httpProtocol/customHeaders");

            if (xmlCustomHeaders?.ChildNodes != null)
            {
                foreach (XmlNode header in xmlCustomHeaders.ChildNodes)
                {
                    if (header.Attributes != null && header.Attributes["name"].Value == "X-Frame-Options")
                    {
                        AntiForgeryConfig.SuppressXFrameOptionsHeader = true;
                        break;
                    }
                }
            }
        }
    }
}
