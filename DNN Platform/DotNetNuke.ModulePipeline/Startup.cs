using DotNetNuke.DependencyInjection;
using DotNetNuke.UI.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetNuke.ModulePipeline
{
    public class Startup : IDnnStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // MULTI-TARGETTING PIPELINE
            // -------------------------
            // This file multi-targets .NET Framework and .NET Standard,
            // which is needed as DNN migrates to .NET Core. The 'NET472'
            // pre-processor directives are to fully support Legacy DNN.
            // As the Pipeline is upgraded to be more complaint with 
            // .NET Standard 2.0 use the apprioprate pre-processor directives.
#if NET472
            services.AddSingleton<IModuleControlPipeline, ModuleControlPipeline>();
#endif
        }
    }
}
