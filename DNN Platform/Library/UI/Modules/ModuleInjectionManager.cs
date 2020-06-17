// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Collections.Internal;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework.Reflections;
    using DotNetNuke.Instrumentation;

    internal class ModuleInjectionManager
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ModuleInjectionManager));
        private static NaiveLockingList<IModuleInjectionFilter> _filters;

        public static void RegisterInjectionFilters()
        {
            _filters = new NaiveLockingList<IModuleInjectionFilter>();

            foreach (IModuleInjectionFilter filter in GetFilters())
            {
                _filters.Add(filter);
            }
        }

        public static bool CanInjectModule(ModuleInfo module, PortalSettings portalSettings)
        {
            return _filters.All(filter => filter.CanInjectModule(module, portalSettings));
        }

        internal static bool IsValidModuleInjectionFilter(Type t)
        {
            return t != null && t.IsClass && !t.IsAbstract && t.IsVisible && typeof(IModuleInjectionFilter).IsAssignableFrom(t);
        }

        private static IEnumerable<IModuleInjectionFilter> GetFilters()
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
    }
}
