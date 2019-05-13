// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#region Usings

using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Instrumentation;

using ICSharpCode.SharpZipLib.Zip;

#endregion

namespace DotNetNuke.Services.Installer.Installers
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ResourceFileInstaller installs Resource File Components (zips) to a DotNetNuke site
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class ResourceFileInstaller : FileInstaller
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ResourceFileInstaller));
        #region "Public Contants"
        public const string DEFAULT_MANIFESTEXT = ".manifest";
        private string _Manifest;

        #endregion

        #region "Protected Properties"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Collection Node ("resourceFiles")
        /// </summary>
        /// <value>A String</value>
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
        /// Gets the name of the Item Node ("resourceFile")
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        protected override string ItemNodeName
        {
            get
            {
                return "resourceFile";
            }
        }

        /// -----------------------------------------------------------------------------
        protected string Manifest
        {
            get
            {
                return _Manifest;
            }
        }

        #endregion

        #region "Public Properties"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a list of allowable file extensions (in addition to the Host's List)
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public override string AllowableFiles
        {
            get
            {
                return "resources, zip";
            }
        }

        #endregion

        #region "Protected Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The CommitFile method commits a single file.
        /// </summary>
        /// <param name="insFile">The InstallFile to commit</param>
        /// -----------------------------------------------------------------------------
        protected override void CommitFile(InstallFile insFile)
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The DeleteFile method deletes a single assembly.
        /// </summary>
        /// <param name="file">The InstallFile to delete</param>
        /// -----------------------------------------------------------------------------
        protected override void DeleteFile(InstallFile file)
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   The InstallFile method installs a single assembly.
        /// </summary>
        /// <param name = "insFile">The InstallFile to install</param>
        protected override bool InstallFile(InstallFile insFile)
        {
            bool retValue = true;
            try
            {
                Log.AddInfo(Util.FILES_Expanding);
                //Create the folder for destination            
                _Manifest = insFile.Name + ".manifest";
                if (!Directory.Exists(PhysicalBasePath))
                {
                    Directory.CreateDirectory(PhysicalBasePath);
                }
                using (var unzip = new ZipInputStream(new FileStream(insFile.TempFileName, FileMode.Open)))
                using (var manifestStream = new FileStream(Path.Combine(PhysicalBasePath, Manifest), FileMode.Create, FileAccess.Write))
                {
                    var settings = new XmlWriterSettings();
                    settings.ConformanceLevel = ConformanceLevel.Fragment;
                    settings.OmitXmlDeclaration = true;
                    settings.Indent = true;

                    using (var writer = XmlWriter.Create(manifestStream, settings))
                    {

                        //Start the new Root Element
                        writer.WriteStartElement("dotnetnuke");
                        writer.WriteAttributeString("type", "ResourceFile");
                        writer.WriteAttributeString("version", "5.0");

                        //Start files Element
                        writer.WriteStartElement("files");

                        ZipEntry entry = unzip.GetNextEntry();
                        while (entry != null)
                        {
                            if (!entry.IsDirectory)
                            {
                                string fileName = Path.GetFileName(entry.Name);

                                //Start file Element
                                writer.WriteStartElement("file");

                                //Write path
                                writer.WriteElementString("path",
                                    entry.Name.Substring(0, entry.Name.IndexOf(fileName)));

                                //Write name
                                writer.WriteElementString("name", fileName);

                                var physicalPath = Path.Combine(PhysicalBasePath, entry.Name);
                                if (File.Exists(physicalPath))
                                {
                                    Util.BackupFile(new InstallFile(entry.Name, Package.InstallerInfo),
                                        PhysicalBasePath,
                                        Log);
                                }

                                Util.WriteStream(unzip, physicalPath);

                                //Close files Element
                                writer.WriteEndElement();

                                Log.AddInfo(string.Format(Util.FILE_Created, entry.Name));
                            }

                            entry = unzip.GetNextEntry();
                        }

                        //Close files Element
                        writer.WriteEndElement();

                        Log.AddInfo(Util.FILES_CreatedResources);
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
        /// Gets a flag that determines what type of file this installer supports
        /// </summary>
        /// <param name="type">The type of file being processed</param>
        /// -----------------------------------------------------------------------------
        protected override bool IsCorrectType(InstallFileType type)
        {
            return (type == InstallFileType.Resources);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The ReadManifestItem method reads a single node
        /// </summary>
        /// <param name="nav">The XPathNavigator representing the node</param>
        /// <param name="checkFileExists">Flag that determines whether a check should be made</param>
        /// -----------------------------------------------------------------------------
        protected override InstallFile ReadManifestItem(XPathNavigator nav, bool checkFileExists)
        {
            InstallFile insFile = base.ReadManifestItem(nav, checkFileExists);

            _Manifest = Util.ReadElement(nav, "manifest");

            if (string.IsNullOrEmpty(_Manifest))
            {
                _Manifest = insFile.FullName + DEFAULT_MANIFESTEXT;
            }

            //Call base method
            return base.ReadManifestItem(nav, checkFileExists);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The RollbackFile method rolls back the install of a single file.
        /// </summary>
        /// <remarks>For new installs this removes the added file.  For upgrades it restores the
        /// backup file created during install</remarks>
        /// <param name="insFile">The InstallFile to commit</param>
        /// -----------------------------------------------------------------------------
        protected override void RollbackFile(InstallFile insFile)
        {
            using (var unzip = new ZipInputStream(new FileStream(insFile.InstallerInfo.TempInstallFolder + insFile.FullName, FileMode.Open)))
            {
                ZipEntry entry = unzip.GetNextEntry();
                while (entry != null)
                {
                    if (!entry.IsDirectory)
                    {
                        //Check for Backups
                        if (File.Exists(insFile.BackupPath + entry.Name))
                        {
                            //Restore File
                            Util.RestoreFile(new InstallFile(unzip, entry, Package.InstallerInfo), PhysicalBasePath, Log);
                        }
                        else
                        {
                            //Delete File
                            Util.DeleteFile(entry.Name, PhysicalBasePath, Log);
                        }
                    }
                    entry = unzip.GetNextEntry();
                }
            }
        }

        protected override void UnInstallFile(InstallFile unInstallFile)
        {
            _Manifest = unInstallFile.Name + ".manifest";
            var doc = new XPathDocument(Path.Combine(PhysicalBasePath, Manifest));

            foreach (XPathNavigator fileNavigator in doc.CreateNavigator().Select("dotnetnuke/files/file"))
            {
                string path = XmlUtils.GetNodeValue(fileNavigator, "path");
                string fileName = XmlUtils.GetNodeValue(fileNavigator, "name");
                string filePath = Path.Combine(path, fileName);
                try
                {
                    if (DeleteFiles)
                    {
                        Util.DeleteFile(filePath, PhysicalBasePath, Log);
                    }
                }
                catch (Exception ex)
                {
                    Log.AddFailure(ex);
                }
            }
            if (DeleteFiles)
            {
                Util.DeleteFile(Manifest, PhysicalBasePath, Log);
            }
        }

        #endregion
    }
}