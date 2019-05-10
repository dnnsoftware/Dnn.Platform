using DotNetNuke.Web.Mvc.Framework.Controllers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace DotNetNuke.Web.Mvc.Extensions
{
    internal static class StartupExtensions
    {
        public static void AddMvc(this IServiceCollection services)
        {
            var startuptypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
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
