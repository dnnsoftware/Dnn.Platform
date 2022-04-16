// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Installers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.XPath;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Utilities;
    using DotNetNuke.Common;
    using DotNetNuke.Instrumentation;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileSystemGlobbing;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The CleanupInstaller cleans up (removes) files from previous versions.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class CleanupInstaller : FileInstaller
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(CleanupInstaller));

        private readonly IList<string> folders = new List<string>();
        private readonly IApplicationStatusInfo applicationStatusInfo;
        private readonly IFileSystemUtils fileSystemUtils;

        private string _fileName;
        private string _glob;

        /// <summary>
        /// Initializes a new instance of the <see cref="CleanupInstaller"/> class.
        /// </summary>
        public CleanupInstaller()
            : this(
                Globals.DependencyProvider.GetRequiredService<IApplicationStatusInfo>(),
                Globals.DependencyProvider.GetRequiredService<IFileSystemUtils>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CleanupInstaller"/> class.
        /// </summary>
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a list of allowable file extensions (in addition to the Host's List).
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public override string AllowableFiles => "*";

        /// <summary>
        /// Gets the list of folders to clean up.
        /// </summary>
        protected IList<string> Folders => this.folders;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Commit method finalises the Install and commits any pending changes.
        /// </summary>
        /// <remarks>In the case of Clenup this is not neccessary.</remarks>
        /// -----------------------------------------------------------------------------
        public override void Commit()
        {
            // Do nothing
            base.Commit();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Install method cleansup the files.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void Install()
        {
            try
            {
                bool bSuccess = true;
                if (string.IsNullOrEmpty(this._fileName) && string.IsNullOrEmpty(this._glob)) // No attribute: use the xml files definition.
                {
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
                else if (!string.IsNullOrEmpty(this._fileName)) // Cleanup file provided: clean each file in the cleanup text file line one by one.
                {
                    bSuccess = this.ProcessCleanupFile();
                }
                else if (!string.IsNullOrEmpty(this._glob)) // A globbing pattern was provided, use it to find the files and delete what matches.
                {
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
            this._fileName = Util.ReadAttribute(manifestNav, "fileName");
            this._glob = Util.ReadAttribute(manifestNav, "glob");

            foreach (XPathNavigator nav in manifestNav.Select("folder"))
            {
                this.ProcessFolder(nav);
            }

            base.ReadManifest(manifestNav);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The UnInstall method uninstalls the file component.
        /// </summary>
        /// <remarks>There is no uninstall for this component.</remarks>
        /// -----------------------------------------------------------------------------
        public override void UnInstall()
        {
        }

        /// <summary>
        /// Adds a folder path to the list.
        /// </summary>
        /// <param name="path">The folder path.</param>
        internal void ProcessFolder(string path)
        {
            this.Folders.Add(path);
        }

        /// <summary>
        /// Validates a folder path for cleanup.
        /// </summary>
        /// <param name="path">The folder oath to validate.</param>
        /// <returns>Whether or not the folder path is valid.</returns>
        internal bool IsValidFolderPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            string normalized;
            try
            {
                normalized = Path.GetFullPath(path);
            }
            catch
            {
                return false;
            }

            var appPath = this.applicationStatusInfo.ApplicationMapPath;
            var fullPath = Path.Combine(appPath, normalized);
            if (!normalized.StartsWith(appPath, StringComparison.InvariantCultureIgnoreCase) || fullPath == appPath)
            {
                return false;
            }

            return true;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The CleanupFile method cleansup a single file.
        /// </summary>
        /// <param name="insFile">The InstallFile to clean up.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
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

        /// <summary>
        /// Deletes a folder and all subfolders from disk.
        /// </summary>
        /// <param name="path">The folder path to delete.</param>
        protected virtual void CleanupFolder(string path)
        {
            try
            {
                if (this.IsValidFolderPath(path))
                {
                    this.fileSystemUtils.DeleteFolderRecursive(path);
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The ProcessFile method determines what to do with parsed "file" node.
        /// </summary>
        /// <param name="file">The file represented by the node.</param>
        /// <param name="nav">The XPathNavigator representing the node.</param>
        /// -----------------------------------------------------------------------------
        protected override void ProcessFile(InstallFile file, XPathNavigator nav)
        {
            if (file != null)
            {
                this.Files.Add(file);
            }
        }

        /// <summary>
        /// Determines what to do with the parsed "folder" node.
        /// </summary>
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The RollbackFile method rolls back the cleanup of a single file.
        /// </summary>
        /// <param name="installFile">The InstallFile to commit.</param>
        /// -----------------------------------------------------------------------------
        protected override void RollbackFile(InstallFile installFile)
        {
            if (File.Exists(installFile.BackupFileName))
            {
                Util.RestoreFile(installFile, this.PhysicalBasePath, this.Log);
            }
        }

        private bool ProcessCleanupFile()
        {
            this.Log.AddInfo(string.Format(Util.CLEANUP_Processing, this.Version.ToString(3)));
            try
            {
                var strListFile = Path.Combine(this.Package.InstallerInfo.TempInstallFolder, this._fileName);
                if (File.Exists(strListFile))
                {
                    this.fileSystemUtils.DeleteFiles(File.ReadAllLines(strListFile));
                }
            }
            catch (Exception ex)
            {
                this.Log.AddWarning(string.Format(Util.CLEANUP_ProcessError, ex.Message));

                // DNN-9202: MUST NOT fail installation when cleanup files deletion fails
                // return false;
            }

            this.Log.AddInfo(string.Format(Util.CLEANUP_ProcessComplete, this.Version.ToString(3)));
            return true;
        }

        private bool ProcessGlob()
        {
            this.Log.AddInfo(string.Format(Util.CLEANUP_Processing, this.Version.ToString(3)));
            try
            {
                if (this._glob.Contains(".."))
                {
                    this.Log.AddWarning(Util.EXCEPTION + " - " + Util.EXCEPTION_GlobDotDotNotSupportedInCleanup);
                }
                else
                {
                    var globs = new Matcher(StringComparison.InvariantCultureIgnoreCase);
                    globs.AddIncludePatterns(this._glob.Split(';'));
                    var files = globs.GetResultsInFullPath(this.applicationStatusInfo.ApplicationMapPath).ToArray();
                    this.fileSystemUtils.DeleteFiles(files);
                }
            }
            catch (Exception ex)
            {
                this.Log.AddWarning(string.Format(Util.CLEANUP_ProcessError, ex.Message));
            }

            this.Log.AddInfo(string.Format(Util.CLEANUP_ProcessComplete, this.Version.ToString(3)));
            return true;
        }
    }
}
