using DotNetNuke.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetNuke.Web.Mvc
{
    public class Startup : IServiceRegistration
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<MvcModuleControlFactory>();
        }
    }
}
