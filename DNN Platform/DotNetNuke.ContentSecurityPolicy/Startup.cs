// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc
{
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.ContentSecurityPolicy;
    using DotNetNuke.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection;

    /// <inheritdoc />
    public class Startup : IDnnStartup
    {
        /// <inheritdoc/>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IContentSecurityPolicy, ContentSecurityPolicy>();
        }
    }
}
