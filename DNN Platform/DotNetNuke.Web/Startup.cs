﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.DependencyInjection;
using DotNetNuke.DependencyInjection.Extensions;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.DependencyInjection;
using DotNetNuke.Web.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotNetNuke.Web
{
    public class Startup : IDnnStartup
    {
        private static readonly ILog _logger = LoggerSource.Instance.GetLogger(typeof(Startup));
        public Startup()
        {
            Configure();
        }

        private void Configure()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IScopeAccessor, ScopeAccessor>();
            ConfigureServices(services);
            DependencyProvider = services.BuildServiceProvider();
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
                .Select(x => CreateInstance(x))
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
