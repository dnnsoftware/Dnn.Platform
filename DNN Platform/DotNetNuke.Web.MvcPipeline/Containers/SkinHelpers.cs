using DotNetNuke.Abstractions.ClientResources;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetNuke.Web.MvcPipeline.Containers
{
    public static partial class SkinHelpers
    {
        private static IClientResourceController GetClientResourcesController()
        {
            var serviceProvider = Common.Globals.GetCurrentServiceProvider();
            return serviceProvider.GetRequiredService<IClientResourceController>();
        }
    }
}
