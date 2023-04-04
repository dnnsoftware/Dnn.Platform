using DotNetNuke.Web.Api;

namespace DotNetNuke.BulkInstall.Components.WebAPI
{
    public class Routes : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("PolyDeploy", "default", "{controller}/{action}", new[] { "Cantarus.Modules.PolyDeploy.Components.WebAPI" });
        }
    }
}
