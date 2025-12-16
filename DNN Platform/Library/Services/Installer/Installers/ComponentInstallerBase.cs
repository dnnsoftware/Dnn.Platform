// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Installers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Xml.XPath;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.EventQueue;
    using DotNetNuke.Services.Installer.Log;
    using DotNetNuke.Services.Installer.Packages;

    /// <summary>The ComponentInstallerBase is a base class for all Component Installers.</summary>
    public abstract class ComponentInstallerBase
    {
        /// <summary>Initializes a new instance of the <see cref="ComponentInstallerBase"/> class.</summary>
        protected ComponentInstallerBase()
        {
            this.Completed = Null.NullBoolean;
        }

        /// <summary>Gets a list of allowable file extensions (in addition to the Host's List).</summary>
        public virtual string AllowableFiles
        {
            get
            {
                return Null.NullString;
            }
        }

        /// <summary>Gets the InstallMode.</summary>
        public InstallMode InstallMode
        {
            get
            {
                return this.Package.InstallMode;
            }
        }

        /// <summary>Gets the Logger.</summary>
        public Logger Log
        {
            get
            {
                return this.Package.Log;
            }
        }

        /// <summary>Gets a Dictionary of Files that are included in the Package.</summary>
        public Dictionary<string, InstallFile> PackageFiles
        {
            get
            {
                return this.Package.Files;
            }
        }

        /// <summary>Gets the Physical Path to the root of the Site (e.g. <c>"D:\Websites\DotNetNuke"</c>).</summary>
        public string PhysicalSitePath
        {
            get
            {
                return this.Package.InstallerInfo.PhysicalSitePath;
            }
        }

        /// <summary>Gets a value indicating whether the Installer supports Manifest only installs.</summary>
        public virtual bool SupportsManifestOnlyInstall
        {
            get
            {
                return true;
            }
        }

        /// <summary>Gets or sets a value indicating whether Completed flag is set.</summary>
        public bool Completed { get; set; }

        /// <summary>Gets or sets the associated Package.</summary>
        public PackageInfo Package { get; set; }

        public bool Skipped { get; set; }

        /// <summary>Gets or sets the Type of the component.</summary>
        public string Type { get; set; }

        /// <summary>Gets or sets the Version of the Component.</summary>
        public Version Version { get; set; }

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
                    Body = string.Empty,
                    ProcessorType = Util.ReadElement(eventMessageNav, "processorType", this.Log, Util.EVENTMESSAGE_TypeMissing),
                    ProcessorCommand = Util.ReadElement(eventMessageNav, "processorCommand", this.Log, Util.EVENTMESSAGE_CommandMissing),
                };
                foreach (XPathNavigator attributeNav in eventMessageNav.Select("attributes/*"))
                {
                    var attribName = attributeNav.Name;
                    var attribValue = attributeNav.Value;
                    if (attribName == "upgradeVersionsList")
                    {
                        if (!string.IsNullOrEmpty(attribValue))
                        {
                            string[] upgradeVersions = attribValue.Split(',');
                            attribValue = string.Empty;
                            foreach (string version in upgradeVersions)
                            {
                                switch (version.ToLowerInvariant())
                                {
                                    case "install":
                                        if (this.Package.InstalledVersion == new Version(0, 0, 0))
                                        {
                                            attribValue += version + ",";
                                        }

                                        break;
                                    case "upgrade":
                                        if (this.Package.InstalledVersion > new Version(0, 0, 0))
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
                                            this.Log.AddWarning(string.Format(CultureInfo.InvariantCulture, Util.MODULE_InvalidVersion, version));
                                        }

                                        if (upgradeVersion != null && (Globals.Status == Globals.UpgradeStatus.Install))
                                        {
                                            // To allow when fresh installing or installresources
                                            attribValue += version + ",";
                                        }
                                        else if (upgradeVersion != null && upgradeVersion > this.Package.InstalledVersion)
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

        public abstract void Commit();

        public abstract void Install();

        public abstract void ReadManifest(XPathNavigator manifestNav);

        public abstract void Rollback();

        public abstract void UnInstall();
    }
}
