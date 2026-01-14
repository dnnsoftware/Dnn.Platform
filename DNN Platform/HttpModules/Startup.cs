// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.HttpModules;

using DotNetNuke.DependencyInjection;
using DotNetNuke.Entities.Urls;

using Microsoft.Extensions.DependencyInjection;

/// <summary>The <see cref="IDnnStartup"/> for <c>DotNetNuke.HttpModules</c>.</summary>
public class Startup : IDnnStartup
{
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<UrlRewriterBase>(serviceProvider => UrlRewriteModule.GetUrlRewriterInstance(serviceProvider));
    }
}
