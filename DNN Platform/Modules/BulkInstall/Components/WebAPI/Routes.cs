// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.BulkInstall.Components.WebAPI
{
    using DotNetNuke.Web.Api;

    /// <summary>Registers web API routes for Bulk Install.</summary>
    public class Routes : IServiceRouteMapper
    {
        /// <inheritdoc />
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute(
                "BulkInstall",
                "default",
                "{controller}/{action}",
                [this.GetType().Namespace,]);
        }
    }
}
