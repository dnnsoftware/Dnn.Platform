// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke
{
    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Abstractions.Portals.Templates;
    using DotNetNuke.Abstractions.Prompt;
    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.DependencyInjection;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Settings;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Portals.Templates;
    using DotNetNuke.Entities.Tabs.TabVersions;
    using DotNetNuke.Prompt;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Search.Controllers;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.UI.Modules.Html5;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <inheritdoc />
    public class Startup : IDnnStartup
    {
        /// <inheritdoc />
        public void ConfigureServices(IServiceCollection services)
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IModuleControlFactory, WebFormsModuleControlFactory>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IModuleControlFactory, Html5ModuleControlFactory>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IModuleControlFactory, ReflectedModuleControlFactory>());

            services.AddSingleton<IDnnContext, DotNetNukeContext>();

#pragma warning disable CS0618
            services.AddScoped<IEventLogger, EventLogController>();
            services.AddScoped<IEventLogConfigService, EventLogController>();
            services.AddScoped<IEventLogService, EventLogController>();
#pragma warning restore CS0618

            services.AddTransient<IPortalController, PortalController>();
            services.AddTransient<IBusinessControllerProvider, BusinessControllerProvider>();
            services.AddScoped<IHostSettingsService, HostController>();
            services.AddScoped<INavigationManager, NavigationManager>();
            services.AddScoped<ISerializationManager, SerializationManager>();

            services.AddScoped<IApplicationInfo, Application.Application>();
            services.AddScoped<IApplicationStatusInfo, ApplicationStatusInfo>();

            services.AddScoped<IPortalAliasService, PortalAliasController>();

            services.AddTransient<IFileSystemUtils, FileSystemUtilsProvider>();
            services.AddTransient<ICommandRepository, CommandRepository>();
            services.AddTransient<IPortalTemplateController, PortalTemplateController>();
            services.AddTransient<ITabVersionBuilder, TabVersionBuilder>();
            services.AddTransient<ISearchController, SearchControllerImpl>();
            services.AddTransient<IFolderMappingController, FolderMappingController>(_ => new FolderMappingController());
        }
    }
}
