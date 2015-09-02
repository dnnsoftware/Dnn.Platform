using DotNetNuke.Web.Api;

namespace OAuth.AuthorizationServer.API.Controllers
{
    class OauthServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("OAUTH", "default", "{controller}/{action}", new[] { "DNN.OAuth.AuthorizationServer.API.Controllers" });
        }
    }
}
