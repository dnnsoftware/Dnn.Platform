﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
