using System;
using System.Linq;
using System.Reflection;

namespace DotNetNuke.Services.Zip
{
    /// <summary>
    /// This class is in charge of fixing an assembly rename which was necessary when DNN
    /// upgraded from an older SharpZipLib. 
    /// </summary>
    /// <remarks>
    /// final solution taken from https://raw.githubusercontent.com/2sic/2sxc/master/2sxc%20Dnn/Dnn920/SharpZipLibRedirect.cs
    /// read the readme.md in this folder for more info
    /// </remarks>
    public class SharpZipLibRedirect
    {
        public static bool AlreadyRun { get; private set; }

        private const string OldName = "SharpZipLib";
        private const string NewName = "ICSharpCode.SharpZipLib";

        /// <summary>
        /// Registration call - should only be called once
        /// Has extra security to prevent it from running multiple times
        /// </summary>
        public static void RegisterSharpZipLibRedirect()
        {
            if (AlreadyRun) return;
            // stop any further attempts to access this
            AlreadyRun = true;

            try
            {
                // Adds an AssemblyResolve handler to redirect all attempts to load a specific assembly name to the specified version.
                AppDomain.CurrentDomain.AssemblyResolve += Handler;
            }
            catch
            {
                /* ignore */
            }
        }

        private static Assembly Handler(object sender, ResolveEventArgs args)
        {
            // only check for access to old SharpZipLib
            if (!args.Name.StartsWith(OldName))
                return null;

            // If the original was not found, it should find the new one - otherwise it will be null
            var alreadyLoadedAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == NewName);
            return alreadyLoadedAssembly;
        }
    }
}
