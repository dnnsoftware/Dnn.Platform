// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Services
{
    using System.Diagnostics.CodeAnalysis;

    using DotNetNuke.Web.Api;

    public class ServiceRouteMapper : IServiceRouteMapper
    {
        private static readonly string[] Namespaces = [typeof(ServiceRouteMapper).Namespace,];

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public void RegisterRoutes(IMapRoute routeManager)
        {
            routeManager.MapHttpRoute(
                "SiteExportImport",
                "default",
                "{controller}/{action}",
                Namespaces);
        }
    }
}
