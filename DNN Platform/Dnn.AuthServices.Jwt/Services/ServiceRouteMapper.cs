// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.AuthServices.Jwt.Services;

using DotNetNuke.Web.Api;

/// <summary>Registers the API routes for this extension.</summary>
public class ServiceRouteMapper : IServiceRouteMapper
{
    /// <inheritdoc/>
    public void RegisterRoutes(IMapRoute mapRouteManager)
    {
        mapRouteManager.MapHttpRoute("JwtAuth", "default", "{controller}/{action}", new[] { this.GetType().Namespace });
    }
}
