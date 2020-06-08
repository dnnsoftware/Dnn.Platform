// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Xml.Serialization;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities;
using DotNetNuke.Services.Installer.Log;

#endregion

namespace DotNetNuke.Services.Installer.Packages
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The PackageInfo class represents a single Installer Package
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class PackageInfo : BaseEntityInfo
    {
        private IList<PackageDependencyInfo> _dependencies;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This Constructor creates a new InstallPackage instance as defined by the
        /// Parameters
        /// </summary>
        /// -----------------------------------------------------------------------------
        public PackageInfo(InstallerInfo info) : this()
        {
            AttachInstallerInfo(info);
        }

        /// <summary>
        /// This Constructor creates a new InstallPackage instance
        /// </summary>
        /// -----------------------------------------------------------------------------
        public PackageInfo()
        {
            PackageID = Null.NullInteger;
            PortalID = Null.NullInteger;
            Version = new Version(0, 0, 0);
            IsValid = true;
            InstalledVersion = new Version(0, 0, 0);
        }

        /// <summary>Gets the direct dependencies of this package.</summary>
        [XmlIgnore]
        public IList<PackageDependencyInfo> Dependencies 
        { 
            get
            {
                return _dependencies ?? (_dependencies = (PackageID == -1) 
                                        ? new List<PackageDependencyInfo>() 
                                        : PackageController.Instance.GetPackageDependencies(p => p.PackageId == PackageID));
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Email for this package
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string Email { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Description of this package
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string Description { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the FileName of this package
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string FileName { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a Dictionary of Files that are included in the Package
        /// </summary>
        /// <value>A Dictionary(Of String, InstallFile)</value>
        /// -----------------------------------------------------------------------------
        [XmlIgnore]
        public Dictionary<string, InstallFile> Files
        {
            get
            {
                return InstallerInfo.Files;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name (path) of the folder where the package is installed
        /// </summary>
        /// <value>A string</value>
        /// -----------------------------------------------------------------------------
        public string FolderName { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and Sets the FriendlyName of this package
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string FriendlyName { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the url for the icon for the package
        /// </summary>
        /// <value>A string</value>
        /// -----------------------------------------------------------------------------
        public string IconFile { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Installed Version of the Package
        /// </summary>
        /// <value>A System.Version</value>
        /// -----------------------------------------------------------------------------
        public Version InstalledVersion { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the associated InstallerInfo
        /// </summary>
        /// <value>An InstallerInfo object</value>
        /// -----------------------------------------------------------------------------
        [XmlIgnore]
        public InstallerInfo InstallerInfo { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the InstallMode
        /// </summary>
        /// <value>An InstallMode value</value>
        /// -----------------------------------------------------------------------------
        public InstallMode InstallMode
        {
            get
            {
                return InstallerInfo.InstallMode;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and Sets whether this package is a "system" Package
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public bool IsSystemPackage { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether the Package is Valid
        /// </summary>
        /// <value>A Boolean value</value>
        /// -----------------------------------------------------------------------------
        [XmlIgnore]
        public bool IsValid { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and Sets the License of this package
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string License { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Logger
        /// </summary>
        /// <value>An Logger object</value>
        /// -----------------------------------------------------------------------------
        [XmlIgnore]
        public Logger Log
        {
            get
            {
                return InstallerInfo.Log;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Manifest of this package
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string Manifest { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Name of this package
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string Name { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Organisation for this package
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string Organization { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Owner of this package
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string Owner { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the ID of this package
        /// </summary>
        /// <value>An Integer</value>
        /// -----------------------------------------------------------------------------
        public int PackageID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Type of this package
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string PackageType { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the ID of this portal
        /// </summary>
        /// <value>An Integer</value>
        /// -----------------------------------------------------------------------------
        public int PortalID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and Sets the ReleaseNotes of this package
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string ReleaseNotes { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Url for this package
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string Url { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Version of this package
        /// </summary>
        /// <value>A System.Version</value>
        /// -----------------------------------------------------------------------------
        public Version Version { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The AttachInstallerInfo method attachs an InstallerInfo instance to the Package
        /// </summary>
        /// <param name="installer">The InstallerInfo instance to attach</param>
        /// -----------------------------------------------------------------------------
        public void AttachInstallerInfo(InstallerInfo installer)
        {
            InstallerInfo = installer;
        }

        /// <summary>
        /// Clone current object.
        /// </summary>
        /// <returns></returns>
        public PackageInfo Clone()
        {
            return new PackageInfo
                   {
                       PackageID = PackageID,
                       PortalID = PortalID,
                       PackageType = PackageType,
                       InstallerInfo = InstallerInfo,
                       Name = Name,
                       FriendlyName = FriendlyName,
                       Manifest = Manifest,
                       Email = Email,
                       Description = Description,
                       FolderName = FolderName,
                       FileName = FileName,
                       IconFile = IconFile,
                       IsSystemPackage = IsSystemPackage,
                       IsValid = IsValid,
                       Organization = Organization,
                       Owner = Owner,
                       License = License,
                       ReleaseNotes = ReleaseNotes,
                       Url = Url,
                       Version = Version,
                       InstalledVersion = InstalledVersion
                   };
        }
    }
}
