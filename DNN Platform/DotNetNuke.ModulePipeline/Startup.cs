using DotNetNuke.DependencyInjection;
using DotNetNuke.UI.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetNuke.ModulePipeline
{
    public class Startup : IDnnStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
#if NET472
            services.AddSingleton<IModuleControlPipeline, ModuleControlPipeline>();
#endif
        }
    }
}
