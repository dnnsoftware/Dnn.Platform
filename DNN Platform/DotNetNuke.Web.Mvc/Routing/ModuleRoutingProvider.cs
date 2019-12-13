using System.Web;
using System.Web.Routing;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Modules;
using DotNetNuke.UI.Modules;

namespace DotNetNuke.Web.Mvc.Routing
{
    public abstract class ModuleRoutingProvider
    {
        public static ModuleRoutingProvider Instance()
        {
            var component = ComponentFactory.GetComponent<ModuleRoutingProvider>();

            if (component == null)
            {
                component = new StandardModuleRoutingProvider();
                ComponentFactory.RegisterComponentInstance<ModuleRoutingProvider>(component);
            }

            return component;
        }

        public abstract string GenerateUrl(RouteValueDictionary routeValues, ModuleInstanceContext moduleContext);

        public abstract RouteData GetRouteData(HttpContextBase httpContext, ModuleControlInfo moduleControl);
    }
}
