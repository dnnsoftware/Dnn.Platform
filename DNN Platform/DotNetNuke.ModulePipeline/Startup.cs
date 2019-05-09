using DotNetNuke.DependencyInjection;
using DotNetNuke.UI.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetNuke.ModulePipeline
{
    public class Startup : IServiceRegistration
    {
        public void ConfigureServices(IServiceCollection services)
        {
#if NET472
            services.AddSingleton<IModuleControlPipeline, ModuleControlPipeline>();
#endif
        }
    }
}
