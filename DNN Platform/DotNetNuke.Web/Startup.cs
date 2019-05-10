using DotNetNuke.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotNetNuke.Web
{
    public class Startup : IServiceRegistration
    {
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
                .SelectMany(x => GetTypes(x))
                .Where(x => typeof(IServiceRegistration).IsAssignableFrom(x) &&
                            x.IsClass &&
                            !x.IsAbstract);

            var startupInstances = startupTypes
                .Select(x => (IServiceRegistration)Activator.CreateInstance(x));

            foreach (IServiceRegistration startup in startupInstances)
            {
                startup.ConfigureServices(services);
            }
        }

        private IEnumerable<Type> GetTypes(Assembly x)
        {
            Type[] types = null;
            try
            {
                types = x.GetTypes();
            }
            catch (Exception ex)
            {
                // log this
                types = new Type[0];
            }

            return types;
        }
    }
}
