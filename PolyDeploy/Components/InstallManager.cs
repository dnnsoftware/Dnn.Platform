using DotNetNuke.Modules.Dashboard.Components.Server;
using DotNetNuke.Services.Installer;
using DotNetNuke.Services.Installer.Packages;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Cantarus.Modules.PolyDeploy.Components
{
    internal class InstallManager
    {
        public IList<InstallJob> InstallJobs { get; set; }

        private string directory { get; set; }

        public InstallManager(string dir)
        {
            if (!Directory.Exists(dir))
            {
                throw new Exception("Directory doesn't exist. " + dir);
            }

            directory = dir;

            BuildGraph();
        }

        private void BuildGraph()
        {
            // Clear.
            InstallJobs = new List<InstallJob>();

            foreach(string file in Directory.GetFiles(directory).ToList<string>())
            {
                InstallJobs.Add(new InstallJob(file));
            }
        }
    }

    internal class ModuleIdentity
    {
        public string moduleName { get; set; }
        public string version { get; set; }
    }

    internal class InstallJob : ModuleIdentity
    {
        public IList<ModuleIdentity> dependencies { get; set; }
        private string zipPath { get; set; }
        public string temporaryDirectory { get; set; }
        public string manifestFileName { get; set; }

        public InstallJob(string path)
        {
            zipPath = path;
            temporaryDirectory = AvailableTemporaryDirectory();

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

            ServerInfo servInfo = new ServerInfo();

            Installer jobInstaller = new Installer(this.temporaryDirectory, manifestFileName, servInfo.PhysicalPath, true);

            PackageInfo package = jobInstaller.Packages[0].Package;

            moduleName = package.Name;
            Version ver = package.Version;

            version = ver.ToString();
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

            if (Directory.Exists(dir)) {
                return AvailableTemporaryDirectory();
            }

            return dir;
        }
    }
}
