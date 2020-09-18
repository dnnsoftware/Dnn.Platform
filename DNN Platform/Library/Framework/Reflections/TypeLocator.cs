// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable ConvertPropertyToExpressionBody
namespace DotNetNuke.Framework.Reflections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using DotNetNuke.Framework.Internal.Reflection;

    public class TypeLocator : ITypeLocator, IAssemblyLocator
    {
        private IAssemblyLocator _assemblyLocator;

        internal IAssemblyLocator AssemblyLocator
        {
            get { return this._assemblyLocator ?? (this._assemblyLocator = this); }
            set { this._assemblyLocator = value; }
        }

        IEnumerable<IAssembly> IAssemblyLocator.Assemblies
        {
            // this method is not readily testable as the assemblies in the current app domain
            // will vary depending on the test runner and test configuration
            get
            {
                return from assembly in AppDomain.CurrentDomain.GetAssemblies()
                       where this.CanScan(assembly)
                       select new AssemblyWrapper(assembly);
            }
        }

        public IEnumerable<Type> GetAllMatchingTypes(Predicate<Type> predicate)
        {
            foreach (var assembly in this.AssemblyLocator.Assemblies)
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    // some assemblies don't want to be reflected but they still
                    // expose types in the exception
                    types = ex.Types ?? new Type[0];
                }

                foreach (var type in types)
                {
                    if (type != null)
                    {
                        if (predicate(type))
                        {
                            yield return type;
                        }
                    }
                }
            }
        }

        private bool CanScan(Assembly assembly)
        {
            string[] ignoreAssemblies = new string[]
                                                {
                                                    "DotNetNuke.Authentication.Facebook",
                                                    "DotNetNuke.Authentication.Google",
                                                    "DotNetNuke.Authentication.LiveConnect",
                                                    "DotNetNuke.Authentication.Twitter",
                                                    "DotNetNuke.ASP2MenuNavigationProvider",
                                                    "DotNetNuke.DNNDropDownNavigationProvider",
                                                    "DotNetNuke.DNNMenuNavigationProvider",
                                                    "DotNetNuke.DNNTreeNavigationProvider",
                                                    "DotNetNuke.SolpartMenuNavigationProvider",
                                                    "DotNetNuke.HttpModules",
                                                    "DotNetNuke.Instrumentation",
                                                    "DotNetNuke.Log4Net",
                                                    "DotNetNuke.Modules.Groups",
                                                    "DotNetNuke.Modules.Html",
                                                    "DotNetNuke.Modules.HtmlEditorManager",
                                                    "DotNetNuke.Modules.MobileManagement",
                                                    "DotNetNuke.Modules.PreviewProfileManagement",
                                                    "DotNetNuke.Modules.RazorHost",
                                                    "DotNetNuke.Modules.Taxonomy",
                                                    "DotNetNuke.Modules.UrlManagement",
                                                    "DotNetNuke.RadEditorProvider",
                                                    "DotNetNuke.Services.Syndication",
                                                    "DotNetNuke.Web.Client",
                                                    "DotNetNuke.Web.DDRMenu",
                                                    "DotNetNuke.Web.Razor",
                                                    "DotNetNuke.Web.Mvc",
                                                    "DotNetNuke.WebControls",
                                                    "DotNetNuke.WebUtility",
                                                };

            // First eliminate by "class"
            var assemblyName = assembly.FullName.ToLowerInvariant();
            bool canScan = !(assemblyName.StartsWith("clientdependency.core") || assemblyName.StartsWith("countrylistbox")
                || assemblyName.StartsWith("icsharpcode") || assemblyName.StartsWith("fiftyone")
                || assemblyName.StartsWith("lucene") || assemblyName.StartsWith("microsoft")
                || assemblyName.StartsWith("newtonsoft") || assemblyName.StartsWith("petapoco")
                || assemblyName.StartsWith("sharpziplib") || assemblyName.StartsWith("system")
                || assemblyName.StartsWith("telerik") || assemblyName.StartsWith("webformsmvp")
                || assemblyName.StartsWith("webmatrix") || assemblyName.StartsWith("solpart"));

            if (canScan)
            {
                // Next eliminate specific assemblies
                if (ignoreAssemblies.Any(ignoreAssembly => assemblyName == ignoreAssembly.ToLowerInvariant()))
                {
                    canScan = false;
                }
            }

            return canScan;
        }
    }
}
