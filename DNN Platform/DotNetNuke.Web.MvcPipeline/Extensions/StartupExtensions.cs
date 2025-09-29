// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Mvc.Extensions
{
    using System.Linq;

    using DotNetNuke.DependencyInjection.Extensions;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Web.MvcPipeline.Controllers;
    using DotNetNuke.Web.MvcPipeline.ModuleControl;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>Adds DNN MVC Controller Specific startup extensions to simplify the <see cref="Startup"/> Class.</summary>
    public static class StartupExtensions
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(StartupExtensions));

        /// <summary>Configures all of the <see cref="IMvcController"/>'s to be used with the Service Collection for Dependency Injection.</summary>
        /// <param name="services">Service Collection used to registering services in the container.</param>
        public static void AddMvcControllers(this IServiceCollection services)
        {
            var allTypes = TypeExtensions.SafeGetTypes();
            allTypes.LogOtherExceptions(Logger);

            var mvcControllerTypes = allTypes.Types
                .Where(
                    type => typeof(IMvcController).IsAssignableFrom(type) &&
                            type is { IsClass: true, IsAbstract: false });
            foreach (var controller in mvcControllerTypes)
            {
                services.TryAddTransient(controller);
            }

            var mvcControlTypes = allTypes.Types
                .Where(
                    type => typeof(IMvcModuleControl).IsAssignableFrom(type) &&
                            type is { IsClass: true, IsAbstract: false });
            foreach (var controller in mvcControlTypes)
            {
                services.TryAddTransient(controller);
            }
        }
    }
}
