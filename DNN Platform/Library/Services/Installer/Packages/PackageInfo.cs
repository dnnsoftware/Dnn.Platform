#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
    /// <history>
    /// 	[cnurse]	07/24/2007  created
    /// </history>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class PackageInfo : BaseEntityInfo
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This Constructor creates a new InstallPackage instance as defined by the
        /// Parameters
        /// </summary>
        /// <history>
        /// 	[cnurse]	07/24/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public PackageInfo(InstallerInfo info) : this()
        {
            AttachInstallerInfo(info);
        }

        /// <summary>
        /// This Constructor creates a new InstallPackage instance
        /// </summary>
        /// <history>
        /// 	[cnurse]	07/24/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public PackageInfo()
        {
            PackageID = Null.NullInteger;
            PortalID = Null.NullInteger;
            Version = new Version(0, 0, 0);
            IsValid = true;
            InstalledVersion = new Version(0, 0, 0);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the ID of this package
        /// </summary>
        /// <value>An Integer</value>
        /// <history>
        /// 	[cnurse]	07/26/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public int PackageID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the ID of this portal
        /// </summary>
        /// <value>An Integer</value>
        /// <history>
        /// 	[cnurse]	09/11/2008  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public int PortalID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Owner of this package
        /// </summary>
        /// <value>A String</value>
        /// <history>
        /// 	[cnurse]	03/26/2008  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public string Owner { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Organisation for this package
        /// </summary>
        /// <value>A String</value>
        /// <history>
        /// 	[cnurse]	03/26/2008  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public string Organization { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Url for this package
        /// </summary>
        /// <value>A String</value>
        /// <history>
        /// 	[cnurse]	03/26/2008  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public string Url { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Email for this package
        /// </summary>
        /// <value>A String</value>
        /// <history>
        /// 	[cnurse]	03/26/2008  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public string Email { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Description of this package
        /// </summary>
        /// <value>A String</value>
        /// <history>
        /// 	[cnurse]	07/26/2007  created
        /// </history>
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
        /// <history>
        /// 	[cnurse]	07/31/2007  created
        /// </history>
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
        /// Gets and Sets the FriendlyName of this package
        /// </summary>
        /// <value>A String</value>
        /// <history>
        /// 	[cnurse]	08/03/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public string FriendlyName { get; set; }

        public string FolderName { get; set; }

        public string IconFile { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the associated InstallerInfo
        /// </summary>
        /// <value>An InstallerInfo object</value>
        /// <history>
        /// 	[cnurse]	07/24/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        [XmlIgnore]
        public InstallerInfo InstallerInfo { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Installed Version of the Package
        /// </summary>
        /// <value>A System.Version</value>
        /// <history>
        /// 	[cnurse]	08/07/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public Version InstalledVersion { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the InstallMode
        /// </summary>
        /// <value>An InstallMode value</value>
        /// <history>
        /// 	[cnurse]	07/31/2007  created
        /// </history>
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
        /// <history>
        /// 	[cnurse]	02/19/2008  created
        /// </history>
        /// -----------------------------------------------------------------------------

        public bool IsSystemPackage { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether the Package is Valid
        /// </summary>
        /// <value>A Boolean value</value>
        /// <history>
        /// 	[cnurse]	08/03/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        [XmlIgnore]
        public bool IsValid { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and Sets the License of this package
        /// </summary>
        /// <value>A String</value>
        /// <history>
        /// 	[cnurse]	08/03/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public string License { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Logger
        /// </summary>
        /// <value>An Logger object</value>
        /// <history>
        /// 	[cnurse]	07/31/2007  created
        /// </history>
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
        /// <history>
        /// 	[cnurse]	07/26/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public string Manifest { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Name of this package
        /// </summary>
        /// <value>A String</value>
        /// <history>
        /// 	[cnurse]	07/24/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public string Name { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Type of this package
        /// </summary>
        /// <value>A String</value>
        /// <history>
        /// 	[cnurse]	07/24/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public string PackageType { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and Sets the ReleaseNotes of this package
        /// </summary>
        /// <value>A String</value>
        /// <history>
        /// 	[cnurse]	01/07/2008  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public string ReleaseNotes { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Version of this package
        /// </summary>
        /// <value>A System.Version</value>
        /// <history>
        /// 	[cnurse]	07/26/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public Version Version { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The AttachInstallerInfo method attachs an InstallerInfo instance to the Package
        /// </summary>
        /// <param name="installer">The InstallerInfo instance to attach</param>
        /// <history>
        /// 	[cnurse]	08/01/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public void AttachInstallerInfo(InstallerInfo installer)
        {
            InstallerInfo = installer;
        }
    }
}
