using DotNetNuke.Common;
using DotNetNuke.Services.Installer;
using DotNetNuke.Services.Installer.Installers;
using DotNetNuke.Services.Installer.Log;
using System;
using System.Collections.Generic;
using System.IO;

namespace Cantarus.Modules.PolyDeploy.Components
{
    internal class InstallJob
    {
        public string Name { get; set; }
        public List<PackageJob> Packages { get; set; }
        public List<string> Failures { get; set; }
        public bool Attempted { get; set; }
        public bool Success { get; set; }

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
            Name = Path.GetFileName(path);
            Packages = new List<PackageJob>();
            Failures = new List<string>();
            Attempted = false;
            Success = false;
            Installer = new Installer(new FileStream(path, FileMode.Open, FileAccess.Read), Globals.ApplicationMapPath, true, false);

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

        public void Install()
        {
            // Record that we have attempted an install.
            Attempted = true;

            // Can this be installed at this point?
            if (CanInstall)
            {
                // Possibly need to recreate the installer at this point.
                Installer = new Installer(Installer.TempInstallFolder, ModuleManifestName(Installer.TempInstallFolder), Globals.ApplicationMapPath, true);

                // Is the installer valid?
                if (Installer.IsValid)
                {
                    // Already installed?
                    if (Installer.InstallerInfo.Installed)
                    {
                        // Yes, make a repair install.
                        Installer.InstallerInfo.RepairInstall = true;
                    }

                    // Install.
                    Installer.Install();

                    // Did the package install successfully?
                    Success = Installer.IsValid;
                }

                // Record failures.
                foreach (LogEntry log in Installer.InstallerInfo.Log.Logs)
                {
                    if (log.Type.Equals(LogType.Failure))
                    {
                        string failure = log.ToString();

                        Failures.Add(failure);
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

        private string ModuleManifestName(string directory)
        {
            string manifestFileName = null;
            foreach (string filePath in Directory.GetFiles(directory))
            {
                if (filePath.EndsWith(".dnn"))
                {
                    if (manifestFileName == null)
                    {
                        manifestFileName = Path.GetFileName(filePath);
                    }
                    else
                    {
                        throw new Exception("More than one manifest found.");
                    }
                }
            }
            return manifestFileName;
        }
    }
}
