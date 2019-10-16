using DotNetNuke.Common;
using DotNetNuke.Common.Interfaces;
using DotNetNuke.DependencyInjection;
using DotNetNuke.UI.Modules;
using DotNetNuke.UI.Modules.Html5;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetNuke
{
    public class Startup : IDnnStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<WebFormsModuleControlFactory>();
            services.AddSingleton<Html5ModuleControlFactory>();
            services.AddSingleton<ReflectedModuleControlFactory>();
            services.AddTransient<INavigationManager, NavigationManager>();
        }
    }
}
