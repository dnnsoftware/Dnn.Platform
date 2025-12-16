// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Installers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Xml.XPath;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Instrumentation;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileSystemGlobbing;

    /// <summary>The CleanupInstaller cleans up (removes) files from previous versions.</summary>
    public class CleanupInstaller : FileInstaller
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(CleanupInstaller));

        private readonly IList<string> folders = new List<string>();
        private readonly IApplicationStatusInfo applicationStatusInfo;
        private readonly IFileSystemUtils fileSystemUtils;

        private string fileName;
        private string glob;

        /// <summary>Initializes a new instance of the <see cref="CleanupInstaller"/> class.</summary>
        public CleanupInstaller()
            : this(
                Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IFileSystemUtils>())
        {
        }

        /// <summary>Initializes a new instance of the <see cref="CleanupInstaller"/> class.</summary>
        /// <param name="applicationStatusInfo">An instance of <see cref="IApplicationStatusInfo"/>.</param>
        /// <param name="fileSystemUtils">An instance of <see cref="IFileSystemUtils"/>.</param>
        internal CleanupInstaller(
            IApplicationStatusInfo applicationStatusInfo,
            IFileSystemUtils fileSystemUtils)
        {
            this.applicationStatusInfo = applicationStatusInfo
                ?? throw new ArgumentNullException(nameof(applicationStatusInfo));

            this.fileSystemUtils = fileSystemUtils
                ?? throw new ArgumentNullException(nameof(fileSystemUtils));
        }

        /// <inheritdoc />
        public override string AllowableFiles => "*";

        /// <summary>Gets the list of folders to clean up.</summary>
        protected IList<string> Folders => this.folders;

        /// <remarks>In the case of Clenup this is not neccessary.</remarks>
        /// <inheritdoc />
        public override void Commit()
        {
            // Do nothing
            base.Commit();
        }

        /// <remarks>The Install method cleans up the files.</remarks>
        /// <inheritdoc />
        public override void Install()
        {
            try
            {
                bool bSuccess = true;
                if (string.IsNullOrEmpty(this.fileName) && string.IsNullOrEmpty(this.glob))
                {
                    // No attribute: use the xml files definition.
                    foreach (InstallFile file in this.Files)
                    {
                        bSuccess = this.CleanupFile(file);
                        if (!bSuccess)
                        {
                            break;
                        }
                    }

                    foreach (string folder in this.Folders)
                    {
                        this.CleanupFolder(folder);
                    }
                }
                else if (!string.IsNullOrEmpty(this.fileName))
                {
                    // Cleanup file provided: clean each file in the cleanup text file line one by one.
                    bSuccess = this.ProcessCleanupFile();
                }
                else if (!string.IsNullOrEmpty(this.glob))
                {
                    // A globbing pattern was provided, use it to find the files and delete what matches.
                    bSuccess = this.ProcessGlob();
                }

                this.Completed = bSuccess;
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(Util.EXCEPTION + " - " + ex.Message);
            }
        }

        /// <inheritdoc/>
        public override void ReadManifest(XPathNavigator manifestNav)
        {
            this.fileName = Util.ReadAttribute(manifestNav, "fileName");
            this.glob = Util.ReadAttribute(manifestNav, "glob");

            foreach (XPathNavigator nav in manifestNav.Select("folder"))
            {
                this.ProcessFolder(nav);
            }

            base.ReadManifest(manifestNav);
        }

        /// <remarks>There is no uninstall for this component.</remarks>
        /// <inheritdoc />
        public override void UnInstall()
        {
        }

        /// <summary>Adds a folder path to the list.</summary>
        /// <param name="path">The folder path.</param>
        internal void ProcessFolder(string path)
        {
            this.Folders.Add(path);
        }

        /// <summary>Validates a folder path for cleanup.</summary>
        /// <param name="path">The folder path to validate.</param>
        /// <param name="validPath">The sanitized absolute folder path after validation.</param>
        /// <returns>Whether or not the folder path is valid.</returns>
        internal bool IsValidFolderPath(string path, out string validPath)
        {
            validPath = null;

            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            if (Path.IsPathRooted(path))
            {
                return false; // no rooted paths
            }

            if (path.StartsWith(Path.DirectorySeparatorChar.ToString()))
            {
                return false; // no absolute paths
            }

            if (path.IndexOf("..", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                return false; // no relative paths outside the app root
            }

            path = path.Trim();

            // normalize slashes
            var appPath = Path.GetFullPath(this.applicationStatusInfo.ApplicationMapPath);

            // ensure trailing slash
            appPath = appPath.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

            try
            {
                validPath = Path.GetFullPath(Path.Combine(appPath, path));
            }
            catch
            {
                return false; // no malformed paths
            }

            validPath = validPath.TrimEnd(Path.DirectorySeparatorChar);

            if (!validPath.StartsWith(appPath, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(validPath, appPath, StringComparison.OrdinalIgnoreCase))
            {
                validPath = null;
                return false; // not the app root or paths outside the app root
            }

            return true;
        }

        /// <summary>The CleanupFile method cleans up a single file.</summary>
        /// <param name="insFile">The InstallFile to clean up.</param>
        /// <returns><see langword="true"/> if the file was deleted, otherwise <see langword="false"/>.</returns>
        protected bool CleanupFile(InstallFile insFile)
        {
            try
            {
                // Backup File
                if (File.Exists(this.PhysicalBasePath + insFile.FullName))
                {
                    Util.BackupFile(insFile, this.PhysicalBasePath, this.Log);
                }

                // Delete file
                Util.DeleteFile(insFile, this.PhysicalBasePath, this.Log);
                return true;
            }
            catch (Exception exc)
            {
                Logger.Error(exc);

                return false;
            }
        }

        /// <summary>Deletes all empty folders beneath a given root folder and the root folder itself as well if empty.</summary>
        /// <param name="path">The root folder path.</param>
        protected virtual void CleanupFolder(string path)
        {
            try
            {
                if (this.IsValidFolderPath(path, out string validPath))
                {
                    this.fileSystemUtils.DeleteEmptyFoldersRecursive(validPath);
                }
                else
                {
                    Logger.Warn($"Ignoring invalid cleanup folder path '{path}' in package '{this.Package?.Name}'.");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        /// <summary>The ProcessFile method determines what to do with parsed "file" node.</summary>
        /// <param name="file">The file represented by the node.</param>
        /// <param name="nav">The XPathNavigator representing the node.</param>
        protected override void ProcessFile(InstallFile file, XPathNavigator nav)
        {
            if (file != null)
            {
                this.Files.Add(file);
            }
        }

        /// <summary>Determines what to do with the parsed "folder" node.</summary>
        /// <param name="nav">The XPathNavigator representing the node.</param>
        protected virtual void ProcessFolder(XPathNavigator nav)
        {
            var pathNav = nav.SelectSingleNode("path");

            if (pathNav != null)
            {
                this.ProcessFolder(pathNav.Value);
            }
        }

        /// <inheritdoc/>
        protected override InstallFile ReadManifestItem(XPathNavigator nav, bool checkFileExists)
        {
            return base.ReadManifestItem(nav, false);
        }

        /// <inheritdoc />
        protected override void RollbackFile(InstallFile installFile)
        {
            if (File.Exists(installFile.BackupFileName))
            {
                Util.RestoreFile(installFile, this.PhysicalBasePath, this.Log);
            }
        }

        private bool ProcessCleanupFile()
        {
            this.Log.AddInfo(string.Format(CultureInfo.InvariantCulture, Util.CLEANUP_Processing, this.Version.ToString(3)));
            try
            {
                var strListFile = Path.Combine(this.Package.InstallerInfo.TempInstallFolder, this.fileName);
                if (File.Exists(strListFile))
                {
                    FileSystemUtils.DeleteFiles(File.ReadAllLines(strListFile));
                }
            }
            catch (Exception ex)
            {
                this.Log.AddWarning(string.Format(CultureInfo.InvariantCulture, Util.CLEANUP_ProcessError, ex.Message));

                // DNN-9202: MUST NOT fail installation when cleanup files deletion fails
                // return false;
            }

            this.Log.AddInfo(string.Format(CultureInfo.InvariantCulture, Util.CLEANUP_ProcessComplete, this.Version.ToString(3)));
            return true;
        }

        private bool ProcessGlob()
        {
            this.Log.AddInfo(string.Format(CultureInfo.InvariantCulture, Util.CLEANUP_Processing, this.Version.ToString(3)));
            try
            {
                if (this.glob.Contains(".."))
                {
                    this.Log.AddWarning(Util.EXCEPTION + " - " + Util.EXCEPTION_GlobDotDotNotSupportedInCleanup);
                }
                else
                {
                    var globs = new Matcher(StringComparison.InvariantCultureIgnoreCase);
                    globs.AddIncludePatterns(this.glob.Split(';'));
                    var files = globs.GetResultsInFullPath(Globals.ApplicationMapPath).ToArray();
                    FileSystemUtils.DeleteFiles(files);
                }
            }
            catch (Exception ex)
            {
                this.Log.AddWarning(string.Format(CultureInfo.InvariantCulture, Util.CLEANUP_ProcessError, ex.Message));
            }

            this.Log.AddInfo(string.Format(CultureInfo.InvariantCulture, Util.CLEANUP_ProcessComplete, this.Version.ToString(3)));
            return true;
        }
    }
}
