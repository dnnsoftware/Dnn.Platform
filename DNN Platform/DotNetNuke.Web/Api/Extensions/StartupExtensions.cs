// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Extensions
{
    using System;
    using System.Linq;

    using DotNetNuke.DependencyInjection.Extensions;
    using DotNetNuke.Web.Api;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>
    /// Adds DNN Web API Specific startup extensions to simplify the
    /// <see cref="Startup"/> Class.
    /// </summary>
    internal static class StartupExtensions
    {
        /// <summary>
        /// Configures all of the <see cref="DnnApiController"/>'s to be used
        /// with the Service Collection for Dependency Injection.
        /// </summary>
        /// <param name="services">
        /// Service Collection used to registering services in the container.
        /// </param>
        public static void AddWebApi(this IServiceCollection services)
        {
            var controllerTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.SafeGetTypes())
                .Where(x => typeof(DnnApiController).IsAssignableFrom(x) &&
                            x.IsClass &&
                            !x.IsAbstract);
            foreach (var controller in controllerTypes)
            {
                services.TryAddScoped(controller);
            }
        }
    }
}
