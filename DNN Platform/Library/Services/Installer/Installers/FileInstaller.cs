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
using System.IO;
using System.Xml.XPath;

using DotNetNuke.Common.Utilities;

#endregion

namespace DotNetNuke.Services.Installer.Installers
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The FileInstaller installs File Components to a DotNetNuke site
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	07/24/2007  created
    /// </history>
    /// -----------------------------------------------------------------------------
    public class FileInstaller : ComponentInstallerBase
    {
		#region Private Members
		
        private readonly List<InstallFile> _Files = new List<InstallFile>();
        private bool _DeleteFiles = Null.NullBoolean;

		#endregion

		#region Protected Properties

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the BasePath for the files
        /// </summary>
        /// <remarks>The Base Path is relative to the WebRoot</remarks>
        /// <value>A String</value>
        /// <history>
        /// 	[cnurse]	07/25/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected string BasePath { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Collection Node ("files")
        /// </summary>
        /// <value>A String</value>
        /// <history>
        /// 	[cnurse]	08/07/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected virtual string CollectionNodeName
        {
            get
            {
                return "files";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a Dictionary of Files that are included in this component
        /// </summary>
        /// <value>A Dictionary(Of String, InstallFile)</value>
        /// <history>
        /// 	[cnurse]	07/25/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected List<InstallFile> Files
        {
            get
            {
                return _Files;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the default Path for the file - if not present in the manifest
        /// </summary>
        /// <value>A String</value>
        /// <history>
        /// 	[cnurse]	08/10/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected virtual string DefaultPath
        {
            get
            {
                return Null.NullString;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Item Node ("file")
        /// </summary>
        /// <value>A String</value>
        /// <history>
        /// 	[cnurse]	08/07/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected virtual string ItemNodeName
        {
            get
            {
                return "file";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the PhysicalBasePath for the files
        /// </summary>
        /// <value>A String</value>
        /// <history>
        /// 	[cnurse]	07/25/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected virtual string PhysicalBasePath
        {
            get
            {
                string _PhysicalBasePath = PhysicalSitePath + "\\" + BasePath;
                if (!_PhysicalBasePath.EndsWith("\\"))
                {
                    _PhysicalBasePath += "\\";
                }
                return _PhysicalBasePath.Replace("/", "\\");
            }
        }
		
		#endregion

		#region Public Properties

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets whether the Packages files are deleted when uninstalling the
        /// package
        /// </summary>
        /// <value>A Boolean value</value>
        /// <history>
        /// 	[cnurse]	01/31/2008  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public bool DeleteFiles
        {
            get
            {
                return _DeleteFiles;
            }
            set
            {
                _DeleteFiles = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether the Installer supports Manifest only installs
        /// </summary>
        /// <value>A Boolean</value>
        /// <history>
        /// 	[cnurse]	02/29/2008  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public override bool SupportsManifestOnlyInstall
        {
            get
            {
                return Null.NullBoolean;
            }
        }
		
		#endregion

		#region Protected Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The CommitFile method commits a single file.
        /// </summary>
        /// <param name="insFile">The InstallFile to commit</param>
        /// <history>
        /// 	[cnurse]	08/24/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected virtual void CommitFile(InstallFile insFile)
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The DeleteFile method deletes a single file.
        /// </summary>
        /// <param name="insFile">The InstallFile to delete</param>
        /// <history>
        /// 	[cnurse]	08/01/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected virtual void DeleteFile(InstallFile insFile)
        {
            if (DeleteFiles)
            {
                Util.DeleteFile(insFile, PhysicalBasePath, Log);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The InstallFile method installs a single file.
        /// </summary>
        /// <param name="insFile">The InstallFile to install</param>
        /// <history>
        /// 	[cnurse]	08/01/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected virtual bool InstallFile(InstallFile insFile)
        {
            try
            {
				//Check the White Lists
                if ((Package.InstallerInfo.IgnoreWhiteList || Util.IsFileValid(insFile, Package.InstallerInfo.AllowableFiles)))
                {
					//Install File
                    if (File.Exists(PhysicalBasePath + insFile.FullName))
                    {
                        Util.BackupFile(insFile, PhysicalBasePath, Log);
                    }
                    
					//Copy file from temp location
					Util.CopyFile(insFile, PhysicalBasePath, Log);
                    return true;
                }
                else
                {
                    Log.AddFailure(string.Format(Util.FILE_NotAllowed, insFile.FullName));
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.AddFailure(ex);
                return false;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a flag that determines what type of file this installer supports
        /// </summary>
        /// <param name="type">The type of file being processed</param>
        /// <history>
        /// 	[cnurse]	08/07/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected virtual bool IsCorrectType(InstallFileType type)
        {
            return true;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The ProcessFile method determines what to do with parsed "file" node
        /// </summary>
        /// <param name="file">The file represented by the node</param>
        /// <param name="nav">The XPathNavigator representing the node</param>
        /// <history>
        /// 	[cnurse]	08/07/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected virtual void ProcessFile(InstallFile file, XPathNavigator nav)
        {
            if (file != null && IsCorrectType(file.Type))
            {
                Files.Add(file);

                //Add to the
                Package.InstallerInfo.Files[file.FullName.ToLower()] = file;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The ReadCustomManifest method reads the custom manifest items (that subclasses
        /// of FileInstaller may need)
        /// </summary>
        /// <param name="nav">The XPathNavigator representing the node</param>
        /// <history>
        /// 	[cnurse]	08/22/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected virtual void ReadCustomManifest(XPathNavigator nav)
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The ReadManifestItem method reads a single node
        /// </summary>
        /// <param name="nav">The XPathNavigator representing the node</param>
        /// <param name="checkFileExists">Flag that determines whether a check should be made</param>
        /// <history>
        /// 	[cnurse]	08/07/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected virtual InstallFile ReadManifestItem(XPathNavigator nav, bool checkFileExists)
        {
            string fileName = Null.NullString;

            //Get the path
            XPathNavigator pathNav = nav.SelectSingleNode("path");
            if (pathNav == null)
            {
                fileName = DefaultPath;
            }
            else
            {
                fileName = pathNav.Value + "\\";
            }
			
            //Get the name
            XPathNavigator nameNav = nav.SelectSingleNode("name");
            if (nameNav != null)
            {
                fileName += nameNav.Value;
            }
            
			//Get the sourceFileName
			string sourceFileName = Util.ReadElement(nav, "sourceFileName");
            var file = new InstallFile(fileName, sourceFileName, Package.InstallerInfo);
            if ((!string.IsNullOrEmpty(BasePath)) && (BasePath.ToLowerInvariant().StartsWith("app_code") && file.Type == InstallFileType.Other))
            {
                file.Type = InstallFileType.AppCode;
            }
            if (file != null)
            {
                //Set the Version
				string strVersion = XmlUtils.GetNodeValue(nav, "version");
                if (!string.IsNullOrEmpty(strVersion))
                {
                    file.SetVersion(new Version(strVersion));
                }
                else
                {
                    file.SetVersion(Package.Version);
                }
                
				//Set the Action
				string strAction = XmlUtils.GetAttributeValue(nav, "action");
                if (!string.IsNullOrEmpty(strAction))
                {
                    file.Action = strAction;
                }
                if (InstallMode == InstallMode.Install && checkFileExists && file.Action != "UnRegister")
                {
                    if (File.Exists(file.TempFileName))
                    {
                        Log.AddInfo(string.Format(Util.FILE_Found, file.Path, file.Name));
                    }
                    else
                    {
                        Log.AddFailure(Util.FILE_NotFound + " - " + file.TempFileName);
                    }
                }
            }
            return file;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The RollbackFile method rolls back the install of a single file.
        /// </summary>
        /// <remarks>For new installs this removes the added file.  For upgrades it restores the
        /// backup file created during install</remarks>
        /// <param name="installFile">The InstallFile to commit</param>
        /// <history>
        /// 	[cnurse]	08/01/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected virtual void RollbackFile(InstallFile installFile)
        {
            if (File.Exists(installFile.BackupFileName))
            {
                Util.RestoreFile(installFile, PhysicalBasePath, Log);
            }
            else
            {
                DeleteFile(installFile);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The UnInstallFile method unInstalls a single file.
        /// </summary>
        /// <param name="unInstallFile">The InstallFile to unInstall.</param>
        /// <history>
        /// 	[cnurse]	01/07/2008  created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected virtual void UnInstallFile(InstallFile unInstallFile)
        {
            DeleteFile(unInstallFile);
        }
		
		#endregion

		#region Public Methods


        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Commit method finalises the Install and commits any pending changes.
        /// </summary>
        /// <remarks>In the case of Files this is not neccessary</remarks>
        /// <history>
        /// 	[cnurse]	08/01/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public override void Commit()
        {
            try
            {
                foreach (InstallFile file in Files)
                {
                    CommitFile(file);
                }
                Completed = true;
            }
            catch (Exception ex)
            {
                Log.AddFailure(Util.EXCEPTION + " - " + ex.Message);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Install method installs the file component
        /// </summary>
        /// <history>
        /// 	[cnurse]	07/25/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public override void Install()
        {
            try
            {
                bool bSuccess = true;
                foreach (InstallFile file in Files)
                {
                    bSuccess = InstallFile(file);
                    if (!bSuccess)
                    {
                        break;
                    }
                }
                Completed = bSuccess;
            }
            catch (Exception ex)
            {
                Log.AddFailure(Util.EXCEPTION + " - " + ex.Message);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The ReadManifest method reads the manifest file for the file compoent.
        /// </summary>
        /// <history>
        /// 	[cnurse]	07/25/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public override void ReadManifest(XPathNavigator manifestNav)
        {
            XPathNavigator rootNav = manifestNav.SelectSingleNode(CollectionNodeName);
            if (rootNav != null)
            {
                XPathNavigator baseNav = rootNav.SelectSingleNode("basePath");
                if (baseNav != null)
                {
                    BasePath = baseNav.Value;
                }
                ReadCustomManifest(rootNav);
                foreach (XPathNavigator nav in rootNav.Select(ItemNodeName))
                {
                    ProcessFile(ReadManifestItem(nav, true), nav);
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Rollback method undoes the installation of the file component in the event 
        /// that one of the other components fails
        /// </summary>
        /// <history>
        /// 	[cnurse]	07/31/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public override void Rollback()
        {
            try
            {
                foreach (InstallFile file in Files)
                {
                    RollbackFile(file);
                }
                Completed = true;
            }
            catch (Exception ex)
            {
                Log.AddFailure(Util.EXCEPTION + " - " + ex.Message);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The UnInstall method uninstalls the file component
        /// </summary>
        /// <history>
        /// 	[cnurse]	07/31/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public override void UnInstall()
        {
            try
            {
                foreach (InstallFile file in Files)
                {
                    UnInstallFile(file);
                }
                Completed = true;
            }
            catch (Exception ex)
            {
                Log.AddFailure(Util.EXCEPTION + " - " + ex.Message);
            }
        }
		
		#endregion
    }
}
