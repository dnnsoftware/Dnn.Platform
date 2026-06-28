// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.InternalServices;

using DotNetNuke.Web.Api;

/// <summary>A web API service route mapper for internal services.</summary>
public class ServiceRouteMapper : IServiceRouteMapper
{
    private static readonly string[] Namespaces = ["DotNetNuke.Web.InternalServices",];

    /// <inheritdoc />
    public void RegisterRoutes(IMapRoute mapRouteManager)
    {
        mapRouteManager.MapHttpRoute(
            "InternalServices",
            "default",
            "{controller}/{action}",
            Namespaces);
    }
}
