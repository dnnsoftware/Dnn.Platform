using DotNetNuke.Web.Api;

namespace Cantarus.Modules.PolyDeploy.WebAPI
{
    public class Routes : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("PolyDeploy", "default", "{controller}/{action}", new[] { "Cantarus.Modules.PolyDeploy.WebAPI" });
        }
    }
}
