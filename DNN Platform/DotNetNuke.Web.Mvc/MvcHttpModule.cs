﻿#region Copyright
// 
// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2014
// by DNN Corporation
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DotNetNuke.Common;
using DotNetNuke.ComponentModel;
using DotNetNuke.Framework.Reflections;
using DotNetNuke.Web.Mvc.Framework;
using DotNetNuke.Web.Mvc.Framework.Modules;

namespace DotNetNuke.Web.Mvc
{
    public class MvcHttpModule : IHttpModule
    {
        public static readonly Regex MvcServicePath = new Regex(@"DesktopModules/MVC/", RegexOptions.Compiled);

        public void Init(HttpApplication context)
        {
            ComponentFactory.RegisterComponentInstance<IModuleExecutionEngine>(new ModuleExecutionEngine());

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new ModuleDelegatingViewEngine());
            ViewEngines.Engines.Add(new RazorViewEngine());

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
    }
}
