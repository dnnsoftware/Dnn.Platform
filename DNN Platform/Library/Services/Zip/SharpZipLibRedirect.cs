// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Zip
{
    using System;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// This class contains a fix for a breaking upgrade in DNN 9.2 regarding the ZIP.
    /// It is in charge of fixing an assembly rename which was necessary when DNN
    /// upgraded from an older SharpZipLib with a different DLL name.
    /// </summary>
    /// <remarks>
    /// The class in this folder will be used by.net when an assembly is missing.
    /// It will then check if .net was looking for SharpZipLib, and if necessary,
    /// redirect it to the correct (new) assembly.
    ///
    /// Once this remapping has been completed, this code will not be used again until the next
    /// restart of the DNN application.
    ///
    /// final solution taken from https://raw.githubusercontent.com/2sic/2sxc/master/2sxc%20Dnn/Dnn920/SharpZipLibRedirect.cs.
    /// </remarks>
    internal class SharpZipLibRedirect
    {
        private const string OldName = "SharpZipLib";
        private const string NewName = "ICSharpCode.SharpZipLib";

        internal static bool AlreadyRun { get; private set; }

        /// <summary>
        /// Registration call - should only be called once
        /// Has extra security to prevent it from running multiple times.
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
