// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.Web.Mvc.Extensions;
using System.Web.Mvc;
using DotNetNuke.Common;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotNetNuke.Web.Mvc
{
    public class Startup : IDnnStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.TryAddSingleton<IControllerFactory, DefaultControllerFactory>();
            services.AddSingleton<MvcModuleControlFactory>();

            services.AddMvcControllers();

            DependencyResolver.SetResolver(new DnnMvcDependencyResolver(Globals.DependencyProvider));
        }
    }
}
