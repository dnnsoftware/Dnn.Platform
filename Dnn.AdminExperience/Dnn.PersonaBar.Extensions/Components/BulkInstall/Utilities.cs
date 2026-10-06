// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Components.BulkInstall
{
    using System;
    using System.IO;

    using DotNetNuke.Abstractions.Application;

    /// <summary>Utility methods.</summary>
    internal static class Utilities
    {
        /// <summary>Gets the BulkInstall data path.</summary>
        /// <param name="appStatus">The application status.</param>
        /// <returns>The absolute/mapped path to the BulkInstall data directory.</returns>
        public static string GetModuleBasePath(IApplicationStatusInfo appStatus) => Path.Combine(appStatus.ApplicationMapPath, "App_Data", "BulkInstall");

        /// <summary>Gets the path to a temp folder.</summary>
        /// <param name="appStatus">The application status.</param>
        /// <param name="basePath">The base path, or <see langword="null"/> to create a temporary folder.</param>
        /// <returns>The absolute/mapped path to a temp directory.</returns>
        public static string AvailableTempDirectory(IApplicationStatusInfo appStatus, string basePath = null)
        {
            if (basePath == null)
            {
                basePath = Path.Combine(GetModuleBasePath(appStatus), "Temp");
                Directory.CreateDirectory(basePath);
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
