using DotNetNuke.Web.Mvc.Framework.Controllers;
using DotNetNuke.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace DotNetNuke.Web.Mvc.Extensions
{
    /// <summary>
    /// Adds DNN MVC Specific startup extensions to simplify the
    /// <see cref="Startup"/> Class.
    /// </summary>
    internal static class StartupExtensions
    {
        /// <summary>
        /// Configures all of the <see cref="IDnnController"/>'s to be used
        /// with the Service Collection for Dependency Injection.
        /// </summary>
        /// <param name="services">
        /// Service Collection used to registering services in the container.
        /// </param>
        public static void AddMvc(this IServiceCollection services)
        {
            var startuptypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.SafeGetTypes())
                .Where(x => typeof(IDnnController).IsAssignableFrom(x) &&
                            x.IsClass &&
                            !x.IsAbstract);
            foreach (var controller in startuptypes)
            {
                services.AddTransient(controller);
            }
        }
    }
}
