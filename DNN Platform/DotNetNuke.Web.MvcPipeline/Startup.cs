// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline
{
    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.DependencyInjection;
    using DotNetNuke.Web.Mvc.Extensions;
    using DotNetNuke.Web.MvcPipeline.Framework;
    using DotNetNuke.Web.MvcPipeline.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using System.Web.Mvc;

    public class Startup : IDnnStartup
    {
        /// <inheritdoc/>
        public void ConfigureServices(IServiceCollection services)
        {
            services.TryAddEnumerable(new ServiceDescriptor(typeof(IRoutingManager), typeof(MvcRoutingManager), ServiceLifetime.Singleton));

            services.AddMvcControllers();
            services.AddTransient<IPageModelFactory, PageModelFactory>();
            services.AddTransient<ISkinModelFactory, SkinModelFactory>();
            services.AddTransient<IPaneModelFactory, PaneModelFactory>();
            services.AddTransient<IContainerModelFactory, ContainerModelFactory>();

            DependencyResolver.SetResolver(new DnnMvcPipelineDependencyResolver(Globals.DependencyProvider));

            //TODO: CSP - enable when CSP implementation is ready
            //services.AddScoped<IContentSecurityPolicy, ContentSecurityPolicy>();
        }
    }
}
