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
        public string ZipPath { get; set; }
        public string TemporaryDirectory { get; set; }
        public string ManifestFilename { get; set; }
        public List<PackageJob> Packages { get; set; }

        public Installer Installer { get; set; }

        public InstallJob(string path)
        {
            ZipPath = path;
            TemporaryDirectory = Utilities.AvailableDirectory(Path.GetDirectoryName(path));
            Packages = new List<PackageJob>();

            // Create temporary directory.
            Directory.CreateDirectory(TemporaryDirectory);

            // Unzip module zip in to the temporary directory.
            ZipFile.ExtractToDirectory(ZipPath, TemporaryDirectory);

            try
            {
                // Find the manifest.
                ManifestFilename = ModuleManifestName(TemporaryDirectory);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Unable to retrieve manifest for {0}", Path.GetFileName(ZipPath)), ex);
            }

            // Grab server info for physical path.
            ServerInfo servInfo = new ServerInfo();

            // Create an installer. This call will also delete the temporary folder.
            Installer = new Installer(new FileStream(path, FileMode.Open, FileAccess.Read), Globals.ApplicationMapPath, true, false);

            //Installer = new Installer(TemporaryDirectory, ManifestFilename, Globals.ApplicationMapPath, true);

            Installer.InstallerInfo.PortalID = 0;

            foreach (KeyValuePair<int, PackageInstaller> orderedPackage in Installer.Packages)
            {
                Packages.Add(new PackageJob(orderedPackage.Value));
            }
        }

        private string ModuleManifestName(string directory)
        {
            string manifestFileName = null;

            foreach (string filePath in Directory.GetFiles(this.TemporaryDirectory))
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
