using DotNetNuke.Common;
using DotNetNuke.Abstractions;
using DotNetNuke.DependencyInjection;
using DotNetNuke.Entities.Portals;
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
            services.AddTransient(x => PortalController.Instance);
            services.AddTransient<INavigationManager, NavigationManager>();
        }
    }
}
