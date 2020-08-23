// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web
{
    using DotNetNuke.DependencyInjection;
    using DotNetNuke.Web.Extensions;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Configures services for The DotNetNuke.Web project.</summary>
    public class Startup : IDnnStartup
    {
        /// <inheritdoc />
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddWebApi();
        }
    }
}
