using DotNetNuke.Web.Api;

namespace Dnn.Modules.BulkInstall.Components.WebAPI
{
    public class Routes : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("BulkInstall", "default", "{controller}/{action}", new[] { this.GetType().Namespace });
        }
    }
}
