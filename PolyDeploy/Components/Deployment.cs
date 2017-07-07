using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Cantarus.Modules.PolyDeploy.Components
{
    internal class Deployment
    {
        protected string IntakePath
        {
            get
            {
                return Path.Combine(WorkingPath, "intake");
            }
        }

        protected string ModulesPath
        {
            get
            {
                return Path.Combine(WorkingPath, "modules");
            }
        }

        protected string TempPath
        {
            get
            {
                return Path.Combine(WorkingPath, "temp");
            }
        }

        protected string WorkingPath { get; set; }

        protected List<string> PackageZips { get; set; }

        public Deployment()
        {
            // Generate a temporary directory.
            WorkingPath = AvailableDirectory();

            // Create working directory if it doesn't exist.
            CreateDirectoryIfNotExist(WorkingPath);

            // Create the intake directory if it doesn't exist.
            CreateDirectoryIfNotExist(IntakePath);

            // Create the modules directory if it doesn't exist.
            CreateDirectoryIfNotExist(ModulesPath);

            // Create the temporary directory if it doesn't exist.
            CreateDirectoryIfNotExist(TempPath);
        }

        public List<string> IdentifyPackages()
        {
            return IdentifyPackagesInDirectory(IntakePath);
        }

        protected List<string> IdentifyPackagesInDirectory(string directoryPath)
        {
            List<string> packages = new List<string>();

            // Loop each file in directory.
            foreach (string testPath in Directory.GetFiles(directoryPath))
            {
                // Is it a zip file?
                if (Path.GetExtension(testPath).ToLower().Equals(".zip") && !Path.GetFileNameWithoutExtension(testPath).ToLower().Equals("resources"))
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
                            string tempPath = AvailableDirectory(TempPath);

                            CreateDirectoryIfNotExist(tempPath);

                            ZipFile.ExtractToDirectory(testPath, tempPath);

                            packages.AddRange(IdentifyPackagesInDirectory(tempPath));
                        }
                    }
                }
            }

            return packages;
        }

        protected bool ZipHasDnnManifest(string filePath)
        {
            return ZipHasFileWithExtension(filePath, ".dnn");
        }

        protected bool ZipHasOtherZip(string filePath)
        {
            return ZipHasFileWithExtension(filePath, ".zip");
        }

        protected bool ZipHasFileWithExtension(string filePath, string extension)
        {
            bool hasFile = false;

            try
            {
                using (FileStream fs = File.OpenRead(filePath))
                {
                    ZipArchive archive = new ZipArchive(fs);

                    // Loop entries in archive.
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (Path.GetExtension(entry.Name).ToLower().Equals(extension))
                        {
                            hasFile = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Corrupt zip?
                return false;
            }

            return hasFile;
        }

        protected void CreateDirectoryIfNotExist(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        protected string AvailableDirectory(string basePath = null)
        {
            // Need to set sensible base?
            if (basePath == null)
            {
                basePath = Path.GetTempPath();
            }

            // Generate a random folder in the desired path.
            string dir = Path.Combine(basePath, "tmp-" + RandomName());

            // Does it already exist? I doubt it.
            if (Directory.Exists(dir))
            {
                // My mistake, try again!
                return AvailableDirectory();
            }

            return dir;
        }

        protected string RandomName()
        {
            return Guid.NewGuid().ToString().ToUpper();
        }
    }
}
