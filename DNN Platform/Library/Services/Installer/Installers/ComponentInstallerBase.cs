#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.Xml.XPath;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Installer.Log;
using DotNetNuke.Services.Installer.Packages;

#endregion

namespace DotNetNuke.Services.Installer.Installers
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ComponentInstallerBase is a base class for all Component Installers
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	07/24/2007  created
    /// </history>
    /// -----------------------------------------------------------------------------
    public abstract class ComponentInstallerBase
    {
        protected ComponentInstallerBase()
        {
            Completed = Null.NullBoolean;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a list of allowable file extensions (in addition to the Host's List)
        /// </summary>
        /// <value>A String</value>
        /// <history>
        /// 	[cnurse]	03/28/2008  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public virtual string AllowableFiles
        {
            get
            {
                return Null.NullString;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Completed flag
        /// </summary>
        /// <value>A Boolean value</value>
        /// <history>
        /// 	[cnurse]	07/31/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public bool Completed { get; set; }

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
                return Package.InstallMode;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Logger
        /// </summary>
        /// <value>An Logger object</value>
        /// <history>
        /// 	[cnurse]	07/31/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public Logger Log
        {
            get
            {
                return Package.Log;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the associated Package
        /// </summary>
        /// <value>An PackageInfo object</value>
        /// <history>
        /// 	[cnurse]	07/24/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public PackageInfo Package { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a Dictionary of Files that are included in the Package
        /// </summary>
        /// <value>A Dictionary(Of String, InstallFile)</value>
        /// <history>
        /// 	[cnurse]	07/31/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public Dictionary<string, InstallFile> PackageFiles
        {
            get
            {
                return Package.Files;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Physical Path to the root of the Site (eg D:\Websites\DotNetNuke")
        /// </summary>
        /// <value>A String</value>
        /// <history>
        /// 	[cnurse]	07/24/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public string PhysicalSitePath
        {
            get
            {
                return Package.InstallerInfo.PhysicalSitePath;
            }
        }

        public bool Skipped { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether the Installer supports Manifest only installs
        /// </summary>
        /// <value>A Boolean</value>
        /// <history>
        /// 	[cnurse]	02/29/2008  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public virtual bool SupportsManifestOnlyInstall
        {
            get
            {
                return true;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Type of the component
        /// </summary>
        /// <value>A String</value>
        /// <history>
        /// 	[cnurse]	07/24/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public string Type { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Version of the Component
        /// </summary>
        /// <value>A System.Version</value>
        /// <history>
        /// 	[cnurse]	02/29/2008  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public Version Version { get; set; }

        public abstract void Commit();

        public abstract void Install();

        public abstract void ReadManifest(XPathNavigator manifestNav);

        public abstract void Rollback();

        public abstract void UnInstall();
    }
}
