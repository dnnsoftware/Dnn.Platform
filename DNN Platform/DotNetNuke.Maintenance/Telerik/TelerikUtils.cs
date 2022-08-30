﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;

    /// <inheritdoc />
    public class TelerikUtils : ITelerikUtils
    {
        /// <summary>
        /// The file name of the Telerik Web UI assembly.
        /// </summary>
        public static readonly string TelerikWebUIFileName = "Telerik.Web.UI.dll";

        private static readonly string[] Vendors = new[]
        {
            "Microsoft",
            "System",
        };

        private static readonly string[] WellKnownAssemblies = new[]
        {
            "Telerik.Web.UI.Skins.dll",
            "DotNetNuke.Website.Deprecated.dll",
            "DotNetNuke.Web.Deprecated.dll",
            "DotNetNuke.Modules.DigitalAssets.dll",
        };

        private readonly IApplicationStatusInfo applicationStatusInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="TelerikUtils"/> class.
        /// </summary>
        /// <param name="applicationStatusInfo">
        /// An instance of <see cref="IApplicationStatusInfo"/>.
        /// </param>
        public TelerikUtils(IApplicationStatusInfo applicationStatusInfo)
        {
            this.applicationStatusInfo = applicationStatusInfo
                ?? throw new ArgumentNullException(nameof(applicationStatusInfo));
        }

        /// <inheritdoc />
        public string BinPath => Path.Combine(this.applicationStatusInfo.ApplicationMapPath, "bin");

        /// <inheritdoc/>
        public IEnumerable<string> GetAssembliesThatDependOnTelerik()
        {
            // use a temp AppDomain to avoid locking files in the bin folder
            var domain = AppDomain.CreateDomain(nameof(TelerikUtils));
            try
            {
                return this.DirectoryGetFiles(this.BinPath, "*.dll", SearchOption.AllDirectories)
                    .Where(this.IsNotWellKnownAssembly)
                    .Where(path => this.AssemblyDependsOnTelerik(path, domain))
                    .ToList();
            }
            finally
            {
                AppDomain.Unload(domain);
            }
        }

        /// <inheritdoc />
        public bool TelerikIsInstalled()
        {
            return this.FileExists(Path.Combine(this.BinPath, TelerikWebUIFileName));
        }

        /// <inheritdoc />
        public Version GetTelerikVersion()
        {
            var domain = AppDomain.CreateDomain(nameof(TelerikUtils));
            try
            {
                var path = Path.Combine(this.BinPath, TelerikWebUIFileName);
                return domain.Load(File.ReadAllBytes(path)).GetName().Version;
            }
            catch (Exception ex)
            {
                throw new IOException("Could not get Telerik version number.", ex);
            }
            finally
            {
                AppDomain.Unload(domain);
            }
        }

        private bool AssemblyDependsOnTelerik(string path, AppDomain domain)
        {
            return this.GetReferencedAssemblyNames(path, domain)
                .Any(assemblyName => assemblyName.StartsWith("Telerik", StringComparison.OrdinalIgnoreCase));
        }

        private string[] DirectoryGetFiles(string path, string searchPattern, SearchOption searchOption) =>
            Directory.GetFiles(path, searchPattern, searchOption);

        private bool FileExists(string path) => File.Exists(path);

        private IEnumerable<string> GetReferencedAssemblyNames(string assemblyFilePath, AppDomain domain)
        {
            try
            {
                return domain.Load(File.ReadAllBytes(assemblyFilePath))
                    .GetReferencedAssemblies()
                    .Select(assembly => assembly.FullName);
            }
            catch (Exception ex)
            {
                throw new IOException($"Could not load assembly '{assemblyFilePath}'", ex);
            }
        }

        private bool IsNotWellKnownAssembly(string path)
        {
            var fileName = Path.GetFileName(path);
            return !WellKnownAssemblies.Contains(fileName) &&
                !Vendors.Any(vendor => fileName.StartsWith($"{vendor}."));
        }
    }
}
