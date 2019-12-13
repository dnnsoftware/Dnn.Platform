using DotNetNuke.Web.Api;

namespace DotNetNuke.Web.InternalServices
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("InternalServices", 
                                            "default", 
                                            "{controller}/{action}", 
                                            new[] { "DotNetNuke.Web.InternalServices" });
        }
    }
}
