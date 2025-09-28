// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.ClientDependency
{
    using System;
    using System.Collections;
    using System.Web;
    using System.Web.UI;

    using DotNetNuke.Abstractions.ClientResources;
    using Microsoft.Extensions.DependencyInjection;

    internal class DependencyController
    {
        internal static IClientResourcesController GetClientResourcesController(Page page)
        {
            var serviceProvider = GetCurrentServiceProvider(page.Request.RequestContext.HttpContext);
            return serviceProvider.GetRequiredService<IClientResourcesController>();
        }

        internal static IServiceProvider GetCurrentServiceProvider(HttpContextBase context)
        {
            return GetScope(context.Items).ServiceProvider;

            // Copy of DotNetNuke.Common.Extensions.HttpContextDependencyInjectionExtensions.GetScope
            static IServiceScope GetScope(IDictionary httpContextItems)
            {
                return httpContextItems[typeof(IServiceScope)] as IServiceScope;
            }
        }
    }
}
