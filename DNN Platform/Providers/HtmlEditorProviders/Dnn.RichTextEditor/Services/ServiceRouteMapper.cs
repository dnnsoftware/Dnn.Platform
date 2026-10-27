// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.RichTextEditorProvider.Services
{
    using DotNetNuke.Web.Api;

    /// <summary>Registers the provider's services under <c>/API/RichTextEditor/{controller}/{action}</c>.</summary>
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        /// <summary>The module folder name used in the service URLs.</summary>
        public const string ModuleFolderName = "RichTextEditor";

        /// <inheritdoc/>
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute(ModuleFolderName, "default", "{controller}/{action}", new[] { typeof(ServiceRouteMapper).Namespace, });
        }
    }
}
