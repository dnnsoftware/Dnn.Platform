using DotNetNuke.DependencyInjection;
using DotNetNuke.Instrumentation;
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
            ConfigureServices(services);
            DependencyProvider = services.BuildServiceProvider();
        }

        public IServiceProvider DependencyProvider { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
            var startupTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => x != Assembly.GetAssembly(typeof(Startup)))
                .SelectMany(x => x.GetTypes())
                .Where(x => typeof(IDnnStartup).IsAssignableFrom(x) &&
                            x.IsClass &&
                            !x.IsAbstract);

            var startupInstances = startupTypes
                .Select(x => (IDnnStartup)Activator.CreateInstance(x));

            foreach (IDnnStartup startup in startupInstances)
            {
                startup.ConfigureServices(services);
            }
        }
    }
}
