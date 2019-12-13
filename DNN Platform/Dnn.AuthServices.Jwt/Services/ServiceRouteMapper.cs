using DotNetNuke.Web.Api;

namespace Dnn.AuthServices.Jwt.Services
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute(
                "JwtAuth", "default", "{controller}/{action}", new[] { GetType().Namespace });
        }
    }
}
