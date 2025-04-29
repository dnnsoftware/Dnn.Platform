// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DNNConnect.CKEditorProvider.Services;

using DotNetNuke.Web.Api;

/// <inheritdoc />
public class CKEditorProviderRouteMapper : IServiceRouteMapper
{
    /// <inheritdoc />
    public void RegisterRoutes(IMapRoute mapRouteManager)
    {
        mapRouteManager.MapHttpRoute("CKEditorProvider", "default", "{controller}/{action}", new[] { "DNNConnect.CKEditorProvider.Services" });
    }
}
