using DotNetNuke.Web.Api;

namespace Dnn.AzureConnector.Services
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute routeManager)
        {
            routeManager.MapHttpRoute("AzureConnector",
                                      "default",
                                      "{controller}/{action}",
                                      new[] { "Dnn.AzureConnector.Services" });
        }
    }
}
