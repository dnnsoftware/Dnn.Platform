// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.TelerikRemovalLibrary
{
    using Dnn.Modules.TelerikRemovalLibrary.Impl;
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
            // core
            services.AddTransient(provider => LoggerSource.Instance);
            services.AddTransient(provider => ModuleController.Instance);
            services.AddTransient(provider => PackageController.Instance);
            services.AddTransient(provider => TabController.Instance);

            // local
            services.AddTransient<IClearCacheStep, ClearCacheStep>();
            services.AddTransient<IDataCache, InternalDataCache>();
            services.AddTransient<IDataProvider, InternalDataProvider>();
            services.AddTransient<IDesktopModuleController, InternalDesktopModuleController>();
            services.AddTransient<IExecuteSqlStep, ExecuteSqlStep>();
            services.AddTransient<IModuleDefinitionController, InternalModuleDefinitionController>();
            services.AddTransient<ITelerikUninstaller, TelerikUninstaller>();
            services.AddTransient<IFileSystemProvider, FileSystemProvider>();
            services.AddTransient<IInstallAvailablePackageStep, InstallAvailablePackageStep>();
            services.AddTransient<INullStep, NullStep>();
            services.AddTransient<IReplacePortalTabModuleStep, ReplacePortalTabModuleStep>();
            services.AddTransient<IReplaceTabModuleStep, ReplaceTabModuleStep>();
        }
    }
}
