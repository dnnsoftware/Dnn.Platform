// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using DotNetNuke.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetNuke.Web.Razor
{
    public class Startup : IDnnStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<RazorModuleControlFactory>();
        }
    }
}
