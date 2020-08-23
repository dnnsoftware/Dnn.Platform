// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web
{
    using System;
    using System.Linq;

    using DotNetNuke.DependencyInjection;
    using DotNetNuke.DependencyInjection.Extensions;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.DependencyInjection;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Initializes the Dependency Injection container.</summary>
    public static class DependencyInjectionInitialize
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DependencyInjectionInitialize));

        internal static IServiceCollection ServiceCollection { get; private set; }

        /// <summary>Builds the service provider.</summary>
        /// <returns>An <see cref="IServiceProvider"/> instance.</returns>
        public static IServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IScopeAccessor, ScopeAccessor>();
            ConfigureAllStartupServices(services);

            ServiceCollection = services;
            return services.BuildServiceProvider();
        }

        private static void ConfigureAllStartupServices(IServiceCollection services)
        {
            var startupTypes = AppDomain.CurrentDomain.GetAssemblies()
                .OrderBy(
                    assembly => assembly.FullName.StartsWith("DotNetNuke", StringComparison.OrdinalIgnoreCase) ? 0 :
                         assembly.FullName.StartsWith("DNN", StringComparison.OrdinalIgnoreCase) ? 1 : 2)
                .ThenBy(assembly => assembly.FullName)
                .SelectMany(assembly => assembly.SafeGetTypes().OrderBy(type => type.FullName ?? type.Name))
                .Where(type => typeof(IDnnStartup).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract);

            var startupInstances = startupTypes.Select(CreateInstance).Where(x => x != null);
            foreach (var startup in startupInstances)
            {
                try
                {
                    startup.ConfigureServices(services);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Unable to configure services for {startup.GetType().FullName}, see exception for details", ex);
                }
            }
        }

        private static IDnnStartup CreateInstance(Type startupType)
        {
            try
            {
                return (IDnnStartup)Activator.CreateInstance(startupType);
            }
            catch (Exception ex)
            {
                Logger.Error($"Unable to instantiate startup code for {startupType.FullName}", ex);
                return null;
            }
        }
    }
}
