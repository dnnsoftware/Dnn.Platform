// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Steps
{
    using System;
    using System.IO;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Maintenance.Shims;
    using DotNetNuke.Maintenance.Telerik.Removal;
    using DotNetNuke.Services.Installer;
    using DotNetNuke.Services.Installer.Packages;

    /// <inheritdoc />
    internal class InstallAvailablePackageStep : StepBase, IInstallAvailablePackageStep
    {
        private readonly IApplicationStatusInfo appStatusInfo;
        private readonly IFileSystemProvider fsProvider;
        private readonly IPackageController packageController;

        /// <summary>Initializes a new instance of the <see cref="InstallAvailablePackageStep"/> class.</summary>
        /// <param name="loggerSource">An instance of <see cref="ILoggerSource"/>.</param>
        /// <param name="localizer">An instance of <see cref="ILocalizer"/>.</param>
        /// <param name="appStatusInfo">An instance of <see cref="IApplicationStatusInfo"/>.</param>
        /// <param name="fsProvider">An instance of <see cref="IFileSystemProvider"/>.</param>
        /// <param name="packageController">An instance of <see cref="IPackageController"/>.</param>
        public InstallAvailablePackageStep(
            ILoggerSource loggerSource,
            ILocalizer localizer,
            IApplicationStatusInfo appStatusInfo,
            IFileSystemProvider fsProvider,
            IPackageController packageController)
            : base(loggerSource, localizer)
        {
            this.appStatusInfo = appStatusInfo ??
                throw new ArgumentNullException(nameof(appStatusInfo));

            this.fsProvider = fsProvider ??
                throw new ArgumentNullException(nameof(fsProvider));

            this.packageController = packageController ??
                throw new ArgumentNullException(nameof(packageController));
        }

        /// <inheritdoc/>
        public override string Name => this.LocalizeFormat("UninstallStepInstallPackage", this.PackageName);

        /// <inheritdoc/>
        [Required]
        public string PackageFileNamePattern { get; set; }

        /// <inheritdoc/>
        [Required]
        public string PackageName { get; set; }

        /// <inheritdoc/>
        [Required]
        public string PackageType { get; set; }

        /// <inheritdoc/>
        protected override void ExecuteInternal()
        {
            if (this.PackageIsAlreadyInstalled())
            {
                this.Success = true;
                this.Notes = this.LocalizeFormat("UninstallStepPackageAlreadyInstalled", this.PackageName);
                return;
            }

            var availableFiles = this.fsProvider.EnumerateFiles(
                Path.Combine(this.appStatusInfo.ApplicationMapPath, "Install", this.PackageType),
                this.PackageFileNamePattern,
                SearchOption.TopDirectoryOnly);

            var filePath = availableFiles
                .OrderByDescending(path => path)
                .FirstOrDefault();

            if (filePath is null)
            {
                this.Success = false;
                this.Notes = this.LocalizeFormat("UninstallStepPackageNotFound", this.PackageFileNamePattern);
                return;
            }

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var installer = new Installer(
                    stream,
                    this.appStatusInfo.ApplicationMapPath,
                    loadManifest: false,
                    deleteTemp: false);

                try
                {
                    installer.InstallerInfo.PortalID = Null.NullInteger;

                    if (installer.InstallerInfo.ManifestFile != null)
                    {
                        installer.ReadManifest(true);
                    }

                    installer.InstallerInfo.Log.Logs.Clear();
                    installer.InstallerInfo.IgnoreWhiteList = true;
                    installer.InstallerInfo.RepairInstall = true;

                    installer.Install();

                    if (!installer.IsValid)
                    {
                        this.Success = false;
                        this.Notes = string.Join(Environment.NewLine, installer.InstallerInfo.Log.Logs);
                    }
                    else
                    {
                        this.DeleteInstallFile(filePath);

                        this.Success = true;
                    }
                }
                finally
                {
                    this.DeleteTempInstallFiles(installer);
                }
            }
        }

        private void DeleteInstallFile(string installerFile)
        {
            if (this.fsProvider.FileExists(installerFile))
            {
                this.fsProvider.SetFileAttributes(installerFile, FileAttributes.Normal);
                this.fsProvider.DeleteFile(installerFile);
            }
        }

        private void DeleteTempInstallFiles(Installer installer)
        {
            var tempFolder = installer.TempInstallFolder;

            if (!string.IsNullOrEmpty(tempFolder) && this.fsProvider.DirectoryExists(tempFolder))
            {
                this.fsProvider.DeleteFolderRecursive(tempFolder);
            }
        }

        private bool PackageIsAlreadyInstalled()
        {
            bool WithMatchingPackageName(PackageInfo p) =>
                "Module".Equals(p.PackageType, StringComparison.OrdinalIgnoreCase) &&
                this.PackageName.Equals(p.Name, StringComparison.OrdinalIgnoreCase);

            return this.packageController.GetExtensionPackages(Null.NullInteger, WithMatchingPackageName).Any();
        }
    }
}
