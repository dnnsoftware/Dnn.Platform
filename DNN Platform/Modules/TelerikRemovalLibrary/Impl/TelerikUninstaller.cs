// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.TelerikRemovalLibrary.Impl
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Instrumentation;

    /// <inheritdoc />
    internal class TelerikUninstaller : ITelerikUninstaller
    {
        private readonly ILog log;
        private readonly IServiceProvider serviceProvider;
        private readonly List<UninstallSummaryItem> progress;

        /// <summary>
        /// Initializes a new instance of the <see cref="TelerikUninstaller"/> class.
        /// </summary>
        /// <param name="serviceProvider">An instance of <see cref="IServiceProvider"/>.</param>
        public TelerikUninstaller(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider ??
                throw new ArgumentNullException(nameof(serviceProvider));

            this.log = this.GetService<ILoggerSource>().GetLogger(typeof(TelerikUninstaller));

            this.progress = new List<UninstallSummaryItem>();
        }

        /// <inheritdoc/>
        public IEnumerable<UninstallSummaryItem> Progress => this.progress;

        /// <inheritdoc/>
        public void Execute()
        {
            var steps = new[]
            {
                this.InstallAvailableExtension(
                    "ResourceManager",
                    "DNNCE_ResourceManager_09.*_Install.resources",
                    "Module"),
                this.ReplaceModuleInPage("File Management", "Digital Asset Management", "ResourceManager"),
                this.ReplaceModuleInHostPage("File Management", "Digital Asset Management", "ResourceManager"),
                this.RemoveSystemAttributeFromPackage("DigitalAssetsManagement"),
                this.RemoveSystemAttributeFromPackage("DotNetNuke.Telerik.Web"),
                this.RemoveSystemAttributeFromPackage("DotNetNuke.Web.Deprecated"),
                this.RemoveSystemAttributeFromPackage("DotNetNuke.Website.Deprecated"),
                this.RemoveSystemAttributeFromPackage("Admin.Messaging"),
                this.ClearCache(),
                this.UninstallExtension("Digital Assets Management"),
                this.UninstallExtension("Messaging"),
                this.UninstallExtension("DotNetNuke Telerik Web Components"),
                this.UninstallExtension("DNN Deprecated Web Controls Library"),
                this.UninstallExtension("DotNetNuke Deprecated Website Codebehind files"),
                this.UninstallExtension("DNN Security HotFix 2017.1"),
                this.UninstallExtension("RadEditor Manager"),
                this.UpdateSiteUrlsConfig(),
                this.UpdateWebConfig(),
                this.RemoveUninstalledExtensionFiles("Library_DotNetNuke.Telerik_*"),
                this.RemoveUninstalledExtensionFiles("Library_DotNetNuke.Web.Deprecated_*"),
                this.RemoveUninstalledExtensionFiles("Library_DotNetNuke.Website.Deprecated_*"),
                this.RemoveUninstalledExtensionFiles("DNNSecurityHotFix*"),
            };

            var skip = false;

            foreach (var step in steps)
            {
                if (!skip)
                {
                    step.Execute();

                    var nullable = step.Success;
                    if (nullable.HasValue && nullable.Value == false)
                    {
                        skip = true;
                    }
                }

                this.progress.AddRange(UninstallSummaryItem.FromStep(step));
            }
        }

        private IStep InstallAvailableExtension(string packageName, string fileNamePattern, string packageType)
        {
            var step = this.GetService<IInstallAvailablePackageStep>();
            step.PackageFileNamePattern = fileNamePattern;
            step.PackageName = packageName;
            step.PackageType = packageType;
            return step;
        }

        private IStep ReplaceModuleInPage(string pageName, string oldModuleName, string newModuleName)
        {
            var step = this.GetService<IReplaceTabModuleStep>();
            step.PageName = pageName;
            step.OldModuleName = oldModuleName;
            step.NewModuleName = newModuleName;
            return step;
        }

        private IStep ReplaceModuleInHostPage(string pageName, string oldModuleName, string newModuleName)
        {
            var parentStep = this.GetService<IReplaceTabModuleStep>();
            parentStep.PageName = pageName;
            parentStep.OldModuleName = oldModuleName;
            parentStep.NewModuleName = newModuleName;

            var step = this.GetService<IReplacePortalTabModuleStep>();
            step.ParentStep = parentStep;
            step.PortalId = Null.NullInteger;

            return step;
        }

        private IStep RemoveSystemAttributeFromPackage(string packageName)
        {
            var commandFormat = string.Join(
                Environment.NewLine,
                "UPDATE {{databaseOwner}}{{objectQualifier}}Packages",
                "SET IsSystemPackage = 0",
                "WHERE [Name] = '{0}'");

            var step = this.GetService<IExecuteSqlStep>();
            step.InternalName = $"Remove the 'System' attribute from package '{packageName}'";
            step.CommandText = string.Format(commandFormat, packageName);

            return step;
        }

        private IStep ClearCache()
        {
            return this.GetService<IClearCacheStep>();
        }

        private IStep UninstallExtension(string packageName)
        {
            return this.NullStep($"Uninstall the '{packageName}' extension");
        }

        private IStep UpdateSiteUrlsConfig()
        {
            return this.NullStep("Remove all Telerik rewrite rules from the SiteUrls.config file");
        }

        private IStep UpdateWebConfig()
        {
            return this.NullStep("Remove all Telerik references from the Web.config file");
        }

        private IStep RemoveUninstalledExtensionFiles(string packageName)
        {
            return this.NullStep($"Remove extension files '{packageName}'");
        }

        private IStep NullStep(string name)
        {
            var step = this.GetService<INullStep>();
            step.InternalName = name;
            return step;
        }

        private T GetService<T>()
            where T : class
        {
            return (T)this.serviceProvider.GetService(typeof(T));
        }
    }
}
