// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Razor
{
    using DotNetNuke.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection;

    public class Startup : IDnnStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<RazorModuleControlFactory>();
        }
    }
}
