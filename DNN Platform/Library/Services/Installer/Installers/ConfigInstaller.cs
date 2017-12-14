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
using System.IO;
using System.Xml;
using System.Xml.XPath;

using DotNetNuke.Common.Utilities;

#endregion

namespace DotNetNuke.Services.Installer.Installers
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ConfigInstaller installs Config changes
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class ConfigInstaller : ComponentInstallerBase
    {
		#region "Private Members"

        private string _FileName = Null.NullString;
        private string _InstallConfig = Null.NullString;
        private XmlDocument _TargetConfig;
        private InstallFile _TargetFile;
        private string _UnInstallConfig = Null.NullString;
        private string _UninstallFileName = Null.NullString;
        private XmlMerge _xmlMerge;

		#endregion

		#region "Protected Properties"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Install config changes
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string InstallConfig
        {
            get
            {
                return _InstallConfig;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Target Config XmlDocument
        /// </summary>
        /// <value>An XmlDocument</value>
        /// -----------------------------------------------------------------------------
        public XmlDocument TargetConfig
        {
            get
            {
                return _TargetConfig;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Target Config file to change
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public InstallFile TargetFile
        {
            get
            {
                return _TargetFile;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the UnInstall config changes
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string UnInstallConfig
        {
            get
            {
                return _UnInstallConfig;
            }
        }
		
		#endregion

		#region "Public Methods"


        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Commit method finalises the Install and commits any pending changes.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void Commit()
        {
            try
            {
                if (string.IsNullOrEmpty(_FileName) && _xmlMerge.ConfigUpdateChangedNodes)
                {
                    //Save the XmlDocument
                    Config.Save(TargetConfig, TargetFile.FullName);
                    Log.AddInfo(Util.CONFIG_Committed + " - " + TargetFile.Name);
                }
                else
                {
                    _xmlMerge.SavePendingConfigs();
                    foreach (var key in _xmlMerge.PendingDocuments.Keys)
                    {
                        Log.AddInfo(Util.CONFIG_Committed + " - " + key);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AddFailure(ex);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Install method installs the config component
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void Install()
        {
            try
            {
                if (string.IsNullOrEmpty(_FileName))
                {
					//First backup the config file
                    Util.BackupFile(TargetFile, PhysicalSitePath, Log);

                    //Create an XmlDocument for the config file
                    _TargetConfig = new XmlDocument();
                    TargetConfig.Load(Path.Combine(PhysicalSitePath, TargetFile.FullName));

                    //Create XmlMerge instance from InstallConfig source
                    _xmlMerge = new XmlMerge(new StringReader(InstallConfig), Package.Version.ToString(), Package.Name);

                    //Update the Config file - Note that this method does not save the file - we will save it in Commit
                    _xmlMerge.UpdateConfig(TargetConfig);
                    Completed = true;
                    Log.AddInfo(Util.CONFIG_Updated + " - " + TargetFile.Name);
                }
                else
                {
					//Process external file
                    string strConfigFile = Path.Combine(Package.InstallerInfo.TempInstallFolder, _FileName);
                    if (File.Exists(strConfigFile))
                    {
						//Create XmlMerge instance from config file source
                        using (var stream = File.OpenText(strConfigFile))
                        {
                            _xmlMerge = new XmlMerge(stream, Package.Version.ToString(3), Package.Name + " Install");

                            //Process merge
                            _xmlMerge.UpdateConfigs(false);
                        }

                        Completed = true;
                        Log.AddInfo(Util.CONFIG_Updated);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AddFailure(Util.EXCEPTION + " - " + ex.Message);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The ReadManifest method reads the manifest file for the config compoent.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void ReadManifest(XPathNavigator manifestNav)
        {
            _FileName = Util.ReadAttribute(manifestNav, "fileName");
            _UninstallFileName = Util.ReadAttribute(manifestNav, "unInstallFileName");

            if (string.IsNullOrEmpty(_FileName))
            {
                XPathNavigator nav = manifestNav.SelectSingleNode("config");

                //Get the name of the target config file to update
                XPathNavigator nodeNav = nav.SelectSingleNode("configFile");
                string targetFileName = nodeNav.Value;
                if (!string.IsNullOrEmpty(targetFileName))
                {
                    _TargetFile = new InstallFile(targetFileName, "", Package.InstallerInfo);
                }
                //Get the Install config changes
                nodeNav = nav.SelectSingleNode("install");
                _InstallConfig = nodeNav.InnerXml;

                //Get the UnInstall config changes
                nodeNav = nav.SelectSingleNode("uninstall");
                _UnInstallConfig = nodeNav.InnerXml;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Rollback method undoes the installation of the file component in the event 
        /// that one of the other components fails
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void Rollback()
        {
			//Do nothing as the changes are all in memory
            Log.AddInfo(Util.CONFIG_RolledBack + " - " + TargetFile.Name);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The UnInstall method uninstalls the config component
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void UnInstall()
        {
            if (string.IsNullOrEmpty(_UninstallFileName))
            {
                if (!string.IsNullOrEmpty(UnInstallConfig))
                {
                    //Create an XmlDocument for the config file
                    _TargetConfig = new XmlDocument();
                    TargetConfig.Load(Path.Combine(PhysicalSitePath, TargetFile.FullName));

                    //Create XmlMerge instance from UnInstallConfig source
                    var merge = new XmlMerge(new StringReader(UnInstallConfig), Package.Version.ToString(), Package.Name);

                    //Update the Config file - Note that this method does save the file
                    merge.UpdateConfig(TargetConfig, TargetFile.FullName);
                }
            }
            else
            {
				//Process external file
                string strConfigFile = Path.Combine(Package.InstallerInfo.TempInstallFolder, _UninstallFileName);
                if (File.Exists(strConfigFile))
                {
					//Create XmlMerge instance from config file source
                    StreamReader stream = File.OpenText(strConfigFile);
                    var merge = new XmlMerge(stream, Package.Version.ToString(3), Package.Name + " UnInstall");

                    //Process merge
                    merge.UpdateConfigs();

                    //Close stream
                    stream.Close();
                }
            }
        }
		
		#endregion
    }
}
