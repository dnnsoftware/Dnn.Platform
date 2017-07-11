using DotNetNuke.Common;
using DotNetNuke.Modules.Dashboard.Components.Server;
using DotNetNuke.Services.Installer;
using DotNetNuke.Services.Installer.Installers;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Cantarus.Modules.PolyDeploy.Components
{
    internal class InstallJob
    {
        public List<PackageJob> Packages { get; set; }

        public bool CanInstall
        {
            get
            {
                foreach (PackageJob package in Packages)
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

        public InstallJob(string path)
        {
            Packages = new List<PackageJob>();
            Installer = new Installer(new FileStream(path, FileMode.Open, FileAccess.Read), Globals.ApplicationMapPath, true, false);

            Installer.InstallerInfo.PortalID = 0;

            foreach (KeyValuePair<int, PackageInstaller> orderedPackage in Installer.Packages)
            {
                Packages.Add(new PackageJob(orderedPackage.Value));
            }
        }

        public void CheckDependencies(List<PackageJob> packageJobs)
        {
            foreach (PackageJob package in Packages)
            {
                foreach (PackageDependency packageDependency in package.Dependencies)
                {
                    if (packageDependency.Type.Equals("package"))
                    {
                        if (FindDependency(packageDependency.Value, packageJobs))
                        {
                            packageDependency.DeployMet = true;
                        }
                    }
                }
            }
        }

        private bool FindDependency(string name, List<PackageJob> packageJobs)
        {
            foreach (PackageJob pj in packageJobs)
            {
                if (pj.Name.ToLower().Equals(name.ToLower()))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
