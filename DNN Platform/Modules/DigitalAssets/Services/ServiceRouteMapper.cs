using DotNetNuke.Web.Api;

namespace DotNetNuke.Modules.DigitalAssets.Services
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("DigitalAssets", "default", "{controller}/{action}", new[] { "DotNetNuke.Modules.DigitalAssets.Services" });
        }
    }
}
