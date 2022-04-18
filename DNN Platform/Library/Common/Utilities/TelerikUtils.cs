// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Internal
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using Microsoft.Extensions.DependencyInjection;

    /// <inheritdoc />
    public class TelerikUtils : ITelerikUtils
    {
        /// <summary>
        /// The file name of the Telerik Web UI assembly.
        /// </summary>
        public static readonly string TelerikWebUIFileName = "Telerik.Web.UI.dll";

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
        public TelerikUtils()
            : this(Globals.DependencyProvider.GetRequiredService<IApplicationStatusInfo>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TelerikUtils"/> class.
        /// </summary>
        /// <param name="applicationStatusInfo">
        /// An instance of <see cref="IApplicationStatusInfo"/>.
        /// </param>
        internal TelerikUtils(IApplicationStatusInfo applicationStatusInfo)
        {
            this.applicationStatusInfo = applicationStatusInfo
                ?? throw new ArgumentNullException(nameof(applicationStatusInfo));
        }

        /// <inheritdoc />
        public string BinPath => Path.Combine(this.applicationStatusInfo.ApplicationMapPath, "bin");

        /// <inheritdoc/>
        public IEnumerable<string> GetAssembliesThatDependOnTelerik()
        {
            return this.DirectoryGetFiles(this.BinPath, "*.dll", SearchOption.AllDirectories)
                .Where(this.IsNotWellKnownAssembly)
                .Where(this.AssemblyDependsOnTelerik);
        }

        /// <inheritdoc />
        public bool TelerikIsInstalled()
        {
            return this.FileExists(Path.Combine(this.BinPath, TelerikWebUIFileName));
        }

        private bool AssemblyDependsOnTelerik(string path)
        {
            return this.GetReferencedAssemblyNames(path)
                .Any(assemblyName => assemblyName.StartsWith("Telerik", StringComparison.OrdinalIgnoreCase));
        }

        private string[] DirectoryGetFiles(string path, string searchPattern, SearchOption searchOption) =>
            Directory.GetFiles(path, searchPattern, searchOption);

        private bool FileExists(string path) => File.Exists(path);

        private IEnumerable<string> GetReferencedAssemblyNames(string assemblyFilePath) =>
            this.LoadAssembly(assemblyFilePath)
                .GetReferencedAssemblies()
                .Select(assembly => assembly.FullName);

        private bool IsNotWellKnownAssembly(string path)
        {
            var fileName = Path.GetFileName(path);
            return !WellKnownAssemblies.Contains(fileName);
        }

        private Assembly LoadAssembly(string assemblyFilePath)
        {
            try
            {
                return Assembly.LoadFile(assemblyFilePath);
            }
            catch (Exception ex)
            {
                throw new IOException($"Could not load assembly '{assemblyFilePath}'", ex);
            }
        }
    }
}
