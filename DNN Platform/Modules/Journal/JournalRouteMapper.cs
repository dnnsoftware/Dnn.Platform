using System;

using DotNetNuke.Web.Api;

namespace DotNetNuke.Modules.Journal
{
    public class JournalRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Journal", "default", "{controller}/{action}", new[] { "DotNetNuke.Modules.Journal" });
        }
    }
}
