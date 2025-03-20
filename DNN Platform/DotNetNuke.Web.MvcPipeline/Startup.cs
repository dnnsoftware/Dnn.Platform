// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline
{
  using DotNetNuke.Common;
  using DotNetNuke.ContentSecurityPolicy;
  using DotNetNuke.DependencyInjection;
  using DotNetNuke.Web.Mvc.Extensions;
  using Microsoft.Extensions.DependencyInjection;
  using System.Web.Mvc;

  public class Startup : IDnnStartup
  {
    /// <inheritdoc/>
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddMvcControllers();
            services.AddTransient<IPageModelFactory, PageModelFactory>();
            services.AddTransient<ISkinModelFactory, SkinModelFactory>();
            services.AddTransient<IPaneModelFactory, PaneModelFactory>();
            services.AddTransient<IContainerModelFactory, ContainerModelFactory>();

      DependencyResolver.SetResolver(new DnnMvcPipelineDependencyResolver(Globals.DependencyProvider));
      services.AddScoped<IContentSecurityPolicy, ContentSecurityPolicy>();
    }
  }
}
