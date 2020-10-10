// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web
{
    using System;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.DependencyInjection.Extensions;
    using DotNetNuke.Instrumentation;

    using Microsoft.AspNetCore.Builder.Internal;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Initializes the Startup Configure API.
    /// </summary>
    internal static class StartupConfigureInitialize
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DependencyInjectionInitialize));

        /// <summary>
        /// Instantiates all implementations of <see cref="IDnnStartupConfigure"/>
        /// and invokes the Configure method.
        /// </summary>
        /// <param name="container">The Dependency Injection container.</param>
        public static void ConfigureApplication(IServiceProvider container)
        {
            var applicationBuilder = new ApplicationBuilder(container);
            var applicationInfo = container.GetRequiredService<IApplicationInfo>();

            var startupInstances = AppDomain.CurrentDomain.GetAssemblies()
                .OrderBy(assembly =>
                    assembly.FullName.StartsWith("DotNetNuke", StringComparison.OrdinalIgnoreCase) ?
                    0 :
                    assembly.FullName.StartsWith("DNN", StringComparison.OrdinalIgnoreCase) ? 1 : 2)
                .ThenBy(assembly => assembly.FullName)
                .SelectMany(assembly => assembly
                    .SafeGetTypes()
                    .OrderBy(type => type.FullName ?? type.Name))
                .Where(type => typeof(IDnnStartupConfigure).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract)
                .Select(CreateInstance)
                .Where(instance => instance != null);

            foreach (var startup in startupInstances)
            {
                try
                {
                    startup.Configure(applicationBuilder, applicationInfo);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Unable to configure services for {startup.GetType().FullName}, see exception for details", ex);
                }
            }

            IDnnStartupConfigure CreateInstance(Type startupType)
            {
                try
                {
                    return (IDnnStartupConfigure)Activator.CreateInstance(startupType);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Unable to instantiate startup code for {startupType.FullName}", ex);
                    return null;
                }
            }
        }
    }
}
