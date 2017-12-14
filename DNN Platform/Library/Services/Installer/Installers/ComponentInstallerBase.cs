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
using System.Xml.XPath;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.EventQueue;
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
        /// -----------------------------------------------------------------------------
        public bool Completed { get; set; }

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
                return Package.InstallMode;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Logger
        /// </summary>
        /// <value>An Logger object</value>
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
        /// -----------------------------------------------------------------------------
        public PackageInfo Package { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a Dictionary of Files that are included in the Package
        /// </summary>
        /// <value>A Dictionary(Of String, InstallFile)</value>
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
        /// -----------------------------------------------------------------------------
        public string PhysicalSitePath
        {
            get
            {
                return Package.InstallerInfo.PhysicalSitePath;
            }
        }

        public EventMessage ReadEventMessageNode(XPathNavigator manifestNav)
        {
            EventMessage eventMessage = null;

            XPathNavigator eventMessageNav = manifestNav.SelectSingleNode("eventMessage");
            if (eventMessageNav != null)
            {
                eventMessage = new EventMessage
                                    {
                                        Priority = MessagePriority.High,
                                        ExpirationDate = DateTime.Now.AddYears(-1),
                                        SentDate = DateTime.Now,
                                        Body = "",
                                        ProcessorType = Util.ReadElement(eventMessageNav, "processorType", Log, Util.EVENTMESSAGE_TypeMissing),
                                        ProcessorCommand = Util.ReadElement(eventMessageNav, "processorCommand", Log, Util.EVENTMESSAGE_CommandMissing)
                                    };
                foreach (XPathNavigator attributeNav in eventMessageNav.Select("attributes/*"))
                {
                    var attribName = attributeNav.Name;
                    var attribValue = attributeNav.Value;
                    if (attribName == "upgradeVersionsList")
                    {
                        if (!String.IsNullOrEmpty(attribValue))
                        {
                            string[] upgradeVersions = attribValue.Split(',');
                            attribValue = "";
                            foreach (string version in upgradeVersions)
                            {
                                switch (version.ToLowerInvariant())
                                {
                                    case "install":
                                        if (Package.InstalledVersion == new Version(0, 0, 0))
                                        {
                                            attribValue += version + ",";
                                        }
                                        break;
                                    case "upgrade":
                                        if (Package.InstalledVersion > new Version(0, 0, 0))
                                        {
                                            attribValue += version + ",";
                                        }
                                        break;
                                    default:
                                        Version upgradeVersion = null;
                                        try
                                        {
                                            upgradeVersion = new Version(version);
                                        }
                                        catch (FormatException)
                                        {
                                            Log.AddWarning(string.Format(Util.MODULE_InvalidVersion, version));
                                        }

                                        if (upgradeVersion != null && (Globals.Status == Globals.UpgradeStatus.Install)) //To allow when fresh installing or installresources
                                        {
                                            attribValue += version + ",";
                                        }
                                        else if (upgradeVersion != null && upgradeVersion > Package.InstalledVersion)
                                        {
                                            attribValue += version + ",";
                                        }
                                        break;
                                }

                            }
                            attribValue = attribValue.TrimEnd(',');
                        }
                    }
                    eventMessage.Attributes.Add(attribName, attribValue);
                }
            }
            return eventMessage;
        }

        public bool Skipped { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether the Installer supports Manifest only installs
        /// </summary>
        /// <value>A Boolean</value>
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
        /// -----------------------------------------------------------------------------
        public string Type { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Version of the Component
        /// </summary>
        /// <value>A System.Version</value>
        /// -----------------------------------------------------------------------------
        public Version Version { get; set; }

        public abstract void Commit();

        public abstract void Install();

        public abstract void ReadManifest(XPathNavigator manifestNav);

        public abstract void Rollback();

        public abstract void UnInstall();
    }
}
