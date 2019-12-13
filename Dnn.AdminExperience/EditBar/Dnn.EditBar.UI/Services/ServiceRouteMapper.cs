using System;
using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Framework.Reflections;
using DotNetNuke.Web.Api;

namespace Dnn.EditBar.UI.Services
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute routeManager)
        {
            routeManager.MapHttpRoute("editBar/Common",
                                      "default",
                                      "{controller}/{action}",
                                      new[] { "Dnn.EditBar.UI.Services" });
        }
    }
}
