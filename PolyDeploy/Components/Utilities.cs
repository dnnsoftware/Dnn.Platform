using System;
using System.IO;

namespace Cantarus.Modules.PolyDeploy.Components
{
    public class Utilities
    {
        public static string AvailableDirectory(string basePath = null)
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

        public static string RandomName()
        {
            return Guid.NewGuid().ToString().ToUpper();
        }
    }
}
