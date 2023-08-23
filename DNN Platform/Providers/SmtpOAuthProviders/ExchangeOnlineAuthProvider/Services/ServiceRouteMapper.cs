// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExchangeOnlineAuthProvider.Services
{
    using DotNetNuke.Web.Api;

    /// <inheritdoc/>
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        /// <inheritdoc/>
        public void RegisterRoutes(IMapRoute routeManager)
        {
            routeManager.MapHttpRoute(
                "ExchangeOnlineAuth",
                "default",
                "{controller}/{action}",
                new[] { "Dnn.ExchangeOnlineAuthProvider.Services" });
        }
    }
}
