// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
