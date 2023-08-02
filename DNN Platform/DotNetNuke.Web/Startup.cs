// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web
{
    using System.Web.Http.Filters;

    using DotNetNuke.Common.Internal;
    using DotNetNuke.DependencyInjection;
    using DotNetNuke.Web.Api.Auth.ApiTokens;
    using DotNetNuke.Web.Api.Auth.ApiTokens.Repositories;
    using DotNetNuke.Web.Api.Internal;
    using DotNetNuke.Web.Extensions;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    using ServicesRoutingManager = DotNetNuke.Web.Api.Internal.ServicesRoutingManager;

    /// <summary>Configures services for The DotNetNuke.Web project.</summary>
    public class Startup : IDnnStartup
    {
        /// <inheritdoc />
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IFilterProvider, DnnActionFilterProvider>();
            services.TryAddEnumerable(new ServiceDescriptor(typeof(IRoutingManager), typeof(ServicesRoutingManager), ServiceLifetime.Singleton));
            services.AddWebApi();

            services.AddTransient<IApiTokenController, ApiTokenController>();
            services.AddTransient<IApiTokenRepository, ApiTokenRepository>(_ => new ApiTokenRepository());
        }
    }
}
