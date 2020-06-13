// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke
{
    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.DependencyInjection;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.UI.Modules.Html5;
    using Microsoft.Extensions.DependencyInjection;

    public class Startup : IDnnStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<WebFormsModuleControlFactory>();
            services.AddSingleton<Html5ModuleControlFactory>();
            services.AddSingleton<ReflectedModuleControlFactory>();

            services.AddTransient(x => PortalController.Instance);
            services.AddTransient<INavigationManager, NavigationManager>();
        }
    }
}
