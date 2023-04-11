// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc
{
    using System.Web.Mvc;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.DependencyInjection;
    using DotNetNuke.Web.Mvc.Extensions;
    using DotNetNuke.Web.Mvc.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    public class Startup : IDnnStartup
    {
        /// <inheritdoc/>
        public void ConfigureServices(IServiceCollection services)
        {
            services.TryAddSingleton<IControllerFactory, DefaultControllerFactory>();
            services.AddSingleton<MvcModuleControlFactory>();
            services.TryAddEnumerable(new ServiceDescriptor(typeof(IRoutingManager), typeof(MvcRoutingManager), ServiceLifetime.Singleton));

            services.AddMvcControllers();

            DependencyResolver.SetResolver(new DnnMvcDependencyResolver(Globals.DependencyProvider));
        }
    }
}
