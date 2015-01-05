#region Copyright
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
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DotNetNuke.ComponentModel;
using DotNetNuke.Framework.Reflections;
using DotNetNuke.Web.Mvc.Framework;
using DotNetNuke.Web.Mvc.Framework.Modules;

namespace DotNetNuke.Web.Mvc
{
    public class MvcHttpModule : IHttpModule
    {
        private static readonly object LockThis = new object();
        private static bool _isInitialized;
        private static RouteCollection _routes;

        public static RouteCollection Routes
        {
            get { return _routes ?? (_routes = RouteTable.Routes); }

            // We really don't want people playing with this outside of the test
            internal set { _routes = value; }
        }

        public void Init(HttpApplication context)
        {
            ComponentFactory.RegisterComponentInstance<IModuleExecutionEngine>(new ModuleExecutionEngine());

            RegisterModules();

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new ModuleDelegatingViewEngine());
            ViewEngines.Engines.Add(new RazorViewEngine());
        }

        public static void RegisterModules()
        {
            var moduleApplications = GetModules().ToDictionary(module => module.ModuleName);

            ComponentFactory.RegisterComponentInstance<IDictionary<string, ModuleApplication>>(moduleApplications);
        }

        private static IEnumerable<ModuleApplication> GetModules()
        {
            var typeLocator = new TypeLocator();
            IEnumerable<Type> types = typeLocator.GetAllMatchingTypes(t => t != null
                                                                                && t.IsClass
                                                                                && !t.IsAbstract
                                                                                && t.IsVisible
                                                                                && typeof(ModuleApplication).IsAssignableFrom(t));

            foreach (var moduleType in types)
            {
                ModuleApplication module;
                try
                {
                    module = Activator.CreateInstance(moduleType) as ModuleApplication;
                }
                catch (Exception)
                {
                    //Logger.ErrorFormat("Unable to create {0} while registering module injection filters.  {1}", filterType.FullName, e.Message);

                    module = null;
                }

                if (module != null)
                {
                    yield return module;
                }
            }
        }

        public void Dispose()
        {
        }
    }
}
