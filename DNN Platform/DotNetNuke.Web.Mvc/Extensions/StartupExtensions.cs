// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Mvc.Extensions
{
    using System;
    using System.Linq;

    using DotNetNuke.DependencyInjection.Extensions;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Web.Mvc.Framework.Controllers;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>
    /// Adds DNN MVC Controller Specific startup extensions to simplify the
    /// <see cref="Startup"/> Class.
    /// </summary>
    public static class StartupExtensions
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(StartupExtensions));

        /// <summary>
        /// Configures all of the <see cref="DnnController"/>'s to be used
        /// with the Service Collection for Dependency Injection.
        /// </summary>
        /// <param name="services">
        /// Service Collection used to registering services in the container.
        /// </param>
        public static void AddMvcControllers(this IServiceCollection services)
        {
            var controllerTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.SafeGetTypes(Logger))
                .Where(x => typeof(IDnnController).IsAssignableFrom(x)
                    && x.IsClass
                    && !x.IsAbstract);
            foreach (var controller in controllerTypes)
            {
                services.TryAddTransient(controller);
            }
        }
    }
}
