using DotNetNuke.Web.Api;

namespace DotNetNuke.Modules.MemberDirectory.Services
{
    public class MemberDirectoryServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("MemberDirectory", "default", "{controller}/{action}", new[] { "DotNetNuke.Modules.MemberDirectory.Services" });
        }
    }
}
