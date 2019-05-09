using DotNetNuke.DependencyInjection;
using DotNetNuke.Web.Mvc.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetNuke.Web.Mvc
{
    public class Startup : IServiceRegistration
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IMvcModuleControlFactory, MvcModuleControlFactory>();
        }
    }
}
