// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Components.Checks
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Dnn.PersonaBar.Extensions.Components.Security.Helper;
    using DotNetNuke.Abstractions.Application;

    /// <summary>
    /// Check for Telerik presence in the site.
    /// Passes if Telerik is not installed.
    /// Fails if Telerik is installed.
    /// If installed and used, it warns about 10.x upgrade issue.
    /// If installed and not used, it provides information about removal steps.
    /// </summary>
    public class CheckTelerikPresence : BaseCheck
    {
        /// <summary>
        /// The file name of the Telerik Web UI assembly.
        /// </summary>
        public static readonly string TelerikWebUIFileName = "Telerik.Web.UI.dll";

        private readonly IApplicationStatusInfo applicationStatusInfo;
        private readonly IFileHelper fileHelper;

        private readonly string[] wellKnownAssemblies = new[]
        {
            "Telerik.Web.UI.Skins.dll",
            "DotNetNuke.Website.Deprecated.dll",
            "DotNetNuke.Web.Deprecated.dll",
            "DotNetNuke.Modules.DigitalAssets.dll",
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckTelerikPresence"/> class.
        /// </summary>
        /// <param name="applicationStatusInfo">
        /// An instance of the <see cref="IApplicationStatusInfo"/> class.
        /// </param>
        public CheckTelerikPresence(IApplicationStatusInfo applicationStatusInfo)
            : this(applicationStatusInfo, new FileHelper())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckTelerikPresence"/> class.
        /// </summary>
        /// <param name="applicationStatusInfo">
        /// An instance of the <see cref="IApplicationStatusInfo"/> class.
        /// </param>
        /// <param name="fileHelper">
        /// An instance of the <see cref="IFileHelper"/> class.
        /// </param>
        internal CheckTelerikPresence(IApplicationStatusInfo applicationStatusInfo, IFileHelper fileHelper)
            : base()
        {
            this.applicationStatusInfo = applicationStatusInfo ??
                throw new ArgumentNullException(nameof(applicationStatusInfo));

            this.fileHelper = fileHelper ??
                throw new ArgumentNullException(nameof(fileHelper));
        }

        private string BinPath => Path.Combine(this.applicationStatusInfo.ApplicationMapPath, "bin");

        /// <inheritdoc />
        protected override CheckResult ExecuteInternal()
        {
            if (this.TelerikIsInstalled())
            {
                var files = this.GetAssembliesThatDependOnTelerik();
                var fileList = files as IList<string> ?? files.ToList();

                if (fileList.Any())
                {
                    return this.InstalledAndUsed(fileList);
                }

                return this.InstalledButNotUsed();
            }

            return this.NotInstalled();
        }

        private CheckResult InstalledButNotUsed()
        {
            var note = this.GetLocalizedString("InstalledButNotUsed");

            return new CheckResult(SeverityEnum.Failure, this.Id)
            {
                Notes = { note },
            };
        }

        private CheckResult InstalledAndUsed(IEnumerable<string> files)
        {
            var caption = this.GetLocalizedString("InstalledAndUsed");
            var relativeFiles = files.Select(path => path.Substring(this.BinPath.Length + 1));
            var fileList = string.Join("<br/>", relativeFiles.Select(path => $"* {path}"));
            var note = string.Join("<br/>", new[] { caption, string.Empty, fileList });

            return new CheckResult(SeverityEnum.Failure, this.Id)
            {
                Notes = { note },
            };
        }

        private CheckResult NotInstalled()
        {
            return new CheckResult(SeverityEnum.Pass, this.Id);
        }

        private bool TelerikIsInstalled()
        {
            return this.fileHelper.FileExists(Path.Combine(this.BinPath, TelerikWebUIFileName));
        }

        private IEnumerable<string> GetAssembliesThatDependOnTelerik()
        {
            return this.fileHelper.DirectoryGetFiles(this.BinPath, "*.dll", SearchOption.AllDirectories)
                .Where(this.IsNotWellKnownAssembly)
                .Where(this.AssemblyDependsOnTelerik);
        }

        private bool IsNotWellKnownAssembly(string path)
        {
            var fileName = Path.GetFileName(path);
            return !this.wellKnownAssemblies.Contains(fileName);
        }

        private bool AssemblyDependsOnTelerik(string path)
        {
            return this.fileHelper.GetReferencedAssemblyNames(path)
                .Any(assemblyName => assemblyName.StartsWith("Telerik", StringComparison.OrdinalIgnoreCase));
        }
    }
}
