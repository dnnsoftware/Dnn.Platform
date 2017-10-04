#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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

using System;
using System.Collections.Generic;
using System.Linq;

using DotNetNuke.Collections.Internal;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework.Reflections;
using DotNetNuke.Instrumentation;

namespace DotNetNuke.UI.Modules
{
    internal class ModuleInjectionManager
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (ModuleInjectionManager));
        private static NaiveLockingList<IModuleInjectionFilter> _filters;

        public static void RegisterInjectionFilters()
        {
            _filters = new NaiveLockingList<IModuleInjectionFilter>();

            foreach (IModuleInjectionFilter filter in GetFilters())
            {
                _filters.Add(filter);
            }
        }

        private static IEnumerable<IModuleInjectionFilter>  GetFilters()
        {
            var typeLocator = new TypeLocator();
            IEnumerable<Type> types = typeLocator.GetAllMatchingTypes(IsValidModuleInjectionFilter);

            foreach (Type filterType in types)
            {
                IModuleInjectionFilter filter;
                try
                {
                    filter = Activator.CreateInstance(filterType) as IModuleInjectionFilter;
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat("Unable to create {0} while registering module injection filters.  {1}", filterType.FullName,
                                 e.Message);
                    filter = null;
                }

                if (filter != null)
                {
                    yield return filter;
                }
            }
        }

        internal static bool IsValidModuleInjectionFilter(Type t)
        {
            return t != null && t.IsClass && !t.IsAbstract && t.IsVisible && typeof(IModuleInjectionFilter).IsAssignableFrom(t);
        }

        public static bool CanInjectModule(ModuleInfo module, PortalSettings portalSettings)
        {
            return _filters.All(filter => filter.CanInjectModule(module, portalSettings));
        }
    }
}