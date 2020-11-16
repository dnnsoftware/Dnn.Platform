// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Web.Http;

    using DotNetNuke.Common;

    public static class HttpConfigurationExtensions
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

        public static IEnumerable<ITabAndModuleInfoProvider> GetTabAndModuleInfoProviders(this HttpConfiguration configuration)
        {
            Requires.NotNull("configuration", configuration);

            var providers = configuration.Properties.GetOrAdd(Key, InitValue) as ConcurrentQueue<ITabAndModuleInfoProvider>;

            if (providers == null)
            {
                // shouldn't ever happen outside of unit tests
                return new ITabAndModuleInfoProvider[] { };
            }

            return providers.ToArray();
        }

        private static object InitValue(object o)
        {
            return new ConcurrentQueue<ITabAndModuleInfoProvider>();
        }
    }
}
