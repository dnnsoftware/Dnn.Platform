// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.XPath;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Installer.Installers;
    using DotNetNuke.Services.Installer.Log;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Installer.Writers;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The Installer class provides a single entrypoint for Package Installation.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class Installer
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Installer));
        private Stream _inputStream;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="Installer"/> class.
        /// This Constructor creates a new Installer instance from a string representing
        /// the physical path to the temporary install folder and a string representing
        /// the physical path to the root of the site.
        /// </summary>
        /// <param name="tempFolder">The physical path to the zip file containg the package.</param>
        /// <param name="manifest">The manifest filename.</param>
        /// <param name="physicalSitePath">The physical path to the root of the site.</param>
        /// <param name="loadManifest">Flag that determines whether the manifest will be loaded.</param>
        /// -----------------------------------------------------------------------------
        public Installer(string tempFolder, string manifest, string physicalSitePath, bool loadManifest)
        {
            this.Packages = new SortedList<int, PackageInstaller>();

            // Called from Interactive installer - default IgnoreWhiteList to false
            this.InstallerInfo = new InstallerInfo(tempFolder, manifest, physicalSitePath) { IgnoreWhiteList = false };

            if (loadManifest)
            {
                this.ReadManifest(true);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="Installer"/> class.
        /// This Constructor creates a new Installer instance from a Stream and a
        /// string representing the physical path to the root of the site.
        /// </summary>
        /// <param name="inputStream">The Stream to use to create this InstallerInfo instance.</param>
        /// <param name="physicalSitePath">The physical path to the root of the site.</param>
        /// <param name="loadManifest">Flag that determines whether the manifest will be loaded.</param>
        /// -----------------------------------------------------------------------------
        public Installer(Stream inputStream, string physicalSitePath, bool loadManifest)
            : this(inputStream, physicalSitePath, loadManifest, true)
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="Installer"/> class.
        /// This Constructor creates a new Installer instance from a Stream and a
        /// string representing the physical path to the root of the site.
        /// </summary>
        /// <param name="inputStream">The Stream to use to create this InstallerInfo instance.</param>
        /// <param name="physicalSitePath">The physical path to the root of the site.</param>
        /// <param name="loadManifest">Flag that determines whether the manifest will be loaded.</param>
        /// <param name="deleteTemp">Whether delete the temp folder.</param>
        /// -----------------------------------------------------------------------------
        public Installer(Stream inputStream, string physicalSitePath, bool loadManifest, bool deleteTemp)
        {
            this.Packages = new SortedList<int, PackageInstaller>();

            this._inputStream = new MemoryStream();
            inputStream.CopyTo(this._inputStream);

            // Called from Batch installer - default IgnoreWhiteList to true
            this.InstallerInfo = new InstallerInfo(inputStream, physicalSitePath) { IgnoreWhiteList = true };

            if (loadManifest)
            {
                this.ReadManifest(deleteTemp);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="Installer"/> class.
        /// This Constructor creates a new Installer instance from a PackageInfo object.
        /// </summary>
        /// <param name="package">The PackageInfo instance.</param>
        /// <param name="physicalSitePath">The physical path to the root of the site.</param>
        /// -----------------------------------------------------------------------------
        public Installer(PackageInfo package, string physicalSitePath)
        {
            this.Packages = new SortedList<int, PackageInstaller>();
            this.InstallerInfo = new InstallerInfo(package, physicalSitePath);

            this.Packages.Add(this.Packages.Count, new PackageInstaller(package));
        }

        public Installer(string manifest, string physicalSitePath, bool loadManifest)
        {
            this.Packages = new SortedList<int, PackageInstaller>();
            this.InstallerInfo = new InstallerInfo(physicalSitePath, InstallMode.ManifestOnly);
            if (loadManifest)
            {
                this.ReadManifest(new FileStream(manifest, FileMode.Open, FileAccess.Read));
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether gets whether the associated InstallerInfo is valid.
        /// </summary>
        /// <value>True - if valid, False if not.</value>
        /// -----------------------------------------------------------------------------
        public bool IsValid
        {
            get
            {
                return this.InstallerInfo.IsValid;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets.
        /// </summary>
        /// <value>A Dictionary(Of String, PackageInstaller).</value>
        /// -----------------------------------------------------------------------------
        public string TempInstallFolder
        {
            get
            {
                return this.InstallerInfo.TempInstallFolder;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the associated InstallerInfo object.
        /// </summary>
        /// <value>An InstallerInfo.</value>
        /// -----------------------------------------------------------------------------
        public InstallerInfo InstallerInfo { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a SortedList of Packages that are included in the Package Zip.
        /// </summary>
        /// <value>A SortedList(Of Integer, PackageInstaller).</value>
        /// -----------------------------------------------------------------------------
        public SortedList<int, PackageInstaller> Packages { get; private set; }

        public static XPathNavigator ConvertLegacyNavigator(XPathNavigator rootNav, InstallerInfo info)
        {
            XPathNavigator nav = null;

            var packageType = Null.NullString;
            if (rootNav.Name == "dotnetnuke")
            {
                packageType = Util.ReadAttribute(rootNav, "type");
            }
            else if (rootNav.Name.Equals("languagepack", StringComparison.InvariantCultureIgnoreCase))
            {
                packageType = "LanguagePack";
            }

            XPathDocument legacyDoc;
            string legacyManifest;
            switch (packageType.ToLowerInvariant())
            {
                case "module":
                    var sb = new StringBuilder();
                    using (var writer = XmlWriter.Create(sb, XmlUtils.GetXmlWriterSettings(ConformanceLevel.Fragment)))
                    {
                        // Write manifest start element
                        PackageWriterBase.WriteManifestStartElement(writer);

                        // Legacy Module - Process each folder
                        foreach (XPathNavigator folderNav in rootNav.Select("folders/folder"))
                        {
                            var modulewriter = new ModulePackageWriter(folderNav, info);
                            modulewriter.WriteManifest(writer, true);
                        }

                        // Write manifest end element
                        PackageWriterBase.WriteManifestEndElement(writer);

                        // Close XmlWriter
                        writer.Close();
                    }

                    // Load manifest into XPathDocument for processing
                    legacyDoc = new XPathDocument(new StringReader(sb.ToString()));

                    // Parse the package nodes
                    nav = legacyDoc.CreateNavigator().SelectSingleNode("dotnetnuke");
                    break;
                case "languagepack":
                    // Legacy Language Pack
                    var languageWriter = new LanguagePackWriter(rootNav, info);
                    info.LegacyError = languageWriter.LegacyError;
                    if (string.IsNullOrEmpty(info.LegacyError))
                    {
                        legacyManifest = languageWriter.WriteManifest(false);
                        legacyDoc = new XPathDocument(new StringReader(legacyManifest));

                        // Parse the package nodes
                        nav = legacyDoc.CreateNavigator().SelectSingleNode("dotnetnuke");
                    }

                    break;
                case "skinobject":
                    // Legacy Skin Object
                    var skinControlwriter = new SkinControlPackageWriter(rootNav, info);
                    legacyManifest = skinControlwriter.WriteManifest(false);
                    legacyDoc = new XPathDocument(new StringReader(legacyManifest));

                    // Parse the package nodes
                    nav = legacyDoc.CreateNavigator().SelectSingleNode("dotnetnuke");
                    break;
            }

            return nav;
        }

        public void DeleteTempFolder()
        {
            try
            {
                if (!string.IsNullOrEmpty(this.TempInstallFolder))
                {
                    Directory.Delete(this.TempInstallFolder, true);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Exception deleting folder " + this.TempInstallFolder + " while installing " + this.InstallerInfo.ManifestFile.Name, ex);
                Exceptions.Exceptions.LogException(ex);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Install method installs the feature.
        /// </summary>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public bool Install()
        {
            this.InstallerInfo.Log.StartJob(Util.INSTALL_Start);
            bool bStatus = true;
            try
            {
                bool clearClientCache = false;
                this.InstallPackages(ref clearClientCache);
                if (clearClientCache)
                {
                    // Update the version of the client resources - so the cache is cleared
                    HostController.Instance.IncrementCrmVersion(true);
                }
            }
            catch (Exception ex)
            {
                this.InstallerInfo.Log.AddFailure(ex);
                bStatus = false;
            }
            finally
            {
                // Delete Temp Folder
                if (!string.IsNullOrEmpty(this.TempInstallFolder))
                {
                    Globals.DeleteFolderRecursive(this.TempInstallFolder);
                }

                this.InstallerInfo.Log.AddInfo(Util.FOLDER_DeletedBackup);
            }

            if (this.InstallerInfo.Log.Valid)
            {
                this.InstallerInfo.Log.EndJob(Util.INSTALL_Success);
            }
            else
            {
                this.InstallerInfo.Log.EndJob(Util.INSTALL_Failed);
                bStatus = false;
            }

            // log installation event
            this.LogInstallEvent("Package", "Install");

            // when the installer initialized by file stream, we need save the file stream into backup folder.
            if (this._inputStream != null && bStatus && this.Packages.Any())
            {
                Task.Run(() =>
                {
                    this.BackupStreamIntoFile(this._inputStream, this.Packages[0].Package);
                });
            }

            // Clear Host Cache
            DataCache.ClearHostCache(true);

            if (Config.GetFcnMode() == Config.FcnMode.Disabled.ToString())
            {
                // force application restart after the new changes only when FCN is disabled
                Config.Touch();
            }

            return bStatus;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The ReadManifest method reads the manifest file and parses it into packages.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void ReadManifest(bool deleteTemp)
        {
            this.InstallerInfo.Log.StartJob(Util.DNN_Reading);
            if (this.InstallerInfo.ManifestFile != null)
            {
                this.ReadManifest(new FileStream(this.InstallerInfo.ManifestFile.TempFileName, FileMode.Open, FileAccess.Read));
            }

            if (this.InstallerInfo.Log.Valid)
            {
                this.InstallerInfo.Log.EndJob(Util.DNN_Success);
            }
            else if (deleteTemp)
            {
                // Delete Temp Folder
                this.DeleteTempFolder();
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The UnInstall method uninstalls the feature.
        /// </summary>
        /// <param name="deleteFiles">A flag that indicates whether the files should be
        /// deleted.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public bool UnInstall(bool deleteFiles)
        {
            this.InstallerInfo.Log.StartJob(Util.UNINSTALL_Start);
            try
            {
                this.UnInstallPackages(deleteFiles);
            }
            catch (Exception ex)
            {
                this.InstallerInfo.Log.AddFailure(ex);
                return false;
            }

            if (this.InstallerInfo.Log.HasWarnings)
            {
                this.InstallerInfo.Log.EndJob(Util.UNINSTALL_Warnings);
            }
            else
            {
                this.InstallerInfo.Log.EndJob(Util.UNINSTALL_Success);
            }

            // log installation event
            this.LogInstallEvent("Package", "UnInstall");
            return true;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The InstallPackages method installs the packages.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void InstallPackages(ref bool clearClientCache)
        {
            // Iterate through all the Packages
            for (int index = 0; index <= this.Packages.Count - 1; index++)
            {
                PackageInstaller installer = this.Packages.Values[index];

                // Check if package is valid
                if (installer.Package.IsValid)
                {
                    if (installer.Package.InstallerInfo.PackageID > Null.NullInteger || installer.Package.InstallerInfo.RepairInstall)
                    {
                        clearClientCache = true;
                    }

                    this.InstallerInfo.Log.AddInfo(Util.INSTALL_Start + " - " + installer.Package.Name);
                    installer.Install();
                    if (this.InstallerInfo.Log.Valid)
                    {
                        this.InstallerInfo.Log.AddInfo(Util.INSTALL_Success + " - " + installer.Package.Name);
                    }
                    else
                    {
                        this.InstallerInfo.Log.AddInfo(Util.INSTALL_Failed + " - " + installer.Package.Name);
                    }
                }
                else
                {
                    this.InstallerInfo.Log.AddFailure(Util.INSTALL_Aborted + " - " + installer.Package.Name);
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Logs the Install event to the Event Log.
        /// </summary>
        /// <param name="package">The name of the package.</param>
        /// <param name="eventType">Event Type.</param>
        /// -----------------------------------------------------------------------------
        private void LogInstallEvent(string package, string eventType)
        {
            try
            {
                var log = new LogInfo { LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString() };

                if (this.InstallerInfo.ManifestFile != null)
                {
                    log.LogProperties.Add(new LogDetailInfo(eventType + " " + package + ":", this.InstallerInfo.ManifestFile.Name.Replace(".dnn", string.Empty)));
                }

                foreach (LogEntry objLogEntry in this.InstallerInfo.Log.Logs)
                {
                    log.LogProperties.Add(new LogDetailInfo("Info:", objLogEntry.Description));
                }

                LogController.Instance.AddLog(log);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The ProcessPackages method processes the packages nodes in the manifest.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void ProcessPackages(XPathNavigator rootNav)
        {
            // Parse the package nodes
            foreach (XPathNavigator nav in rootNav.Select("packages/package"))
            {
                int order = this.Packages.Count;
                string name = Util.ReadAttribute(nav, "name");
                string installOrder = Util.ReadAttribute(nav, "installOrder");
                if (!string.IsNullOrEmpty(installOrder))
                {
                    order = int.Parse(installOrder);
                }

                this.Packages.Add(order, new PackageInstaller(nav.OuterXml, this.InstallerInfo));
            }
        }

        private void ReadManifest(Stream stream)
        {
            var doc = new XPathDocument(stream);

            // Read the root node to determine what version the manifest is
            XPathNavigator rootNav = doc.CreateNavigator();
            rootNav.MoveToFirstChild();
            string packageType = Null.NullString;

            if (rootNav.Name == "dotnetnuke")
            {
                packageType = Util.ReadAttribute(rootNav, "type");
            }
            else if (rootNav.Name.Equals("languagepack", StringComparison.InvariantCultureIgnoreCase))
            {
                packageType = "LanguagePack";
            }
            else
            {
                this.InstallerInfo.Log.AddFailure(Util.PACKAGE_UnRecognizable);
            }

            switch (packageType.ToLowerInvariant())
            {
                case "package":
                    this.InstallerInfo.IsLegacyMode = false;

                    // Parse the package nodes
                    this.ProcessPackages(rootNav);
                    break;
                case "module":
                case "languagepack":
                case "skinobject":
                    this.InstallerInfo.IsLegacyMode = true;
                    this.ProcessPackages(ConvertLegacyNavigator(rootNav, this.InstallerInfo));
                    break;
            }
        }

        private void UnInstallPackages(bool deleteFiles)
        {
            for (int index = 0; index <= this.Packages.Count - 1; index++)
            {
                PackageInstaller installer = this.Packages.Values[index];
                this.InstallerInfo.Log.AddInfo(Util.UNINSTALL_Start + " - " + installer.Package.Name);
                installer.DeleteFiles = deleteFiles;
                installer.UnInstall();
                if (this.InstallerInfo.Log.HasWarnings)
                {
                    this.InstallerInfo.Log.AddWarning(Util.UNINSTALL_Warnings + " - " + installer.Package.Name);
                }
                else
                {
                    this.InstallerInfo.Log.AddInfo(Util.UNINSTALL_Success + " - " + installer.Package.Name);
                }
            }
        }

        private void BackupStreamIntoFile(Stream stream, PackageInfo package)
        {
            try
            {
                var filePath = Util.GetPackageBackupPath(package);

                if (File.Exists(filePath))
                {
                    File.SetAttributes(filePath, FileAttributes.Normal);
                    File.Delete(filePath);
                }

                using (var fileStream = File.Create(filePath))
                {
                    if (stream.CanSeek)
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                    }

                    stream.CopyTo(fileStream);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}
