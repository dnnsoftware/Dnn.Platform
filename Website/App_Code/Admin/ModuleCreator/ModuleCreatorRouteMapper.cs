#region Copyright

// 
// Copyright (c) 2013
// by DotNetNuke
// 

#endregion

#region Using Statements

using DotNetNuke.Web.Api;

#endregion

namespace DotNetNuke.ModuleCreator
{
    public class ModuleCreatorRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Admin/ModuleCreator", "default", "{controller}/{action}", new[] { "DotNetNuke.ModuleCreator" });
        }
    }
} 

