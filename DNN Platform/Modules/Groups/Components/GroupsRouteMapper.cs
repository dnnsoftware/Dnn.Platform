using System;
using DotNetNuke.Web.Api;

namespace DotNetNuke.Modules.Groups 
{
    public class ServiceRouteMapper : IServiceRouteMapper 
    {
        public void RegisterRoutes(IMapRoute mapRouteManager) 
        {
            mapRouteManager.MapHttpRoute("SocialGroups", "default", "{controller}/{action}", new[] { "DotNetNuke.Modules.Groups" });
        }
    }
}
