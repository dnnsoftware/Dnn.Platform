// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Installers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Xml;
    using System.Xml.XPath;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Instrumentation;

    /// <summary>The ResourceFileInstaller installs Resource File Components (zips) to a DotNetNuke site.</summary>
    public class ResourceFileInstaller : FileInstaller
    {
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]

        // ReSharper disable once InconsistentNaming
        public const string DEFAULT_MANIFESTEXT = ".manifest";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ResourceFileInstaller));
        private string manifest;

        /// <inheritdoc />
        public override string AllowableFiles
        {
            get
            {
                return "resources, zip";
            }
        }

        /// <summary>Gets the name of the Collection Node (<c>resourceFiles</c>).</summary>
        protected override string CollectionNodeName
        {
            get
            {
                return "resourceFiles";
            }
        }

        /// <summary>Gets the name of the Item Node (<c>resourceFile</c>).</summary>
        protected override string ItemNodeName
        {
            get
            {
                return "resourceFile";
            }
        }

        protected string Manifest
        {
            get
            {
                return this.manifest;
            }
        }

        /// <inheritdoc />
        protected override void CommitFile(InstallFile insFile)
        {
        }

        /// <inheritdoc />
        protected override void DeleteFile(InstallFile file)
        {
        }

        /// <inheritdoc />
        protected override bool InstallFile(InstallFile insFile)
        {
            bool retValue = true;
            try
            {
                this.Log.AddInfo(Util.FILES_Expanding);

                // Create the folder for destination
                this.manifest = insFile.Name + ".manifest";
                if (!Directory.Exists(this.PhysicalBasePath))
                {
                    Directory.CreateDirectory(this.PhysicalBasePath);
                }

                using (var unzip = new ZipArchive(new FileStream(insFile.TempFileName, FileMode.Open)))
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

                        foreach (var entry in unzip.FileEntries())
                        {
                            entry.CheckZipEntry();
                            string fileName = Path.GetFileName(entry.FullName);

                            // Start file Element
                            writer.WriteStartElement("file");

                            // Write path
                            writer.WriteElementString(
                                "path",
                                entry.FullName.Substring(0, entry.FullName.IndexOf(fileName, StringComparison.OrdinalIgnoreCase)));

                            // Write name
                            writer.WriteElementString("name", fileName);

                            var physicalPath = Path.Combine(this.PhysicalBasePath, entry.FullName);
                            if (File.Exists(physicalPath))
                            {
                                Util.BackupFile(
                                    new InstallFile(entry.FullName, this.Package.InstallerInfo),
                                    this.PhysicalBasePath,
                                    this.Log);
                            }

                            Util.WriteStream(entry.Open(), physicalPath);

                            // Close files Element
                            writer.WriteEndElement();

                            this.Log.AddInfo(string.Format(CultureInfo.InvariantCulture, Util.FILE_Created, entry.FullName));
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

        /// <inheritdoc />
        protected override bool IsCorrectType(InstallFileType type)
        {
            return type == InstallFileType.Resources;
        }

        /// <inheritdoc />
        protected override InstallFile ReadManifestItem(XPathNavigator nav, bool checkFileExists)
        {
            InstallFile insFile = base.ReadManifestItem(nav, checkFileExists);

            this.manifest = Util.ReadElement(nav, "manifest");

            if (string.IsNullOrEmpty(this.manifest))
            {
                this.manifest = insFile.FullName + DEFAULT_MANIFESTEXT;
            }

            // Call base method
            return base.ReadManifestItem(nav, checkFileExists);
        }

        /// <inheritdoc />
        protected override void RollbackFile(InstallFile insFile)
        {
            using (var unzip = new ZipArchive(new FileStream(insFile.InstallerInfo.TempInstallFolder + insFile.FullName, FileMode.Open)))
            {
                foreach (var entry in unzip.FileEntries())
                {
                    // Check for Backups
                    if (File.Exists(insFile.BackupPath + entry.FullName))
                    {
                        // Restore File
                        Util.RestoreFile(new InstallFile(entry, this.Package.InstallerInfo), this.PhysicalBasePath, this.Log);
                    }
                    else
                    {
                        // Delete File
                        Util.DeleteFile(entry.FullName, this.PhysicalBasePath, this.Log);
                    }
                }
            }
        }

        /// <inheritdoc/>
        protected override void UnInstallFile(InstallFile unInstallFile)
        {
            this.manifest = unInstallFile.Name + ".manifest";
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
