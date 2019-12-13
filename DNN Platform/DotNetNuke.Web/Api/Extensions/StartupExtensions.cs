// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.DependencyInjection.Extensions;
using DotNetNuke.Web.Api;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace DotNetNuke.Web.Extensions
{
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
            var startuptypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.SafeGetTypes())
                .Where(x => typeof(DnnApiController).IsAssignableFrom(x) &&
                            x.IsClass &&
                            !x.IsAbstract);
            foreach (var controller in startuptypes)
            {
                services.AddTransient(controller);
            }
        }
    }
}
