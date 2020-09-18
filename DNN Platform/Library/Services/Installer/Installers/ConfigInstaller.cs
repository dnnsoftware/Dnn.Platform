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

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ConfigInstaller installs Config changes.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class ConfigInstaller : ComponentInstallerBase
    {
        private string _FileName = Null.NullString;
        private string _InstallConfig = Null.NullString;
        private XmlDocument _TargetConfig;
        private InstallFile _TargetFile;
        private string _UnInstallConfig = Null.NullString;
        private string _UninstallFileName = Null.NullString;
        private XmlMerge _xmlMerge;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Install config changes.
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string InstallConfig
        {
            get
            {
                return this._InstallConfig;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Target Config XmlDocument.
        /// </summary>
        /// <value>An XmlDocument.</value>
        /// -----------------------------------------------------------------------------
        public XmlDocument TargetConfig
        {
            get
            {
                return this._TargetConfig;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Target Config file to change.
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public InstallFile TargetFile
        {
            get
            {
                return this._TargetFile;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the UnInstall config changes.
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string UnInstallConfig
        {
            get
            {
                return this._UnInstallConfig;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Commit method finalises the Install and commits any pending changes.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void Commit()
        {
            try
            {
                if (string.IsNullOrEmpty(this._FileName) && this._xmlMerge.ConfigUpdateChangedNodes)
                {
                    // Save the XmlDocument
                    Config.Save(this.TargetConfig, this.TargetFile.FullName);
                    this.Log.AddInfo(Util.CONFIG_Committed + " - " + this.TargetFile.Name);
                }
                else
                {
                    this._xmlMerge.SavePendingConfigs();
                    foreach (var key in this._xmlMerge.PendingDocuments.Keys)
                    {
                        this.Log.AddInfo(Util.CONFIG_Committed + " - " + key);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(ex);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Install method installs the config component.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void Install()
        {
            try
            {
                if (string.IsNullOrEmpty(this._FileName))
                {
                    // First backup the config file
                    Util.BackupFile(this.TargetFile, this.PhysicalSitePath, this.Log);

                    // Create an XmlDocument for the config file
                    this._TargetConfig = new XmlDocument { XmlResolver = null };
                    this.TargetConfig.Load(Path.Combine(this.PhysicalSitePath, this.TargetFile.FullName));

                    // Create XmlMerge instance from InstallConfig source
                    this._xmlMerge = new XmlMerge(new StringReader(this.InstallConfig), this.Package.Version.ToString(), this.Package.Name);

                    // Update the Config file - Note that this method does not save the file - we will save it in Commit
                    this._xmlMerge.UpdateConfig(this.TargetConfig);
                    this.Completed = true;
                    this.Log.AddInfo(Util.CONFIG_Updated + " - " + this.TargetFile.Name);
                }
                else
                {
                    // Process external file
                    string strConfigFile = Path.Combine(this.Package.InstallerInfo.TempInstallFolder, this._FileName);
                    if (File.Exists(strConfigFile))
                    {
                        // Create XmlMerge instance from config file source
                        using (var stream = File.OpenText(strConfigFile))
                        {
                            this._xmlMerge = new XmlMerge(stream, this.Package.Version.ToString(3), this.Package.Name + " Install");

                            // Process merge
                            this._xmlMerge.UpdateConfigs(false);
                        }

                        this.Completed = true;
                        this.Log.AddInfo(Util.CONFIG_Updated);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(Util.EXCEPTION + " - " + ex.Message);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The ReadManifest method reads the manifest file for the config compoent.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void ReadManifest(XPathNavigator manifestNav)
        {
            this._FileName = Util.ReadAttribute(manifestNav, "fileName");
            this._UninstallFileName = Util.ReadAttribute(manifestNav, "unInstallFileName");

            if (string.IsNullOrEmpty(this._FileName))
            {
                XPathNavigator nav = manifestNav.SelectSingleNode("config");

                // Get the name of the target config file to update
                XPathNavigator nodeNav = nav.SelectSingleNode("configFile");
                string targetFileName = nodeNav.Value;
                if (!string.IsNullOrEmpty(targetFileName))
                {
                    this._TargetFile = new InstallFile(targetFileName, string.Empty, this.Package.InstallerInfo);
                }

                // Get the Install config changes
                nodeNav = nav.SelectSingleNode("install");
                this._InstallConfig = nodeNav.InnerXml;

                // Get the UnInstall config changes
                nodeNav = nav.SelectSingleNode("uninstall");
                this._UnInstallConfig = nodeNav.InnerXml;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Rollback method undoes the installation of the file component in the event
        /// that one of the other components fails.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void Rollback()
        {
            // Do nothing as the changes are all in memory
            this.Log.AddInfo(Util.CONFIG_RolledBack + " - " + this.TargetFile.Name);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The UnInstall method uninstalls the config component.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void UnInstall()
        {
            if (string.IsNullOrEmpty(this._UninstallFileName))
            {
                if (!string.IsNullOrEmpty(this.UnInstallConfig))
                {
                    // Create an XmlDocument for the config file
                    this._TargetConfig = new XmlDocument { XmlResolver = null };
                    this.TargetConfig.Load(Path.Combine(this.PhysicalSitePath, this.TargetFile.FullName));

                    // Create XmlMerge instance from UnInstallConfig source
                    var merge = new XmlMerge(new StringReader(this.UnInstallConfig), this.Package.Version.ToString(), this.Package.Name);

                    // Update the Config file - Note that this method does save the file
                    merge.UpdateConfig(this.TargetConfig, this.TargetFile.FullName);
                }
            }
            else
            {
                // Process external file
                string strConfigFile = Path.Combine(this.Package.InstallerInfo.TempInstallFolder, this._UninstallFileName);
                if (File.Exists(strConfigFile))
                {
                    // Create XmlMerge instance from config file source
                    StreamReader stream = File.OpenText(strConfigFile);
                    var merge = new XmlMerge(stream, this.Package.Version.ToString(3), this.Package.Name + " UnInstall");

                    // Process merge
                    merge.UpdateConfigs();

                    // Close stream
                    stream.Close();
                }
            }
        }
    }
}
