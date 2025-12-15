// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Instrumentation;

    /// <inheritdoc />
    public class TelerikUtils : ITelerikUtils
    {
        /// <summary>The file name of the Telerik Web UI assembly.</summary>
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
        private readonly ILog log;

        /// <summary>Initializes a new instance of the <see cref="TelerikUtils"/> class.</summary>
        /// <param name="applicationStatusInfo">An instance of <see cref="IApplicationStatusInfo"/>.</param>
        /// <param name="loggerSource">An instance of <see cref="ILoggerSource"/>.</param>
        public TelerikUtils(
            IApplicationStatusInfo applicationStatusInfo,
            ILoggerSource loggerSource)
        {
            this.applicationStatusInfo = applicationStatusInfo
                ?? throw new ArgumentNullException(nameof(applicationStatusInfo));
            this.log = loggerSource.GetLogger(nameof(TelerikUtils));
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
                return DirectoryGetFiles(this.BinPath, "*.dll", SearchOption.AllDirectories)
                    .Where(IsNotWellKnownAssembly)
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
            return FileExists(Path.Combine(this.BinPath, TelerikWebUIFileName));
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

        /// <inheritdoc />
        public bool IsTelerikVersionVulnerable(Version version)
        {
            return version < new Version(2014, 0);
        }

        private static string[] DirectoryGetFiles(string path, string searchPattern, SearchOption searchOption) =>
            Directory.GetFiles(path, searchPattern, searchOption);

        private static bool FileExists(string path) => File.Exists(path);

        private static IEnumerable<string> GetReferencedAssemblyNames(string assemblyFilePath, AppDomain domain)
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

        private static bool IsNotWellKnownAssembly(string path)
        {
            var fileName = Path.GetFileName(path);
            return !WellKnownAssemblies.Contains(fileName) &&
                !Vendors.Any(vendor => fileName.StartsWith($"{vendor}."));
        }

        private bool AssemblyDependsOnTelerik(string path, AppDomain domain)
        {
            try
            {
                return GetReferencedAssemblyNames(path, domain)
                    .Any(assemblyName => assemblyName.StartsWith("Telerik", StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                // If we can't get the referenced assemblies, then it can't depend on Telerik.
                this.log.Warn("Could not determine Telerik dependencies on some assemblies.", ex);
                return false;
            }
        }
    }
}
