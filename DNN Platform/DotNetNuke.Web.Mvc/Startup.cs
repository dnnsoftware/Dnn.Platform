using DotNetNuke.DependencyInjection;
using DotNetNuke.Web.Mvc.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Web.Mvc;

namespace DotNetNuke.Web.Mvc
{
    public class Startup : IDnnStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSingleton<IControllerFactory, DnnMvcControllerFactory>();
            services.AddSingleton<MvcModuleControlFactory>();
        }
    }
}
