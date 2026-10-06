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
    using Microsoft.Extensions.Logging;

    /// <summary>Initializes the Dependency Injection container.</summary>
    public static class DependencyInjectionInitialize
    {
        private static readonly ILogger Logger = DnnLoggingController.GetLogger(typeof(DependencyInjectionInitialize));

        /// <summary>Gets the service collection (for logging/diagnostics).</summary>
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
            var allTypes = TypeExtensions.SafeGetTypes();
            allTypes.LogOtherExceptions(Logger);
            if (allTypes.LoadExceptions.Any())
            {
                var messageBuilder = allTypes.LoadExceptions.BuildLoaderExceptionsMessage();
                Logger.DependencyInjectionInitializeAssembliesCouldNotBeLoaded(messageBuilder.ToString());
            }

            var startupTypes = allTypes.Types
                .Where(
                    type => typeof(IDnnStartup).IsAssignableFrom(type) &&
                            type is { IsClass: true, IsAbstract: false });

            var startupInstances = startupTypes.Select(CreateInstance).Where(x => x != null);
            foreach (var startup in startupInstances)
            {
                try
                {
                    startup.ConfigureServices(services);
                }
                catch (Exception ex)
                {
                    Logger.DependencyInjectionInitializeUnableToConfigureServicesFor(ex, startup.GetType().FullName);
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
                Logger.DependencyInjectionInitializeUnableToInstantiateStartupCodeFor(ex, startupType.FullName);
                return null;
            }
        }
    }
}
