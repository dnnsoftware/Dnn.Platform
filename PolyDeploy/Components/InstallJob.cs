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
        public string zipPath { get; set; }
        public string temporaryDirectory { get; set; }
        public string manifestFileName { get; set; }
        public List<PackageJob> Packages { get; set; }

        public InstallJob(string path)
        {
            zipPath = path;
            temporaryDirectory = AvailableTemporaryDirectory();
            Packages = new List<PackageJob>();

            // Create temporary directory.
            Directory.CreateDirectory(temporaryDirectory);

            // Unzip module zip in to the temporary directory.
            ZipFile.ExtractToDirectory(zipPath, temporaryDirectory);

            try
            {
                // Find the manifest.
                manifestFileName = ModuleManifestName(temporaryDirectory);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Unable to retrieve manifest for {0}", Path.GetFileName(zipPath)), ex);
            }

            // Grab server info for physical path.
            ServerInfo servInfo = new ServerInfo();

            // Create an installer. This call will also delete the temporary folder.
            Installer jobInstaller = new Installer(this.temporaryDirectory, manifestFileName, servInfo.PhysicalPath, true);

            foreach (KeyValuePair<int, PackageInstaller> orderedPackage in jobInstaller.Packages)
            {
                Packages.Add(new PackageJob(orderedPackage.Value));
            }
        }

        private string ModuleManifestName(string directory)
        {
            string manifestFileName = null;

            foreach (string filePath in Directory.GetFiles(this.temporaryDirectory))
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

        private string AvailableTemporaryDirectory()
        {
            string dir = Path.Combine(Path.GetTempPath(), "tmp-" + Guid.NewGuid().ToString().ToUpper());

            if (Directory.Exists(dir))
            {
                return AvailableTemporaryDirectory();
            }

            return dir;
        }
    }
}
