using DotNetNuke.Modules.Dashboard.Components.Server;
using DotNetNuke.Services.Installer;
using DotNetNuke.Services.Installer.Dependencies;
using DotNetNuke.Services.Installer.Installers;
using DotNetNuke.Services.Installer.Packages;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.XPath;

namespace Cantarus.Modules.PolyDeploy.Components
{
    internal class InstallManager
    {
        private List<InstallJob> InstallJobs { get; set; }
        public SortedList<int, PackageJob> OrderedPackages { get; set; }

        private string directory { get; set; }

        public InstallManager(string dir)
        {
            // Initialise install jobs list.
            InstallJobs = new List<InstallJob>();

            // Check that the passed directory exists.
            if (!Directory.Exists(dir))
            {
                throw new Exception("Directory doesn't exist. " + dir);
            }

            directory = dir;

            // Create an install job for each zip.
            foreach (string file in Directory.GetFiles(directory).ToList<string>())
            {
                InstallJobs.Add(new InstallJob(file));
            }

            OrderedPackages = SortPackages(InstallJobs);
        }

        public void InstallPackages ()
        {
            foreach (KeyValuePair<int, PackageJob> keyPair in OrderedPackages)
            {
                PackageJob packJob = keyPair.Value;

                packJob.Install();
            }
        }

        private SortedList<int, PackageJob> SortPackages(List<InstallJob> installJobs)
        {
            SortedList<int, PackageJob> orderedPackages = new SortedList<int, PackageJob>();

            List<PackageJob> packages = AggregatePackages(installJobs);

            foreach (PackageJob package in packages)
            {
                AddPackage(package, packages, orderedPackages);
            }

            foreach (KeyValuePair<int, PackageJob> keyPair in orderedPackages)
            {
                int checkIndex = keyPair.Key;
                PackageJob checkPackage = keyPair.Value;

                foreach (PackageDependency packDep in checkPackage.Dependencies)
                {
                    foreach (KeyValuePair<int, PackageJob> depKeyPair in orderedPackages)
                    {
                        int depIndex = depKeyPair.Key;
                        PackageJob depPackage = depKeyPair.Value;

                        if (depPackage.Name.Equals(packDep.Value) && checkIndex > depIndex)
                        {
                            packDep.WillFulfill = true;
                            break;
                        }
                    }
                }
            }

            return orderedPackages;
        }

        private void AddPackage(PackageJob package, List<PackageJob> packages, SortedList<int, PackageJob> orderedPackages)
        {
            // First of all, are we in the list already?
            if (!orderedPackages.ContainsValue(package))
            {
                // No, do we have any dependencies?
                if (package.Dependencies.Count > 0)
                {
                    // Yes, we need to add those first.
                    foreach (PackageDependency dependency in package.Dependencies)
                    {
                        // Only evaluate package dependencies.
                        if (dependency.Type.Equals("package"))
                        {
                            // Find it in packages.
                            PackageJob depPack = packages.Find(x => x.Name == dependency.Value);

                            // Did we find it?
                            if (depPack != null)
                            {
                                AddPackage(depPack, packages, orderedPackages);
                            }
                        }
                    }
                }

                // Add to the list.
                orderedPackages.Add(orderedPackages.Count, package);
            }
        }

        private List<PackageJob> AggregatePackages(List<InstallJob> installJobs)
        {
            List<PackageJob> packages = new List<PackageJob>();

            foreach(InstallJob installJob in installJobs)
            {
                packages.AddRange(installJob.Packages);
            }

            return packages;
        }
    }
}
