#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

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

#endregion

namespace DotNetNuke.Services.Installer
{
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
		#region Private Members

        #endregion

		#region Constructors

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This Constructor creates a new InstallerInfo instance
        /// </summary>
        /// -----------------------------------------------------------------------------
        public InstallerInfo()
        {
            PhysicalSitePath = Null.NullString;
            Initialize();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This Constructor creates a new InstallerInfo instance from a 
        /// string representing the physical path to the root of the site
        /// </summary>
        /// <param name="sitePath">The physical path to the root of the site</param>
        /// <param name="mode">Install Mode.</param>
        /// -----------------------------------------------------------------------------
        public InstallerInfo(string sitePath, InstallMode mode)
        {
            Initialize();
            TempInstallFolder = Globals.InstallMapPath + "Temp\\" + Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            PhysicalSitePath = sitePath;
            InstallMode = mode;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This Constructor creates a new InstallerInfo instance from a Stream and a
        /// string representing the physical path to the root of the site
        /// </summary>
        /// <param name="inputStream">The Stream to use to create this InstallerInfo instance</param>
        /// <param name="sitePath">The physical path to the root of the site</param>
        /// -----------------------------------------------------------------------------
        public InstallerInfo(Stream inputStream, string sitePath)
        {
            Initialize();
            TempInstallFolder = Globals.InstallMapPath + "Temp\\" + Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            PhysicalSitePath = sitePath;

            //Read the Zip file into its component entries
            ReadZipStream(inputStream, false);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This Constructor creates a new InstallerInfo instance from a string representing
        /// the physical path to the temporary install folder and a string representing 
        /// the physical path to the root of the site
        /// </summary>
        /// <param name="tempFolder">The physical path to the zip file containg the package</param>
        /// <param name="manifest">The manifest filename</param>
        /// <param name="sitePath">The physical path to the root of the site</param>
        /// -----------------------------------------------------------------------------
        public InstallerInfo(string tempFolder, string manifest, string sitePath)
        {
            Initialize();
            TempInstallFolder = tempFolder;
            PhysicalSitePath = sitePath;
            if (!string.IsNullOrEmpty(manifest))
            {
                ManifestFile = new InstallFile(manifest, this);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This Constructor creates a new InstallerInfo instance from a PackageInfo object
        /// </summary>
        /// <param name="package">The PackageInfo instance</param>
        /// <param name="sitePath">The physical path to the root of the site</param>
        /// -----------------------------------------------------------------------------
        public InstallerInfo(PackageInfo package, string sitePath)
        {
            Initialize();
            PhysicalSitePath = sitePath;
            TempInstallFolder = Globals.InstallMapPath + "Temp\\" + Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            InstallMode = InstallMode.UnInstall;
            ManifestFile = new InstallFile(Path.Combine(TempInstallFolder, package.Name + ".dnn"));
            package.AttachInstallerInfo(this);
        }
		
		#endregion

		#region Public Properties

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets a list of allowable file extensions (in addition to the Host's List)
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string AllowableFiles { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a Dictionary of Files that are included in the Package
        /// </summary>
        /// <value>A Dictionary(Of String, InstallFile)</value>
        /// -----------------------------------------------------------------------------
        public Dictionary<string, InstallFile> Files { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether the package contains Valid Files
        /// </summary>
        /// <value>A Boolean</value>
        /// -----------------------------------------------------------------------------
        public bool HasValidFiles
        {
            get
            {
                bool _HasValidFiles = true;
                if (Files.Values.Any(file => !Util.IsFileValid(file, AllowableFiles)))
                {
                    _HasValidFiles = Null.NullBoolean;
                }
                return _HasValidFiles;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets whether the File Extension WhiteList is ignored
        /// </summary>
        /// <value>A Boolean value</value>
        /// -----------------------------------------------------------------------------
        public bool IgnoreWhiteList { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether the Package is already installed with the same version
        /// </summary>
        /// <value>A Boolean value</value>
        /// -----------------------------------------------------------------------------
        public bool Installed { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the InstallMode
        /// </summary>
        /// <value>A InstallMode value</value>
        /// -----------------------------------------------------------------------------
        public InstallMode InstallMode { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Invalid File Extensions
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string InvalidFileExtensions
        {
            get
            {
                string _InvalidFileExtensions = Files.Values.Where(file => !Util.IsFileValid(file, AllowableFiles))
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
        /// Gets whether the Installer is in legacy mode
        /// </summary>
        /// -----------------------------------------------------------------------------
        public bool IsLegacyMode { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether the InstallerInfo instance is Valid
        /// </summary>
        /// <value>A Boolean value</value>
        /// -----------------------------------------------------------------------------
        public bool IsValid
        {
            get
            {
                return Log.Valid;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the associated Logger
        /// </summary>
        /// <value>A Logger</value>
        /// -----------------------------------------------------------------------------
        public string LegacyError { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the associated Logger
        /// </summary>
        /// <value>A Logger</value>
        /// -----------------------------------------------------------------------------
        public Logger Log { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and Sets the Manifest File for the Package
        /// </summary>
        /// <value>An InstallFile</value>
        /// -----------------------------------------------------------------------------
        public InstallFile ManifestFile { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Id of the package after installation (-1 if fail)
        /// </summary>
        /// <value>An Integer</value>
        /// -----------------------------------------------------------------------------
        public int PackageID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Physical Path to the root of the Site (eg D:\Websites\DotNetNuke")
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string PhysicalSitePath { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Id of the current portal (-1 if Host)
        /// </summary>
        /// <value>An Integer</value>
        /// -----------------------------------------------------------------------------
        public int PortalID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets whether the Package Install is being repaird
        /// </summary>
        /// <value>A Boolean value</value>
        /// -----------------------------------------------------------------------------
        public bool RepairInstall { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the security Access Level of the user that is calling the INstaller
        /// </summary>
        /// <value>A SecurityAccessLevel enumeration</value>
        /// -----------------------------------------------------------------------------
        public SecurityAccessLevel SecurityAccessLevel { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Temporary Install Folder used to unzip the archive (and to place the 
        /// backups of existing files) during InstallMode
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string TempInstallFolder { get; private set; }

        #endregion

		#region Private Methods

        private void Initialize()
        {
            TempInstallFolder = Null.NullString;
            SecurityAccessLevel = SecurityAccessLevel.Host;
            RepairInstall = Null.NullBoolean;
            PortalID = Null.NullInteger;
            PackageID = Null.NullInteger;
            Log = new Logger();
            IsLegacyMode = Null.NullBoolean;
            IgnoreWhiteList = Null.NullBoolean;
            InstallMode = InstallMode.Install;
            Installed = Null.NullBoolean;
            Files = new Dictionary<string, InstallFile>();
        }

        private void ReadZipStream(Stream inputStream, bool isEmbeddedZip)
        {
            Log.StartJob(Util.FILES_Reading);
            if (inputStream.CanSeek)
            {
                inputStream.Seek(0, SeekOrigin.Begin);
            }

            var unzip = new ZipInputStream(inputStream);
            ZipEntry entry = unzip.GetNextEntry();
            while (entry != null)
            {
                if (!entry.IsDirectory)
                {
					//Add file to list
                    var file = new InstallFile(unzip, entry, this);
                    if (file.Type == InstallFileType.Resources && (file.Name.ToLowerInvariant() == "containers.zip" || file.Name.ToLowerInvariant() == "skins.zip"))
                    {
						//Temporarily save the TempInstallFolder
                        string tmpInstallFolder = TempInstallFolder;

                        //Create Zip Stream from File
                        using (var zipStream = new FileStream(file.TempFileName, FileMode.Open, FileAccess.Read))
                        {
                            //Set TempInstallFolder
                            TempInstallFolder = Path.Combine(TempInstallFolder, Path.GetFileNameWithoutExtension(file.Name));

                            //Extract files from zip
                            ReadZipStream(zipStream, true);
                        }

                        //Restore TempInstallFolder
                        TempInstallFolder = tmpInstallFolder;

                        //Delete zip file
                        var zipFile = new FileInfo(file.TempFileName);
                        zipFile.Delete();
                    }
                    else
                    {
                        Files[file.FullName.ToLower()] = file;
                        if (file.Type == InstallFileType.Manifest && !isEmbeddedZip)
                        {
                            if (ManifestFile == null)
                            {
                                ManifestFile = file;
                            }
                            else
                            {
                                if (file.Extension == "dnn7" && (ManifestFile.Extension == "dnn" || ManifestFile.Extension == "dnn5" || ManifestFile.Extension == "dnn6"))
                                {
                                    ManifestFile = file;
                                }
                                else if (file.Extension == "dnn6" && (ManifestFile.Extension == "dnn" || ManifestFile.Extension == "dnn5"))
                                {
                                   ManifestFile = file; 
                                }
                                else if (file.Extension == "dnn5" && ManifestFile.Extension == "dnn")
                                {
                                    ManifestFile = file;
                                }
                                else if (file.Extension == ManifestFile.Extension)
                                {
                                    Log.AddFailure((Util.EXCEPTION_MultipleDnn + ManifestFile.Name + " and " + file.Name));
                                }
                            }
                        }
                    }
                    Log.AddInfo(string.Format(Util.FILE_ReadSuccess, file.FullName));
                }
                entry = unzip.GetNextEntry();
            }
            if (ManifestFile == null)
            {
                Log.AddFailure(Util.EXCEPTION_MissingDnn);
            }
            if (Log.Valid)
            {
                Log.EndJob(Util.FILES_ReadingEnd);
            }
            else
            {
                Log.AddFailure(new Exception(Util.EXCEPTION_FileLoad));
                Log.EndJob(Util.FILES_ReadingEnd);
            }
			
            //Close the Zip Input Stream as we have finished with it
            inputStream.Close();
        }
		
		#endregion
    }
}
