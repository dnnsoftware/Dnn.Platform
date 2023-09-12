// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.ExportImport
{
    using System.Linq;

    using Dnn.ExportImport.Components.Controllers;
    using Dnn.ExportImport.Components.Engines;
    using Dnn.ExportImport.Components.Interfaces;
    using Dnn.ExportImport.Components.Services;

    using DotNetNuke.DependencyInjection;
    using DotNetNuke.Framework.Reflections;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>Register services from Export/Import module.</summary>
    public class Startup : IDnnStartup
    {
        /// <inheritdoc/>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IEntitiesController, EntitiesController>();
            services.AddTransient<ExportImportEngine>();
            services.AddTransient<ImportController>();
            services.AddTransient<ExportController>();
            services.AddTransient<BaseController>();

            var portableServiceTypes = new TypeLocator().GetAllMatchingTypes(
                t => t is { IsClass: true, IsAbstract: false, IsVisible: true } && typeof(BasePortableService).IsAssignableFrom(t));
            services.TryAddEnumerable(portableServiceTypes.Select(type => ServiceDescriptor.Transient(typeof(BasePortableService), type)));
        }
    }
}
