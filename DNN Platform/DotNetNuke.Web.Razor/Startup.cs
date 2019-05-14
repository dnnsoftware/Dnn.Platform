using DotNetNuke.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetNuke.Web.Razor
{
    public class Startup : IDnnStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<RazorModuleControlFactory>();
        }
    }
}
