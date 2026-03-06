// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.BulkInstall.Components
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Services.Installer;
    using DotNetNuke.Services.Installer.Installers;
    using DotNetNuke.Services.Installer.Log;

    using Newtonsoft.Json;

    /// <summary>Information about a request to install a package.</summary>
    internal sealed class InstallJob
    {
        private readonly IApplicationStatusInfo appStatus;

        /// <summary>Initializes a new instance of the <see cref="InstallJob"/> class.</summary>
        /// <param name="appStatus">The application status.</param>
        /// <param name="serviceProvider">The DI container.</param>
        /// <param name="path">The path to the package file.</param>
        public InstallJob(IApplicationStatusInfo appStatus, IServiceProvider serviceProvider, string path)
        {
            this.appStatus = appStatus;

            this.Name = Path.GetFileName(path);
            this.Packages = new List<PackageJob>();
            this.Failures = new List<string>();
            this.Attempted = false;
            this.Success = false;
            this.Installer = new Installer(new FileStream(path, FileMode.Open, FileAccess.Read), appStatus.ApplicationMapPath, true, false);

            foreach (KeyValuePair<int, PackageInstaller> orderedPackage in this.Installer.Packages)
            {
                this.Packages.Add(new PackageJob(serviceProvider, orderedPackage.Value));
            }
        }

        [JsonConstructor]
        private InstallJob()
        {
        }

        /// <summary>Gets or sets the name of the package file to install.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the packages in the package file.</summary>
        public List<PackageJob> Packages { get; set; }

        /// <summary>Gets or sets the failure messages.</summary>
        public List<string> Failures { get; set; }

        /// <summary>Gets or sets a value indicating whether the installation has been attempted.</summary>
        public bool Attempted { get; set; }

        /// <summary>Gets or sets a value indicating whether the installation was successful.</summary>
        public bool Success { get; set; }

        /// <summary>Gets a value indicating whether the installation can proceed.</summary>
        public bool CanInstall
        {
            get
            {
                foreach (PackageJob package in this.Packages)
                {
                    if (!package.CanInstall)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private Installer Installer { get; set; }

        /// <summary>Updates <see cref="PackageDependency.DeployMet"/> for all the dependencies of <see cref="Packages"/>.</summary>
        /// <param name="packageJobs">All the package jobs for every package file.</param>
        public void CheckDependencies(List<PackageJob> packageJobs)
        {
            foreach (PackageJob package in this.Packages)
            {
                foreach (PackageDependency packageDependency in package.Dependencies)
                {
                    if (packageDependency.IsPackageDependency)
                    {
                        if (FindDependency(packageDependency, packageJobs))
                        {
                            packageDependency.DeployMet = true;
                        }
                    }
                }
            }
        }

        /// <summary>Attempts to install the package file.</summary>
        public void Install()
        {
            // Record that we have attempted an install.
            this.Attempted = true;

            // Can this be installed at this point?
            if (this.CanInstall)
            {
                // Possibly need to recreate the installer at this point.
                this.Installer = new Installer(this.Installer.TempInstallFolder, ModuleManifestName(this.Installer.TempInstallFolder), this.appStatus.ApplicationMapPath, true);

                // Is the installer valid?
                if (this.Installer.IsValid)
                {
                    // Already installed?
                    if (this.Installer.InstallerInfo.Installed)
                    {
                        // Yes, make a repair install.
                        this.Installer.InstallerInfo.RepairInstall = true;
                    }

                    // Install.
                    this.Installer.Install();

                    // Did the package install successfully?
                    this.Success = this.Installer.IsValid;
                }

                // Record failures.
                foreach (LogEntry log in this.Installer.InstallerInfo.Log.Logs)
                {
                    if (log.Type.Equals(LogType.Failure))
                    {
                        string failure = log.ToString();

                        this.Failures.Add(failure);
                    }
                }
            }
        }

        private static bool FindDependency(PackageDependency dependency, List<PackageJob> packageJobs)
        {
            foreach (PackageJob pj in packageJobs)
            {
                if (!pj.Name.Equals(dependency.PackageName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!Version.TryParse(dependency.DependencyVersion, out var requiredVersion))
                {
                    return true;
                }

                if (Version.TryParse(pj.VersionStr, out var packageVersion) && packageVersion >= requiredVersion)
                {
                    return true;
                }
            }

            return false;
        }

        private static string ModuleManifestName(string directory)
        {
            string manifestFileName = null;
            foreach (string filePath in Directory.GetFiles(directory))
            {
                if (filePath.EndsWith(".dnn", StringComparison.OrdinalIgnoreCase))
                {
                    if (manifestFileName == null)
                    {
                        manifestFileName = Path.GetFileName(filePath);
                    }
                    else
                    {
                        throw new InvalidOperationException("More than one manifest found.");
                    }
                }
            }

            return manifestFileName;
        }
    }
}
