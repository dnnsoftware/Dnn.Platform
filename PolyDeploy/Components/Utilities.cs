using DotNetNuke.Common;
using System;
using System.IO;

namespace Cantarus.Modules.PolyDeploy.Components
{
    internal class Utilities
    {
        public static string ModulePath = Path.Combine(Globals.ApplicationMapPath, "DesktopModules", "Cantarus", "PolyDeploy");

        public static string AvailableTempDirectory(string basePath = null)
        {
            // Need to set sensible base?
            if (basePath == null)
            {
                // We'll create a temporary folder in the module folder.
                basePath = ModulePath;

                // Check we found the module directory.
                if (Directory.Exists(basePath))
                {
                    // Prepare a temporary directory.
                    basePath = Path.Combine(basePath, "Temp");

                    // Does it exist?
                    if (!Directory.Exists(basePath))
                    {
                        // No, create it.
                        Directory.CreateDirectory(basePath);
                    }
                }
                else
                {
                    // No module directory, use windows temp.
                    basePath = Path.GetTempPath();
                }
            }

            // Generate a random folder in the desired path.
            string dir = Path.Combine(basePath, "tmp-" + RandomName());

            // Does it already exist? I doubt it.
            if (Directory.Exists(dir))
            {
                // My mistake, try again!
                return AvailableTempDirectory();
            }

            return dir;
        }

        public static string RandomName()
        {
            // Get new guid as string.
            string guidString = Guid.NewGuid().ToString();

            // Remove hyphens, uppercase and return.
            return guidString.Replace("-", null).ToUpper();
        }
    }
}
