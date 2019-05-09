using DotNetNuke.DependencyInjection;
using DotNetNuke.UI.Modules;
using DotNetNuke.UI.Modules.Html5;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace DotNetNuke
{
    public class Startup : IServiceRegistration
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IWebFormsModuleControlFactory, WebFormsModuleControlFactory>();
            services.AddSingleton<IHtml5ModuleControlFactory, Html5ModuleControlFactory>();
            services.AddSingleton<IReflectedModuleControlFactory, ReflectedModuleControlFactory>();
        }

        public IEnumerable<KeyValuePair<Type, Type>> GetServiceRegistrations()
        {
            List<KeyValuePair<Type, Type>> registrations = new List<KeyValuePair<Type, Type>>();
            registrations.Add(new KeyValuePair<Type, Type>(typeof(IWebFormsModuleControlFactory), typeof(WebFormsModuleControlFactory)));
            registrations.Add(new KeyValuePair<Type, Type>(typeof(IHtml5ModuleControlFactory), typeof(Html5ModuleControlFactory)));
            registrations.Add(new KeyValuePair<Type, Type>(typeof(IReflectedModuleControlFactory), typeof(ReflectedModuleControlFactory)));

            return registrations;
        }
    }
}
