using DotNetNuke.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetNuke.Web.Razor
{
    class Startup : IServiceRegistration
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IRazorModuleControlFactory, RazorModuleControlFactory>();
        }
    }
}
