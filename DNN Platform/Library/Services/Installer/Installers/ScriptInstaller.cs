// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Installers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.XPath;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Framework.Providers;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ScriptInstaller installs Script Components to a DotNetNuke site.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class ScriptInstaller : FileInstaller
    {
        private readonly SortedList<Version, InstallFile> _installScripts = new SortedList<Version, InstallFile>();
        private readonly SortedList<Version, InstallFile> _unInstallScripts = new SortedList<Version, InstallFile>();
        private InstallFile _installScript;
        private InstallFile _upgradeScript;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a list of allowable file extensions (in addition to the Host's List).
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public override string AllowableFiles
        {
            get
            {
                return "*dataprovider, sql";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the base Install Script (if present).
        /// </summary>
        /// <value>An InstallFile.</value>
        /// -----------------------------------------------------------------------------
        protected InstallFile InstallScript
        {
            get
            {
                return this._installScript;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the collection of Install Scripts.
        /// </summary>
        /// <value>A List(Of InstallFile).</value>
        /// -----------------------------------------------------------------------------
        protected SortedList<Version, InstallFile> InstallScripts
        {
            get
            {
                return this._installScripts;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the collection of UnInstall Scripts.
        /// </summary>
        /// <value>A List(Of InstallFile).</value>
        /// -----------------------------------------------------------------------------
        protected SortedList<Version, InstallFile> UnInstallScripts
        {
            get
            {
                return this._unInstallScripts;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Collection Node ("scripts").
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        protected override string CollectionNodeName
        {
            get
            {
                return "scripts";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Item Node ("script").
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        protected override string ItemNodeName
        {
            get
            {
                return "script";
            }
        }

        protected ProviderConfiguration ProviderConfiguration
        {
            get
            {
                return ProviderConfiguration.GetProviderConfiguration("data");
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Upgrade Script (if present).
        /// </summary>
        /// <value>An InstallFile.</value>
        /// -----------------------------------------------------------------------------
        protected InstallFile UpgradeScript
        {
            get
            {
                return this._upgradeScript;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Commit method finalises the Install and commits any pending changes.
        /// </summary>
        /// <remarks>In the case of Files this is not neccessary.</remarks>
        /// -----------------------------------------------------------------------------
        public override void Commit()
        {
            base.Commit();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Install method installs the script component.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void Install()
        {
            this.Log.AddInfo(Util.SQL_Begin);
            try
            {
                bool bSuccess = true;
                Version installedVersion = this.Package.InstalledVersion;

                // First process InstallScript
                if (installedVersion == new Version(0, 0, 0))
                {
                    if (this.InstallScript != null)
                    {
                        bSuccess = this.InstallScriptFile(this.InstallScript);
                        installedVersion = this.InstallScript.Version;
                    }
                }

                // Then process remain Install/Upgrade Scripts
                if (bSuccess)
                {
                    foreach (InstallFile file in this.InstallScripts.Values)
                    {
                        if (file.Version > installedVersion)
                        {
                            bSuccess = this.InstallScriptFile(file);
                            if (!bSuccess)
                            {
                                break;
                            }
                        }
                    }
                }

                // Next process UpgradeScript - this script always runs if present
                if (this.UpgradeScript != null)
                {
                    bSuccess = this.InstallScriptFile(this.UpgradeScript);
                    installedVersion = this.UpgradeScript.Version;
                }

                // Then process uninstallScripts - these need to be copied but not executed
                if (bSuccess)
                {
                    foreach (InstallFile file in this.UnInstallScripts.Values)
                    {
                        bSuccess = this.InstallFile(file);
                        if (!bSuccess)
                        {
                            break;
                        }
                    }
                }

                this.Completed = bSuccess;
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(ex);
            }

            this.Log.AddInfo(Util.SQL_End);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Rollback method undoes the installation of the script component in the event
        /// that one of the other components fails.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void Rollback()
        {
            base.Rollback();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The UnInstall method uninstalls the script component.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void UnInstall()
        {
            this.Log.AddInfo(Util.SQL_BeginUnInstall);

            // Call the base method
            base.UnInstall();

            this.Log.AddInfo(Util.SQL_EndUnInstall);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a flag that determines what type of file this installer supports.
        /// </summary>
        /// <param name="type">The type of file being processed.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        protected override bool IsCorrectType(InstallFileType type)
        {
            return type == InstallFileType.Script;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The ProcessFile method determines what to do with parsed "file" node.
        /// </summary>
        /// <param name="file">The file represented by the node.</param>
        /// <param name="nav">The XPathNavigator representing the node.</param>
        /// -----------------------------------------------------------------------------
        protected override void ProcessFile(InstallFile file, XPathNavigator nav)
        {
            string type = nav.GetAttribute("type", string.Empty);
            if (file != null && this.IsCorrectType(file.Type))
            {
                if (file.Name.StartsWith("install.", StringComparison.InvariantCultureIgnoreCase))
                {
                    // This is the initial script when installing
                    this._installScript = file;
                }
                else if (file.Name.StartsWith("upgrade.", StringComparison.InvariantCultureIgnoreCase))
                {
                    this._upgradeScript = file;
                }
                else if (type.Equals("install", StringComparison.InvariantCultureIgnoreCase))
                {
                    // These are the Install/Upgrade scripts
                    this.InstallScripts[file.Version] = file;
                }
                else
                {
                    // These are the Uninstall scripts
                    this.UnInstallScripts[file.Version] = file;
                }
            }

            // Call base method to set up for file processing
            base.ProcessFile(file, nav);
        }

        protected override void UnInstallFile(InstallFile scriptFile)
        {
            // Process the file if it is an UnInstall Script
            var extension = Path.GetExtension(scriptFile.Name.ToLowerInvariant());
            if (extension != null && this.UnInstallScripts.ContainsValue(scriptFile))
            {
                string fileExtension = extension.Substring(1);
                if (scriptFile.Name.StartsWith("uninstall.", StringComparison.InvariantCultureIgnoreCase) && this.IsValidScript(fileExtension))
                {
                    // Install Script
                    this.Log.AddInfo(Util.SQL_Executing + scriptFile.Name);
                    this.ExecuteSql(scriptFile);
                }
            }

            // Call base method to delete file
            base.UnInstallFile(scriptFile);
        }

        private bool ExecuteSql(InstallFile scriptFile)
        {
            bool bSuccess = true;

            this.Log.AddInfo(string.Format(Util.SQL_BeginFile, scriptFile.Name));

            // read script file for installation
            string strScript = FileSystemUtils.ReadFile(this.PhysicalBasePath + scriptFile.FullName);

            // This check needs to be included because the unicode Byte Order mark results in an extra character at the start of the file
            // The extra character - '?' - causes an error with the database.
            if (strScript.StartsWith("?"))
            {
                strScript = strScript.Substring(1);
            }

            string strSQLExceptions = DataProvider.Instance().ExecuteScript(strScript);
            if (!string.IsNullOrEmpty(strSQLExceptions))
            {
                if (this.Package.InstallerInfo.IsLegacyMode)
                {
                    this.Log.AddWarning(string.Format(Util.SQL_Exceptions, Environment.NewLine, strSQLExceptions));
                }
                else
                {
                    this.Log.AddFailure(string.Format(Util.SQL_Exceptions, Environment.NewLine, strSQLExceptions));
                    bSuccess = false;
                }
            }

            this.Log.AddInfo(string.Format(Util.SQL_EndFile, scriptFile.Name));
            return bSuccess;
        }

        private bool IsValidScript(string fileExtension)
        {
            return this.ProviderConfiguration.DefaultProvider.Equals(fileExtension, StringComparison.InvariantCultureIgnoreCase) || fileExtension.Equals("sql", StringComparison.InvariantCultureIgnoreCase);
        }

        private bool InstallScriptFile(InstallFile scriptFile)
        {
            // Call base InstallFile method to copy file
            bool bSuccess = this.InstallFile(scriptFile);

            // Process the file if it is an Install Script
            var extension = Path.GetExtension(scriptFile.Name.ToLowerInvariant());
            if (extension != null)
            {
                string fileExtension = extension.Substring(1);
                if (bSuccess && this.IsValidScript(fileExtension))
                {
                    this.Log.AddInfo(Util.SQL_Executing + scriptFile.Name);
                    bSuccess = this.ExecuteSql(scriptFile);
                }
            }

            return bSuccess;
        }
    }
}
