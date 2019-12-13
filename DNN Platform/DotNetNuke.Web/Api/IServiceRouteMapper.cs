using System;

namespace DotNetNuke.Web.Api
{
    public interface IServiceRouteMapper
    {
        void RegisterRoutes(IMapRoute mapRouteManager);
    }
}
