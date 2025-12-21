// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Steps
{
    using System;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Maintenance.Shims;
    using DotNetNuke.Maintenance.Telerik.Removal;
    using DotNetNuke.Services.Installer.Packages;

    /// <inheritdoc cref="IUninstallPackageStep" />
    internal sealed class UninstallPackageStep : StepBase, IUninstallPackageStep
    {
        private readonly IPackageController packageController;
        private readonly IApplicationStatusInfo applicationStatusInfo;
        private readonly Func<PackageInfo, string, IInstaller> installerFactory;

        /// <summary>Initializes a new instance of the <see cref="UninstallPackageStep"/> class.</summary>
        /// <param name="loggerSource">An instance of <see cref="ILoggerSource"/>.</param>
        /// <param name="localizer">An instance of <see cref="ILocalizer"/>.</param>
        /// <param name="packageController">An instance of <see cref="IPackageController"/>.</param>
        /// <param name="applicationStatusInfo">An instance of <see cref="IApplicationStatusInfo"/>.</param>
        /// <param name="installerFactory">A function that returns an instance of <see cref="IInstaller"/>.</param>
        public UninstallPackageStep(
            ILoggerSource loggerSource,
            ILocalizer localizer,
            IPackageController packageController,
            IApplicationStatusInfo applicationStatusInfo,
            Func<PackageInfo, string, IInstaller> installerFactory)
            : base(loggerSource, localizer)
        {
            this.packageController = packageController ??
                throw new ArgumentNullException(nameof(packageController));

            this.applicationStatusInfo = applicationStatusInfo ??
                throw new ArgumentNullException(nameof(applicationStatusInfo));

            this.installerFactory = installerFactory ??
                throw new ArgumentNullException(nameof(installerFactory));
        }

        /// <inheritdoc/>
        public override string Name => this.LocalizeFormat("UninstallStepUninstallExtension", this.PackageName);

        /// <inheritdoc/>
        public bool DeleteFiles { get; set; }

        /// <inheritdoc/>
        [Required]
        public string PackageName { get; set; }

        /// <inheritdoc/>
        protected override void ExecuteInternal()
        {
            bool WithMatchingName(PackageInfo p) =>
                p.Name.Equals(this.PackageName, StringComparison.OrdinalIgnoreCase);

            var packageInfo = this.packageController
                .GetExtensionPackages(Null.NullInteger, WithMatchingName)
                .FirstOrDefault();

            if (packageInfo == default)
            {
                this.Success = true;
                this.Notes = this.LocalizeFormat("UninstallStepPackageAlreadyUninstalled", this.PackageName);
                return;
            }

            var physicalPath = this.applicationStatusInfo.ApplicationMapPath;
            var installer = this.installerFactory(packageInfo, physicalPath);
            this.Success = installer.UnInstall(this.DeleteFiles);
        }
    }
}
