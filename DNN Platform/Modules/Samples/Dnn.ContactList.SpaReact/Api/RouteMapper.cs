using DotNetNuke.Web.Api;

namespace Dnn.ContactList.SpaReact.Api
{
    public class RouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Dnn/ContactListSpaReact", "DnnContactListSpaReact1", "{controller}/{action}", new[] { "Dnn.ContactList.SpaReact.Api" });
            mapRouteManager.MapHttpRoute("Dnn/ContactListSpaReact", "DnnContactListSpaReact2", "{controller}/{action}/{id}", null, new { id = @"-?\d+" }, new[] { "Dnn.ContactList.SpaReact.Api" });
        }
    }
}
