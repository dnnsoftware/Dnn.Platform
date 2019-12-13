using DotNetNuke.Web.Api;

namespace Dnn.ExportImport.Services
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute routeManager)
        {
            routeManager.MapHttpRoute("SiteExportImport",
                                      "default",
                                      "{controller}/{action}",
                                      new[] { typeof(ServiceRouteMapper).Namespace });
        }
    }
}
