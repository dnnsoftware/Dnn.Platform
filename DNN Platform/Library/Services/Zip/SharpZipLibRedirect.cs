using System;
using System.Linq;
using System.Reflection;

// DNN uses SharpZipLib as the zip system of choice.Because of this, it is bundled with DNN.
//
// This folder here contains a fix for a breaking upgrade in DNN 9.2 regarding the ZIP.
// The class in this folder will be used by.net when an assembly is missing.
// It will then check if .net was looking for SharpZipLib, and if necessary,
// redirect it to the correct (new) assembly.
//
// Once this remapping has been completed, this code will not be used again until the next
// restart of the DNN application. 

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
    internal class SharpZipLibRedirect
    {
        internal static bool AlreadyRun { get; private set; }

        private const string OldName = "SharpZipLib";
        private const string NewName = "ICSharpCode.SharpZipLib";

        /// <summary>
        /// Registration call - should only be called once
        /// Has extra security to prevent it from running multiple times
        /// </summary>
        internal static void RegisterSharpZipLibRedirect()
        {
            if (AlreadyRun)
            {
                return;
            }
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
                // this is to ensure that if anything at all goes wrong when registering this, it won't break DNN.
            }
        }

        private static Assembly Handler(object sender, ResolveEventArgs args)
        {
            // only check for access to old SharpZipLib
            // requires startswith, because the full string is something like "SharpZipLib, Version=0.81.0.1407, Culture=neutral, PublicKeyToken=null"
            if (!args.Name.StartsWith(OldName, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            // If the original was not found, it should find the new one - otherwise it will be null
            var alreadyLoadedAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == NewName);
            return alreadyLoadedAssembly;
        }
    }
}
