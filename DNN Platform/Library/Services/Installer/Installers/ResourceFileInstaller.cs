// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Installers
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Instrumentation;
    using ICSharpCode.SharpZipLib.Zip;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ResourceFileInstaller installs Resource File Components (zips) to a DotNetNuke site.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class ResourceFileInstaller : FileInstaller
    {
        public const string DEFAULT_MANIFESTEXT = ".manifest";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ResourceFileInstaller));
        private string _Manifest;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a list of allowable file extensions (in addition to the Host's List).
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public override string AllowableFiles
        {
            get
            {
                return "resources, zip";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Collection Node ("resourceFiles").
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        protected override string CollectionNodeName
        {
            get
            {
                return "resourceFiles";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Item Node ("resourceFile").
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        protected override string ItemNodeName
        {
            get
            {
                return "resourceFile";
            }
        }

        // -----------------------------------------------------------------------------
        protected string Manifest
        {
            get
            {
                return this._Manifest;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The CommitFile method commits a single file.
        /// </summary>
        /// <param name="insFile">The InstallFile to commit.</param>
        /// -----------------------------------------------------------------------------
        protected override void CommitFile(InstallFile insFile)
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The DeleteFile method deletes a single assembly.
        /// </summary>
        /// <param name="file">The InstallFile to delete.</param>
        /// -----------------------------------------------------------------------------
        protected override void DeleteFile(InstallFile file)
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   The InstallFile method installs a single assembly.
        /// </summary>
        /// <param name = "insFile">The InstallFile to install.</param>
        /// <returns></returns>
        protected override bool InstallFile(InstallFile insFile)
        {
            bool retValue = true;
            try
            {
                this.Log.AddInfo(Util.FILES_Expanding);

                // Create the folder for destination
                this._Manifest = insFile.Name + ".manifest";
                if (!Directory.Exists(this.PhysicalBasePath))
                {
                    Directory.CreateDirectory(this.PhysicalBasePath);
                }

                using (var unzip = new ZipInputStream(new FileStream(insFile.TempFileName, FileMode.Open)))
                using (var manifestStream = new FileStream(Path.Combine(this.PhysicalBasePath, this.Manifest), FileMode.Create, FileAccess.Write))
                {
                    var settings = new XmlWriterSettings();
                    settings.ConformanceLevel = ConformanceLevel.Fragment;
                    settings.OmitXmlDeclaration = true;
                    settings.Indent = true;

                    using (var writer = XmlWriter.Create(manifestStream, settings))
                    {
                        // Start the new Root Element
                        writer.WriteStartElement("dotnetnuke");
                        writer.WriteAttributeString("type", "ResourceFile");
                        writer.WriteAttributeString("version", "5.0");

                        // Start files Element
                        writer.WriteStartElement("files");

                        ZipEntry entry = unzip.GetNextEntry();
                        while (entry != null)
                        {
                            entry.CheckZipEntry();
                            if (!entry.IsDirectory)
                            {
                                string fileName = Path.GetFileName(entry.Name);

                                // Start file Element
                                writer.WriteStartElement("file");

                                // Write path
                                writer.WriteElementString(
                                    "path",
                                    entry.Name.Substring(0, entry.Name.IndexOf(fileName)));

                                // Write name
                                writer.WriteElementString("name", fileName);

                                var physicalPath = Path.Combine(this.PhysicalBasePath, entry.Name);
                                if (File.Exists(physicalPath))
                                {
                                    Util.BackupFile(
                                        new InstallFile(entry.Name, this.Package.InstallerInfo),
                                        this.PhysicalBasePath,
                                        this.Log);
                                }

                                Util.WriteStream(unzip, physicalPath);

                                // Close files Element
                                writer.WriteEndElement();

                                this.Log.AddInfo(string.Format(Util.FILE_Created, entry.Name));
                            }

                            entry = unzip.GetNextEntry();
                        }

                        // Close files Element
                        writer.WriteEndElement();

                        this.Log.AddInfo(Util.FILES_CreatedResources);
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);

                retValue = false;
            }

            return retValue;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a flag that determines what type of file this installer supports.
        /// </summary>
        /// <param name="type">The type of file being processed.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        protected override bool IsCorrectType(InstallFileType type)
        {
            return type == InstallFileType.Resources;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The ReadManifestItem method reads a single node.
        /// </summary>
        /// <param name="nav">The XPathNavigator representing the node.</param>
        /// <param name="checkFileExists">Flag that determines whether a check should be made.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        protected override InstallFile ReadManifestItem(XPathNavigator nav, bool checkFileExists)
        {
            InstallFile insFile = base.ReadManifestItem(nav, checkFileExists);

            this._Manifest = Util.ReadElement(nav, "manifest");

            if (string.IsNullOrEmpty(this._Manifest))
            {
                this._Manifest = insFile.FullName + DEFAULT_MANIFESTEXT;
            }

            // Call base method
            return base.ReadManifestItem(nav, checkFileExists);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The RollbackFile method rolls back the install of a single file.
        /// </summary>
        /// <remarks>For new installs this removes the added file.  For upgrades it restores the
        /// backup file created during install.</remarks>
        /// <param name="insFile">The InstallFile to commit.</param>
        /// -----------------------------------------------------------------------------
        protected override void RollbackFile(InstallFile insFile)
        {
            using (var unzip = new ZipInputStream(new FileStream(insFile.InstallerInfo.TempInstallFolder + insFile.FullName, FileMode.Open)))
            {
                ZipEntry entry = unzip.GetNextEntry();
                while (entry != null)
                {
                    entry.CheckZipEntry();
                    if (!entry.IsDirectory)
                    {
                        // Check for Backups
                        if (File.Exists(insFile.BackupPath + entry.Name))
                        {
                            // Restore File
                            Util.RestoreFile(new InstallFile(unzip, entry, this.Package.InstallerInfo), this.PhysicalBasePath, this.Log);
                        }
                        else
                        {
                            // Delete File
                            Util.DeleteFile(entry.Name, this.PhysicalBasePath, this.Log);
                        }
                    }

                    entry = unzip.GetNextEntry();
                }
            }
        }

        protected override void UnInstallFile(InstallFile unInstallFile)
        {
            this._Manifest = unInstallFile.Name + ".manifest";
            var doc = new XPathDocument(Path.Combine(this.PhysicalBasePath, this.Manifest));

            foreach (XPathNavigator fileNavigator in doc.CreateNavigator().Select("dotnetnuke/files/file"))
            {
                string path = XmlUtils.GetNodeValue(fileNavigator, "path");
                string fileName = XmlUtils.GetNodeValue(fileNavigator, "name");
                string filePath = Path.Combine(path, fileName);
                try
                {
                    if (this.DeleteFiles)
                    {
                        Util.DeleteFile(filePath, this.PhysicalBasePath, this.Log);
                    }
                }
                catch (Exception ex)
                {
                    this.Log.AddFailure(ex);
                }
            }

            if (this.DeleteFiles)
            {
                Util.DeleteFile(this.Manifest, this.PhysicalBasePath, this.Log);
            }
        }
    }
}
