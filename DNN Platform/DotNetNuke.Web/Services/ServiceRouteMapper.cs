using DotNetNuke.Web.Api;

namespace DotNetNuke.Web.Services
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute(
                "web", "default", "{controller}/{action}", new[] { GetType().Namespace });
        }
    }
}
