// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Extensions;

using System.Linq;

using DotNetNuke.DependencyInjection.Extensions;
using DotNetNuke.Instrumentation;
using DotNetNuke.Web.Api;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>Adds DNN Web API Specific startup extensions to simplify the <see cref="Startup"/> Class.</summary>
internal static class StartupExtensions
{
    private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(StartupExtensions));

    /// <summary>Configures all of the <see cref="DnnApiController"/>'s to be used with the Service Collection for Dependency Injection.</summary>
    /// <param name="services">Service Collection used to registering services in the container.</param>
    public static void AddWebApi(this IServiceCollection services)
    {
        var allTypes = TypeExtensions.SafeGetTypes();
        allTypes.LogOtherExceptions(Logger);

        var controllerTypes = allTypes.Types
            .Where(
                type => typeof(DnnApiController).IsAssignableFrom(type) &&
                        type is { IsClass: true, IsAbstract: false });
        foreach (var controller in controllerTypes)
        {
            services.TryAddScoped(controller);
        }
    }
}
