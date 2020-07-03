// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using DotNetNuke.DependencyInjection;
    using DotNetNuke.DependencyInjection.Extensions;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.DependencyInjection;
    using DotNetNuke.Web.Extensions;
    using Microsoft.Extensions.DependencyInjection;

    public class Startup : IDnnStartup
    {
        private static readonly ILog _logger = LoggerSource.Instance.GetLogger(typeof(Startup));

        public Startup()
        {
            this.Configure();
        }

        public IServiceProvider DependencyProvider { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
            var startupTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => x != Assembly.GetAssembly(typeof(Startup)))
                .SelectMany(x => x.SafeGetTypes())
                .Where(x => typeof(IDnnStartup).IsAssignableFrom(x) &&
                            x.IsClass &&
                            !x.IsAbstract);

            var startupInstances = startupTypes
                .Select(x => this.CreateInstance(x))
                .Where(x => x != null);

            foreach (IDnnStartup startup in startupInstances)
            {
                try
                {
                    startup.ConfigureServices(services);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Unable to configure services for {typeof(Startup).FullName}, see exception for details", ex);
                }
            }

            services.AddWebApi();
        }

        private void Configure()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IScopeAccessor, ScopeAccessor>();
            this.ConfigureServices(services);
            this.DependencyProvider = services.BuildServiceProvider();
        }

        private object CreateInstance(Type startupType)
        {
            IDnnStartup startup = null;
            try
            {
                startup = (IDnnStartup)Activator.CreateInstance(startupType);
            }
            catch (Exception ex)
            {
                _logger.Error($"Unable to instantiate startup code for {startupType.FullName}", ex);
            }

            return startup;
        }
    }
}
