// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Installer.Log;
    using DotNetNuke.Services.Installer.Packages;
    using ICSharpCode.SharpZipLib.Zip;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The InstallerInfo class holds all the information associated with a
    /// Installation.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class InstallerInfo
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="InstallerInfo"/> class.
        /// This Constructor creates a new InstallerInfo instance.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public InstallerInfo()
        {
            this.PhysicalSitePath = Null.NullString;
            this.Initialize();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="InstallerInfo"/> class.
        /// This Constructor creates a new InstallerInfo instance from a
        /// string representing the physical path to the root of the site.
        /// </summary>
        /// <param name="sitePath">The physical path to the root of the site.</param>
        /// <param name="mode">Install Mode.</param>
        /// -----------------------------------------------------------------------------
        public InstallerInfo(string sitePath, InstallMode mode)
        {
            this.Initialize();
            this.TempInstallFolder = Globals.InstallMapPath + "Temp\\" + Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            this.PhysicalSitePath = sitePath;
            this.InstallMode = mode;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="InstallerInfo"/> class.
        /// This Constructor creates a new InstallerInfo instance from a Stream and a
        /// string representing the physical path to the root of the site.
        /// </summary>
        /// <param name="inputStream">The Stream to use to create this InstallerInfo instance.</param>
        /// <param name="sitePath">The physical path to the root of the site.</param>
        /// -----------------------------------------------------------------------------
        public InstallerInfo(Stream inputStream, string sitePath)
        {
            this.Initialize();
            this.TempInstallFolder = Globals.InstallMapPath + "Temp\\" + Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            this.PhysicalSitePath = sitePath;

            // Read the Zip file into its component entries
            this.ReadZipStream(inputStream, false);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="InstallerInfo"/> class.
        /// This Constructor creates a new InstallerInfo instance from a string representing
        /// the physical path to the temporary install folder and a string representing
        /// the physical path to the root of the site.
        /// </summary>
        /// <param name="tempFolder">The physical path to the zip file containg the package.</param>
        /// <param name="manifest">The manifest filename.</param>
        /// <param name="sitePath">The physical path to the root of the site.</param>
        /// -----------------------------------------------------------------------------
        public InstallerInfo(string tempFolder, string manifest, string sitePath)
        {
            this.Initialize();
            this.TempInstallFolder = tempFolder;
            this.PhysicalSitePath = sitePath;
            if (!string.IsNullOrEmpty(manifest))
            {
                this.ManifestFile = new InstallFile(manifest, this);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="InstallerInfo"/> class.
        /// This Constructor creates a new InstallerInfo instance from a PackageInfo object.
        /// </summary>
        /// <param name="package">The PackageInfo instance.</param>
        /// <param name="sitePath">The physical path to the root of the site.</param>
        /// -----------------------------------------------------------------------------
        public InstallerInfo(PackageInfo package, string sitePath)
        {
            this.Initialize();
            this.PhysicalSitePath = sitePath;
            this.TempInstallFolder = Globals.InstallMapPath + "Temp\\" + Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            this.InstallMode = InstallMode.UnInstall;
            this.ManifestFile = new InstallFile(Path.Combine(this.TempInstallFolder, package.Name + ".dnn"));
            package.AttachInstallerInfo(this);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether gets whether the package contains Valid Files.
        /// </summary>
        /// <value>A Boolean.</value>
        /// -----------------------------------------------------------------------------
        public bool HasValidFiles
        {
            get
            {
                bool _HasValidFiles = true;
                if (this.Files.Values.Any(file => !Util.IsFileValid(file, this.AllowableFiles)))
                {
                    _HasValidFiles = Null.NullBoolean;
                }

                return _HasValidFiles;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Invalid File Extensions.
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string InvalidFileExtensions
        {
            get
            {
                string _InvalidFileExtensions = this.Files.Values.Where(file => !Util.IsFileValid(file, this.AllowableFiles))
                                                            .Aggregate(Null.NullString, (current, file) => current + (", " + file.Extension));
                if (!string.IsNullOrEmpty(_InvalidFileExtensions))
                {
                    _InvalidFileExtensions = _InvalidFileExtensions.Substring(2);
                }

                return _InvalidFileExtensions;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether gets whether the InstallerInfo instance is Valid.
        /// </summary>
        /// <value>A Boolean value.</value>
        /// -----------------------------------------------------------------------------
        public bool IsValid
        {
            get
            {
                return this.Log.Valid;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets a list of allowable file extensions (in addition to the Host's List).
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string AllowableFiles { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a Dictionary of Files that are included in the Package.
        /// </summary>
        /// <value>A Dictionary(Of String, InstallFile).</value>
        /// -----------------------------------------------------------------------------
        public Dictionary<string, InstallFile> Files { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets whether the File Extension WhiteList is ignored.
        /// </summary>
        /// <value>A Boolean value.</value>
        /// -----------------------------------------------------------------------------
        public bool IgnoreWhiteList { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets whether the Package is already installed with the same version.
        /// </summary>
        /// <value>A Boolean value.</value>
        /// -----------------------------------------------------------------------------
        public bool Installed { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the InstallMode.
        /// </summary>
        /// <value>A InstallMode value.</value>
        /// -----------------------------------------------------------------------------
        public InstallMode InstallMode { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets whether the Installer is in legacy mode.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public bool IsLegacyMode { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the associated Logger.
        /// </summary>
        /// <value>A Logger.</value>
        /// -----------------------------------------------------------------------------
        public string LegacyError { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the associated Logger.
        /// </summary>
        /// <value>A Logger.</value>
        /// -----------------------------------------------------------------------------
        public Logger Log { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and Sets the Manifest File for the Package.
        /// </summary>
        /// <value>An InstallFile.</value>
        /// -----------------------------------------------------------------------------
        public InstallFile ManifestFile { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Id of the package after installation (-1 if fail).
        /// </summary>
        /// <value>An Integer.</value>
        /// -----------------------------------------------------------------------------
        public int PackageID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Physical Path to the root of the Site (eg D:\Websites\DotNetNuke").
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string PhysicalSitePath { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Id of the current portal (-1 if Host).
        /// </summary>
        /// <value>An Integer.</value>
        /// -----------------------------------------------------------------------------
        public int PortalID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets whether the Package Install is being repaird.
        /// </summary>
        /// <value>A Boolean value.</value>
        /// -----------------------------------------------------------------------------
        public bool RepairInstall { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the security Access Level of the user that is calling the INstaller.
        /// </summary>
        /// <value>A SecurityAccessLevel enumeration.</value>
        /// -----------------------------------------------------------------------------
        public SecurityAccessLevel SecurityAccessLevel { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Temporary Install Folder used to unzip the archive (and to place the
        /// backups of existing files) during InstallMode.
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string TempInstallFolder { get; private set; }

        private void Initialize()
        {
            this.TempInstallFolder = Null.NullString;
            this.SecurityAccessLevel = SecurityAccessLevel.Host;
            this.RepairInstall = Null.NullBoolean;
            this.PortalID = Null.NullInteger;
            this.PackageID = Null.NullInteger;
            this.Log = new Logger();
            this.IsLegacyMode = Null.NullBoolean;
            this.IgnoreWhiteList = Null.NullBoolean;
            this.InstallMode = InstallMode.Install;
            this.Installed = Null.NullBoolean;
            this.Files = new Dictionary<string, InstallFile>();
        }

        private void ReadZipStream(Stream inputStream, bool isEmbeddedZip)
        {
            this.Log.StartJob(Util.FILES_Reading);
            if (inputStream.CanSeek)
            {
                inputStream.Seek(0, SeekOrigin.Begin);
            }

            var unzip = new ZipInputStream(inputStream);
            ZipEntry entry = unzip.GetNextEntry();
            while (entry != null)
            {
                entry.CheckZipEntry();
                if (!entry.IsDirectory)
                {
                    // Add file to list
                    var file = new InstallFile(unzip, entry, this);
                    if (file.Type == InstallFileType.Resources && (file.Name.Equals("containers.zip", StringComparison.InvariantCultureIgnoreCase) || file.Name.Equals("skins.zip", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        // Temporarily save the TempInstallFolder
                        string tmpInstallFolder = this.TempInstallFolder;

                        // Create Zip Stream from File
                        using (var zipStream = new FileStream(file.TempFileName, FileMode.Open, FileAccess.Read))
                        {
                            // Set TempInstallFolder
                            this.TempInstallFolder = Path.Combine(this.TempInstallFolder, Path.GetFileNameWithoutExtension(file.Name));

                            // Extract files from zip
                            this.ReadZipStream(zipStream, true);
                        }

                        // Restore TempInstallFolder
                        this.TempInstallFolder = tmpInstallFolder;

                        // Delete zip file
                        var zipFile = new FileInfo(file.TempFileName);
                        zipFile.Delete();
                    }
                    else
                    {
                        this.Files[file.FullName.ToLowerInvariant()] = file;
                        if (file.Type == InstallFileType.Manifest && !isEmbeddedZip)
                        {
                            if (this.ManifestFile == null)
                            {
                                this.ManifestFile = file;
                            }
                            else
                            {
                                if (file.Extension == "dnn7" && (this.ManifestFile.Extension == "dnn" || this.ManifestFile.Extension == "dnn5" || this.ManifestFile.Extension == "dnn6"))
                                {
                                    this.ManifestFile = file;
                                }
                                else if (file.Extension == "dnn6" && (this.ManifestFile.Extension == "dnn" || this.ManifestFile.Extension == "dnn5"))
                                {
                                    this.ManifestFile = file;
                                }
                                else if (file.Extension == "dnn5" && this.ManifestFile.Extension == "dnn")
                                {
                                    this.ManifestFile = file;
                                }
                                else if (file.Extension == this.ManifestFile.Extension)
                                {
                                    this.Log.AddFailure(Util.EXCEPTION_MultipleDnn + this.ManifestFile.Name + " and " + file.Name);
                                }
                            }
                        }
                    }

                    this.Log.AddInfo(string.Format(Util.FILE_ReadSuccess, file.FullName));
                }

                entry = unzip.GetNextEntry();
            }

            if (this.ManifestFile == null)
            {
                this.Log.AddFailure(Util.EXCEPTION_MissingDnn);
            }

            if (this.Log.Valid)
            {
                this.Log.EndJob(Util.FILES_ReadingEnd);
            }
            else
            {
                this.Log.AddFailure(new Exception(Util.EXCEPTION_FileLoad));
                this.Log.EndJob(Util.FILES_ReadingEnd);
            }

            // Close the Zip Input Stream as we have finished with it
            inputStream.Close();
        }
    }
}
