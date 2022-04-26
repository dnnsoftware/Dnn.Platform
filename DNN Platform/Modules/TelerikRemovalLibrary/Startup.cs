// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.TelerikRemovalLibrary
{
    using DotNetNuke.DependencyInjection;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Installer.Packages;
    using Microsoft.Extensions.DependencyInjection;

    /// <inheritdoc />
    public class Startup : IDnnStartup
    {
        /// <inheritdoc />
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<ITelerikUninstaller, TelerikUninstaller>();

            // core
            services.AddTransient(provider => LoggerSource.Instance);
            services.AddTransient(provider => ModuleController.Instance);
            services.AddTransient(provider => PackageController.Instance);
            services.AddTransient(provider => TabController.Instance);

            // shims
            services.AddTransient<IDataCache, DataCacheShim>();
            services.AddTransient<IDataProvider, DataProviderShim>();
            services.AddTransient<IDesktopModuleController, DesktopModuleControllerShim>();
            services.AddTransient<IModuleDefinitionController, ModuleDefinitionControllerShim>();
            services.AddTransient<IFileSystemProvider, FileSystemProviderShim>();

            // steps
            services.AddTransient<IClearCacheStep, ClearCacheStep>();
            services.AddTransient<IExecuteSqlStep, ExecuteSqlStep>();
            services.AddTransient<IInstallAvailablePackageStep, InstallAvailablePackageStep>();
            services.AddTransient<INullStep, NullStep>();
            services.AddTransient<IReplacePortalTabModuleStep, ReplacePortalTabModuleStep>();
            services.AddTransient<IReplaceTabModuleStep, ReplaceTabModuleStep>();
        }
    }
}
