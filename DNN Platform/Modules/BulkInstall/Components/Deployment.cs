// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.BulkInstall.Components
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Web.Script.Serialization;

    using Dnn.Modules.BulkInstall.Components.DataAccess.Models;
    using Dnn.Modules.BulkInstall.Components.Logging;

    using DotNetNuke.Abstractions.Application;

    using Newtonsoft.Json;

    /// <summary>A deployment of installation packages.</summary>
    internal class Deployment
    {
        private readonly SessionManager sessionManager;
        private readonly EventLogManager eventLogManager;
        private readonly IApplicationStatusInfo appStatus;

        /// <summary>Initializes a new instance of the <see cref="Deployment"/> class.</summary>
        /// <param name="sessionManager">The session manager.</param>
        /// <param name="eventLogManager">The event log manager.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="serviceProvider">The DI container.</param>
        /// <param name="session">The session.</param>
        /// <param name="ipAddress">The IP address.</param>
        public Deployment(SessionManager sessionManager, EventLogManager eventLogManager, IApplicationStatusInfo appStatus, IServiceProvider serviceProvider, Session session, string ipAddress)
        {
            this.sessionManager = sessionManager;
            this.eventLogManager = eventLogManager;
            this.appStatus = appStatus;

            // Store ip address for logging later.
            this.IPAddress = ipAddress;

            // Store the session.
            this.Session = session;

            // Create the temporary directory if it doesn't exist.
            CreateDirectoryIfNotExist(this.TempPath);

            // Identify package zips.
            List<string> packageZips = this.IdentifyPackages();

            // Create install jobs.
            List<InstallJob> installJobs = new List<InstallJob>();
            List<PackageJob> packageJobs = new List<PackageJob>();

            foreach (string packageZip in packageZips)
            {
                InstallJob installJob = new InstallJob(this.appStatus, serviceProvider, packageZip);
                installJobs.Add(installJob);
                packageJobs.AddRange(installJob.Packages);
            }

            // Are package dependencies fulfilled?
            foreach (InstallJob installJob in installJobs)
            {
                installJob.CheckDependencies(packageJobs);
            }

            // Order jobs.
            this.OrderedInstall = OrderInstallJobs(installJobs);
        }

        /// <summary>Gets the path to a temp folder for this deployment.</summary>
        protected string TempPath => Path.Combine(this.sessionManager.PathForSession(this.Session.SessionGuid), "temp");

        /// <summary>Gets or sets the IP address requesting this deployment.</summary>
        protected string IPAddress { get; set; }

        /// <summary>Gets or sets the session for this deployment.</summary>
        protected Session Session { get; set; }

        /// <summary>Gets or sets a list of package zips.</summary>
        protected List<string> PackageZips { get; set; }

        /// <summary>Gets or sets a sorted list of <see cref="InstallJob"/> instances.</summary>
        protected SortedList<int, InstallJob> OrderedInstall { get; set; }

        /// <summary>Gets a summary of the deployment.</summary>
        /// <returns>A sorted list of <see cref="InstallJob"/> instances.</returns>
        public SortedList<int, InstallJob> Summary()
        {
            return this.OrderedInstall;
        }

        /// <summary>Attempts the installation of all the packages in the session.</summary>
        public void Deploy()
        {
            // Do the install.
            // Set as started.
            this.Session.Status = SessionStatus.InProgress;
            this.sessionManager.UpdateSession(this.Session);

            // Install in order.
            foreach (KeyValuePair<int, InstallJob> keyPair in this.OrderedInstall)
            {
                // Get install job.
                InstallJob job = keyPair.Value;

                // Attempt install.
                job.Install();

                // Log package installs.
                foreach (PackageJob package in job.Packages)
                {
                    string log = $"Package successfully installed: {package.Name} @ {package.VersionStr}, session: {this.Session.SessionGuid}.";

                    this.eventLogManager.Log("PACKAGE_INSTALLED", EventLogSeverity.Info, log);
                }

                // Make sorted list serializable.
                SortedList<string, InstallJob> serOrderedInstall = new SortedList<string, InstallJob>();
                foreach (KeyValuePair<int, InstallJob> pair in this.OrderedInstall)
                {
                    serOrderedInstall.Add(pair.Key.ToString(CultureInfo.InvariantCulture), pair.Value);
                }

                // After each install job, update response.
                this.Session.Response = JsonConvert.SerializeObject(serOrderedInstall);
                this.sessionManager.UpdateSession(this.Session);
            }

            // Done.
            this.Session.Status = SessionStatus.Complete;
            this.sessionManager.UpdateSession(this.Session);
        }

        /// <summary>Gets a value indicating whether the given zip file has a DNN manifest.</summary>
        /// <param name="filePath">The path to the package file.</param>
        /// <returns><see langword="true"/> if the file has a DNN manifest, otherwise <see langword="false"/>.</returns>
        protected static bool ZipHasDnnManifest(string filePath)
        {
            return ZipHasFileWithExtension(filePath, ".dnn");
        }

        /// <summary>Gets a value indicating whether the given zip file has another zip file.</summary>
        /// <param name="filePath">The path to the package file.</param>
        /// <returns><see langword="true"/> if the file has a zip file, otherwise <see langword="false"/>.</returns>
        protected static bool ZipHasOtherZip(string filePath)
        {
            return ZipHasFileWithExtension(filePath, ".zip");
        }

        /// <summary>Gets a value indicating whether the given zip file has another file with the specific <paramref name="extension"/>.</summary>
        /// <param name="filePath">The path to the package file.</param>
        /// <param name="extension">The file extension (with leading <c>.</c>).</param>
        /// <returns><see langword="true"/> if the file has a matching file, otherwise <see langword="false"/>.</returns>
        protected static bool ZipHasFileWithExtension(string filePath, string extension)
        {
            bool hasFile = false;

            try
            {
                using FileStream fs = File.OpenRead(filePath);
                ZipArchive archive = new ZipArchive(fs);

                // Loop entries in archive.
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (Path.GetExtension(entry.Name).Equals(extension, StringComparison.OrdinalIgnoreCase))
                    {
                        hasFile = true;
                        break;
                    }
                }
            }
            catch (Exception)
            {
                // Corrupt zip?
                return false;
            }

            return hasFile;
        }

        /// <summary>Ensures the directory has been created.</summary>
        /// <param name="directoryPath">The path.</param>
        protected static void CreateDirectoryIfNotExist(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        /// <summary>Logs job failures.</summary>
        /// <param name="jobs">The jobs.</param>
        protected virtual void LogAnyFailures(List<InstallJob> jobs)
        {
            // Nothing in here yet.
        }

        /// <summary>Gets a list of package files for the session.</summary>
        /// <returns>A list of file paths.</returns>
        protected List<string> IdentifyPackages()
        {
            return this.IdentifyPackagesInDirectory(this.sessionManager.PathForSession(this.Session.SessionGuid));
        }

        /// <summary>Gets a list of package files for the specified <paramref name="directoryPath" />.</summary>
        /// <param name="directoryPath">The path in which to look for package files.</param>
        /// <returns>A list of file paths.</returns>
        protected List<string> IdentifyPackagesInDirectory(string directoryPath)
        {
            List<string> packages = new List<string>();

            // Loop each file in directory.
            foreach (string testPath in Directory.GetFiles(directoryPath))
            {
                // Is it a zip file?
                if (Path.GetExtension(testPath).Equals(".zip", StringComparison.OrdinalIgnoreCase) && !Path.GetFileNameWithoutExtension(testPath).Equals("resources", StringComparison.OrdinalIgnoreCase))
                {
                    // Does it contain a module?
                    if (ZipHasDnnManifest(testPath))
                    {
                        // Yes, add to packages list.
                        packages.Add(testPath);
                    }
                    else
                    {
                        // Does it have other zips?
                        if (ZipHasOtherZip(testPath))
                        {
                            string tempPath = Utilities.AvailableTempDirectory(this.appStatus, this.TempPath);

                            CreateDirectoryIfNotExist(tempPath);

                            ZipFile.ExtractToDirectory(testPath, tempPath);

                            packages.AddRange(this.IdentifyPackagesInDirectory(tempPath));
                        }
                    }
                }
            }

            return packages;
        }

        private static SortedList<int, InstallJob> OrderInstallJobs(List<InstallJob> installJobs)
        {
            SortedList<int, InstallJob> orderedInstall = new SortedList<int, InstallJob>();

            foreach (InstallJob ij in installJobs)
            {
                // Already in the list?
                if (!orderedInstall.ContainsValue(ij))
                {
                    // No, add.
                    AddInstallJob(ij, orderedInstall, installJobs);
                }
            }

            return orderedInstall;
        }

        private static void AddInstallJob(InstallJob installJob, SortedList<int, InstallJob> orderedInstall, List<InstallJob> installJobs, List<InstallJob> dependencyStack = null)
        {
            // Initialise dependency stack if needed.
            if (dependencyStack == null)
            {
                dependencyStack = new List<InstallJob>();
            }

            // Is this job already in the dependency stack?
            if (dependencyStack.Contains(installJob))
            {
                // Yes, that's a circular dependency detection then.
                throw new InvalidOperationException("Circular package dependency!");
            }

            // Add this job to the dependency stack.
            dependencyStack.Add(installJob);

            // Loop packages in this install job.
            foreach (PackageJob pj in installJob.Packages)
            {
                // Loop dependencies in this package.
                foreach (PackageDependency pd in pj.Dependencies)
                {
                    // Is this dependency met by our deployment and is it a package dependency?
                    if (pd.DeployMet && pd.IsPackageDependency)
                    {
                        // Try and find the install job that provides this dependency.
                        InstallJob foundInstallDependency = FindInstallJobWithPackage(pd.PackageName, installJobs);

                        // Did we find it?
                        if (foundInstallDependency == null)
                        {
                            // No, unfulfilled dependency.
                            throw new InvalidOperationException("Unfulfilled package dependency.");
                        }

                        // Is it already in the ordered jobs?
                        if (!orderedInstall.ContainsValue(foundInstallDependency))
                        {
                            // No, add that install job first.
                            AddInstallJob(foundInstallDependency, orderedInstall, installJobs, dependencyStack);
                        }
                    }
                }
            }

            // Add ourself.
            orderedInstall.Add(orderedInstall.Count, installJob);
        }

        private static InstallJob FindInstallJobWithPackage(string name, List<InstallJob> installJobs)
        {
            foreach (InstallJob ij in installJobs)
            {
                foreach (PackageJob pj in ij.Packages)
                {
                    if (pj.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        return ij;
                    }
                }
            }

            return null;
        }
    }
}
