// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Routing
{
    public interface IMvcRouteMapper
    {
        /// <summary>
        /// Registers MVC routes using the provided route manager.
        /// </summary>
        /// <param name="mapRouteManager">The route manager used to map routes.</param>
        void RegisterRoutes(IMapRoute mapRouteManager);
    }
}
