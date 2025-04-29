// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Services;

using DotNetNuke.Web.Api;

/// <inheritdoc />
public class RouteMapper : IServiceRouteMapper
{
    /// <inheritdoc />
    public void RegisterRoutes(IMapRoute mapRouteManager)
    {
        mapRouteManager.MapHttpRoute("ResourceManager", "default", "{controller}/{action}", new[] { "Dnn.Modules.ResourceManager.Services" });
    }
}
