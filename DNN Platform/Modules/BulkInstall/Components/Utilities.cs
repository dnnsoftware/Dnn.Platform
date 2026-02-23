// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.BulkInstall.Components
{
    using System;
    using System.IO;

    using DotNetNuke.Abstractions.Application;

    /// <summary>Utility methods.</summary>
    internal static class Utilities
    {
        /// <summary>Gets the module's path.</summary>
        /// <param name="appStatus">The application status.</param>
        /// <returns>The absolute/mapped path to the module directory.</returns>
        public static string GetModulePath(IApplicationStatusInfo appStatus) => Path.Combine(appStatus.ApplicationMapPath, "DesktopModules", "BulkInstall");

        /// <summary>Gets the path to a temp folder.</summary>
        /// <param name="appStatus">The application status.</param>
        /// <param name="basePath">The base path, or <see langword="null"/> to create a temporary folder.</param>
        /// <returns>The absolute/mapped path to a temp directory.</returns>
        public static string AvailableTempDirectory(IApplicationStatusInfo appStatus, string basePath = null)
        {
            // Need to set sensible base?
            if (basePath == null)
            {
                // We'll create a temporary folder in the module folder.
                basePath = GetModulePath(appStatus);

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
                return AvailableTempDirectory(appStatus);
            }

            return dir;
        }

        /// <summary>Gets a random name.</summary>
        /// <returns>The name.</returns>
        public static string RandomName()
        {
            // Get new guid as string.
            string guidString = Guid.NewGuid().ToString();

            // Remove hyphens, uppercase and return.
            return guidString.Replace("-", null).ToUpperInvariant();
        }
    }
}
