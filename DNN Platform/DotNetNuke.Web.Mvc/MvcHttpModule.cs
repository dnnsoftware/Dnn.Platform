﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Framework.Reflections;
using DotNetNuke.Web.Mvc.Framework;
using DotNetNuke.Web.Mvc.Framework.Modules;

namespace DotNetNuke.Web.Mvc
{
    public class MvcHttpModule : IHttpModule
    {
        public static readonly Regex MvcServicePath = new Regex(@"DesktopModules/MVC/", RegexOptions.Compiled);

        static MvcHttpModule()
        {
            var engines = ViewEngines.Engines;
            engines.Clear();
            engines.Add(new ModuleDelegatingViewEngine());
            engines.Add(new RazorViewEngine());
        }

        public void Init(HttpApplication context)
        {
            SuppressXFrameOptionsHeaderIfPresentInConfig();
            ComponentFactory.RegisterComponentInstance<IModuleExecutionEngine>(new ModuleExecutionEngine());
            context.BeginRequest += InitDnn;
        }

        private static void InitDnn(object sender, EventArgs e)
        {
            var app = sender as HttpApplication;
            if (app != null && MvcServicePath.IsMatch(app.Context.Request.RawUrl.ToLowerInvariant()))
            {
                Initialize.Init(app);
            }
        }
        public void Dispose()
        {
        }

        /// <summary>
        /// Suppress X-Frame-Options Header if there is configuration specified in web.config for it.
        /// </summary>
        private static void SuppressXFrameOptionsHeaderIfPresentInConfig()
        {
            var xmlConfig = Config.Load();
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
