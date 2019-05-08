using DotNetNuke.Contracts;
using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Common;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
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
            //var path = System.Web.Hosting.HostingEnvironment.MapPath("~/bin");
            //var dll = $"{path}\\DotNetNuke.dll";
            //var assembly = Assembly.LoadFile(dll);
            //var startup = assembly.GetTypes()
            //var startup = AppDomain.CurrentDomain.GetAssemblies()
            //    .SelectMany(x => x.GetTypes())
            //    .Where(x => typeof(IServiceRegistration).IsAssignableFrom(x) && 
            //                x.IsClass && 
            //                !x.IsAbstract)
            //    .Select(x => (IServiceRegistration)Activator.CreateInstance(x))
            //    .FirstOrDefault();
            //var registrations = startup.GetServiceRegistrations();
            //foreach (var registration in registrations)
            //{
            //    services.AddSingleton(registration.Key, registration.Value);
            //}

            //var core = new DotNetNuke.Startup();
            //core.ConfigureServices(services);

            //var applicationEnvironment = PlatformServices.Default.Application;
            //services.AddSingleton(applicationEnvironment);
            //var appDirectory = Directory.GetCurrentDirectory();

            //var environment = new HostingEnvironment
            //{

            //    WebRootFileProvider = new PhysicalFileProvider(appDirectory),
            //    ApplicationName = "DotNetNuke"
            //};
            //services.AddSingleton<IHostingEnvironment>(environment);
            //services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();


            //var path = System.Web.Hosting.HostingEnvironment.MapPath("~/bin");

            //var currentAssemblies = AppDomain.CurrentDomain
            //    .GetAssemblies()
            //    .Select(x => x.FullName);

            //foreach (string dll in Directory.GetFiles(path, "*.dll"))
            //{
            //    if (dll == $"{path}\\DotNetNuke.Web.dll")
            //        continue;

            //    var assembly = Assembly.LoadFile(dll);
            //    if (currentAssemblies.Any(x => x == assembly.FullName)) continue;

            //    AppDomain.CurrentDomain.Load(assembly.FullName);
            //}

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

            //var razor = new DotNetNuke.ModulePipeline.Startup();
            //razor.ConfigureServices(services);
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
