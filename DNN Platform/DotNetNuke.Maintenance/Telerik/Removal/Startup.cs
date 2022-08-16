// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Removal
{
    using System;

    using DotNetNuke.DependencyInjection;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Maintenance.Shims;
    using DotNetNuke.Maintenance.Telerik.Steps;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Localization;
    using Microsoft.Extensions.DependencyInjection;

    /// <inheritdoc />
    public class Startup : IDnnStartup
    {
        /// <inheritdoc />
#pragma warning disable CS3001 // Argument type is not CLS-compliant
        public void ConfigureServices(IServiceCollection services)
#pragma warning restore CS3001 // Argument type is not CLS-compliant
        {
            services.AddTransient<ITelerikUtils, TelerikUtils>();
            services.AddTransient<IDamUninstaller, DamUninstaller>();
            services.AddTransient<ITelerikUninstaller, TelerikUninstaller>();
            services.AddTransient<ILocalizer, Localizer>();

            // core
            services.AddTransient(provider => LocalizationProvider.Instance);
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
            services.AddTransient(this.InstallerFactory);

            // steps
            services.AddTransient<IClearCacheStep, ClearCacheStep>();
            services.AddTransient<IDeleteFilesStep, DeleteFilesStep>();
            services.AddTransient<IExecuteSqlStep, ExecuteSqlStep>();
            services.AddTransient<IInstallAvailablePackageStep, InstallAvailablePackageStep>();
            services.AddTransient<IRemoveExtensionStep, RemoveExtensionStep>();
            services.AddTransient<IRemoveItemFromCollectionStep, RemoveItemFromCollectionStep>();
            services.AddTransient<IRemoveTelerikBindingRedirectsStep, RemoveTelerikBindingRedirectsStep>();
            services.AddTransient<IRemoveTelerikRewriterRulesStep, RemoveTelerikRewriterRulesStep>();
            services.AddTransient<IReplacePortalTabModuleStep, ReplacePortalTabModuleStep>();
            services.AddTransient<IReplaceTabModuleStep, ReplaceTabModuleStep>();
            services.AddTransient<IUninstallPackageStep, UninstallPackageStep>();
        }

        private Func<PackageInfo, string, IInstaller> InstallerFactory(IServiceProvider provider) =>
            (package, physicalSitePath) => new InstallerShim(package, physicalSitePath);
    }
}
