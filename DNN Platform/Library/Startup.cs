// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke
{
    using System;
    using System.Linq;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Abstractions.Portals.Templates;
    using DotNetNuke.Abstractions.Prompt;
    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Data.PetaPoco;
    using DotNetNuke.DependencyInjection;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Settings;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Portals.Templates;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Tabs.TabVersions;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Framework.Reflections;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Prompt;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Installer;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Journal;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Mail.OAuth;
    using DotNetNuke.Services.Mobile;
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.Services.Search.Controllers;
    using DotNetNuke.Services.Search.Internals;
    using DotNetNuke.Services.UserRequest;
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
            services.AddTransient(typeof(Lazy<>), typeof(LazyWrapper<>));

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IModuleControlFactory, WebFormsModuleControlFactory>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IModuleControlFactory, Html5ModuleControlFactory>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IModuleControlFactory, ReflectedModuleControlFactory>());
            services.AddSingleton<IDnnContext, DotNetNukeContext>();
            services.AddSingleton<IHostSettings, HostSettings>();
            services.AddTransient<IMailSettings, MailSettings>();

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

            services.AddScoped<IPermissionDefinitionService, PermissionController>();

            services.AddTransient<IFileSystemUtils, FileSystemUtilsProvider>();
            services.AddTransient<ISmtpOAuthController, SmtpOAuthController>();
            SmtpOAuthController.RegisterOAuthProviders(services);
            services.AddTransient<ICommandRepository, CommandRepository>();
            services.AddTransient<IPortalTemplateController, PortalTemplateController>();
            services.AddTransient<ITabVersionBuilder, TabVersionBuilder>();
            services.AddTransient<ISearchController, SearchControllerImpl>();
            services.AddTransient<IFolderMappingController, FolderMappingController>(_ => new FolderMappingController());

            // TODO: LocalizationProvider can be overridden via the ComponentFactory, need to be able to get an instance registered via ComponentFactory without creating a dependency loop
            services.AddTransient<ILocalizationProvider, LocalizationProvider>();
            services.AddTransient<IFileManager, FileManager>();
            services.AddTransient<IFolderManager, FolderManager>();
            services.AddTransient(_ => ComponentFactory.GetComponent<DataProvider>());

            services.AddTransient<ILoggerSource, LoggerSourceImpl>();
            services.AddTransient<IModuleController, ModuleController>();
            services.AddTransient<IPackageController, PackageController>();
            services.AddTransient<ITabController, TabController>();
            services.AddTransient<IRedirectionController, RedirectionController>();
            services.AddTransient<ICBO, CBO>();
            services.AddTransient<IJavaScriptLibraryController, JavaScriptLibraryController>();
            services.AddTransient<IJavaScriptLibraryHelper, JavaScript>();
            services.AddTransient<IPortalSettingsController, PortalSettingsController>();
            services.AddTransient<IPortalAliasController, PortalAliasController>();
            services.AddTransient<IPortalGroupController, PortalGroupController>();
            services.AddTransient<ILocaleController, LocaleController>();
            services.AddTransient<IUserRequestIPAddressController, UserRequestIPAddressController>();
            services.AddTransient<IRoleController, RoleController>();
            services.AddTransient(_ => RoleProvider.Instance());
            services.AddTransient<IUserController, UserController>();
            services.AddTransient<IEventManager, EventManager>();
            services.AddTransient<ILocalUpgradeService, LocalUpgradeService>();
            services.AddTransient<IJournalDataService, JournalDataServiceImpl>();
            services.AddTransient<IFileContentTypeManager, FileContentTypeManager>();
            services.AddTransient<ISearchHelper, SearchHelperImpl>();

            services.AddTransient<IDataContext>(serviceProvider =>
            {
                var dataProvider = serviceProvider.GetRequiredService<DataProvider>();
                var defaultConnectionStringName = dataProvider.Settings["connectionStringName"];

                return new PetaPocoDataContext(defaultConnectionStringName, DataProvider.Instance().ObjectQualifier);
            });

            services.AddTransient<ModuleInjectionManager>();
            services.AddTransient<PersonalizationController>();
            services.AddTransient(_ => PortalSecurity.Instance);
            RegisterModuleInjectionFilters(services);
        }

        private static void RegisterModuleInjectionFilters(IServiceCollection services)
        {
            var filterTypes = new TypeLocator().GetAllMatchingTypes(IsValidModuleInjectionFilter);
            services.TryAddEnumerable(filterTypes.Select(t => new ServiceDescriptor(typeof(IModuleInjectionFilter), t, ServiceLifetime.Scoped)));
        }

        private static bool IsValidModuleInjectionFilter(Type t)
        {
            return t is { IsClass: true, IsAbstract: false, IsVisible: true } && typeof(IModuleInjectionFilter).IsAssignableFrom(t);
        }
    }
}
