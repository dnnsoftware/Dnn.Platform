// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using DotNetNuke.Framework.Reflections;
    using DotNetNuke.Web.Api;

    public class ServiceRouteMapper : IServiceRouteMapper
    {
        private static readonly string[] Namespaces = ["Dnn.EditBar.UI.Services",];

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public void RegisterRoutes(IMapRoute routeManager)
        {
            routeManager.MapHttpRoute(
                "editBar/Common",
                "default",
                "{controller}/{action}",
                Namespaces);
        }
    }
}
