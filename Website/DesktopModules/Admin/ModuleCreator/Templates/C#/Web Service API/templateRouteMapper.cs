#region Copyright

// 
// Copyright (c) [YEAR]
// by [OWNER]
// 

#endregion

#region Using Statements

using DotNetNuke.Web.Api;

#endregion

namespace [OWNER].[MODULE]
{
    public class [MODULE]RouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("[OWNER].[MODULE]", "default", "{controller}/{action}", new[] {"[OWNER].[MODULE]"});
        }
    }
} 
