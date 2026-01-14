// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.AzureConnector.Services
{
    using DotNetNuke.Web.Api;

    /// <inheritdoc/>
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        private static readonly string[] Namespaces = ["Dnn.AzureConnector.Services",];

        /// <inheritdoc/>
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute(
                "AzureConnector",
                "default",
                "{controller}/{action}",
                Namespaces);
        }
    }
}
