// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api;

/// <summary>A contract specifying the ability to register routes for an extension.</summary>
public interface IServiceRouteMapper
{
    /// <summary>Register the routes for an extension.</summary>
    /// <param name="mapRouteManager">The route mapper.</param>
    void RegisterRoutes(IMapRoute mapRouteManager);
}
