﻿using DotNetNuke.Common;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Web.Http;

namespace DotNetNuke.Web.Mvc
{
    internal static class HttpConfigurationExtensions
    {
        private const string Key = "MvcTabAndModuleInfoProvider";

        public static void AddTabAndModuleInfoProvider(this HttpConfiguration configuration, ITabAndModuleInfoProvider tabAndModuleInfoProvider)
        {
            Requires.NotNull("configuration", configuration);
            Requires.NotNull("tabAndModuleInfoProvider", tabAndModuleInfoProvider);

            var providers = configuration.Properties.GetOrAdd(Key, InitValue) as ConcurrentQueue<ITabAndModuleInfoProvider>;

            if (providers == null)
            {
                providers = new ConcurrentQueue<ITabAndModuleInfoProvider>();
                configuration.Properties[Key] = providers;
            }

            providers.Enqueue(tabAndModuleInfoProvider);
        }

        private static object InitValue(object o)
        {
            return new ConcurrentQueue<ITabAndModuleInfoProvider>();
        }

        public static IEnumerable<ITabAndModuleInfoProvider> GetTabAndModuleInfoProviders(this HttpConfiguration configuration)
        {
            Requires.NotNull("configuration", configuration);

            var providers = configuration.Properties.GetOrAdd(Key, InitValue) as ConcurrentQueue<ITabAndModuleInfoProvider>;

            if (providers == null)
            {
                //shouldn't ever happen outside of unit tests
                return new ITabAndModuleInfoProvider[] { };
            }

            return providers.ToArray();
        }
    }
}
