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
using System.Collections;
using System.IO;
using System.Xml.XPath;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.UI.Skins;

#endregion

namespace DotNetNuke.Services.Installer.Installers
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The SkinInstaller installs Skin Components to a DotNetNuke site
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class SkinInstaller : FileInstaller
    {
		#region "Private Members"

        private readonly ArrayList _SkinFiles = new ArrayList();

        private SkinPackageInfo SkinPackage;
        private SkinPackageInfo TempSkinPackage;
        private string _SkinName = Null.NullString;
		
		#endregion

		#region "Protected Properties"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Collection Node ("skinFiles")
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        protected override string CollectionNodeName
        {
            get
            {
                return "skinFiles";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Item Node ("skinFile")
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        protected override string ItemNodeName
        {
            get
            {
                return "skinFile";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the PhysicalBasePath for the skin files
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        protected override string PhysicalBasePath
        {
            get
            {
                string _PhysicalBasePath = RootPath + SkinRoot + "\\" + SkinPackage.SkinName;
                if (!_PhysicalBasePath.EndsWith("\\"))
                {
                    _PhysicalBasePath += "\\";
                }
                return _PhysicalBasePath.Replace("/", "\\");
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the root folder for the Skin
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        protected string RootPath
        {
            get
            {
                string _RootPath = Null.NullString;
                if (Package.InstallerInfo.PortalID == Null.NullInteger && Package.PortalID == Null.NullInteger)
                {
                    _RootPath = Globals.HostMapPath;
                }
                else
                {
                    _RootPath = PortalController.Instance.GetCurrentPortalSettings().HomeSystemDirectoryMapPath;
                }
                return _RootPath;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the collection of Skin Files
        /// </summary>
        /// <value>A List(Of InstallFile)</value>
        /// -----------------------------------------------------------------------------
        protected ArrayList SkinFiles
        {
            get
            {
                return _SkinFiles;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the SkinName Node ("skinName")
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        protected virtual string SkinNameNodeName
        {
            get
            {
                return "skinName";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the RootName of the Skin
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        protected virtual string SkinRoot
        {
            get
            {
                return SkinController.RootSkin;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Type of the Skin
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        protected virtual string SkinType
        {
            get
            {
                return "Skin";
            }
        }
		
		#endregion

		#region "Public Properties"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a list of allowable file extensions (in addition to the Host's List)
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public override string AllowableFiles
        {
            get
            {
                return "ascx, html, htm, css, xml, js, resx, jpg, jpeg, gif, png";
            }
        }
		
		#endregion

		#region "Private Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The DeleteSkinPackage method deletes the Skin Package
        /// from the data Store.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void DeleteSkinPackage()
        {
            try
            {
				//Attempt to get the Authentication Service
                SkinPackageInfo skinPackage = SkinController.GetSkinByPackageID(Package.PackageID);
                if (skinPackage != null)
                {
                    SkinController.DeleteSkinPackage(skinPackage);
                }
                Log.AddInfo(string.Format(Util.SKIN_UnRegistered, skinPackage.SkinName));
            }
            catch (Exception ex)
            {
                Log.AddFailure(ex);
            }
        }
		
		#endregion

		#region "Protected Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The ProcessFile method determines what to do with parsed "file" node
        /// </summary>
        /// <param name="file">The file represented by the node</param>
        /// <param name="nav">The XPathNavigator representing the node</param>
        /// -----------------------------------------------------------------------------
        protected override void ProcessFile(InstallFile file, XPathNavigator nav)
        {
            switch (file.Extension)
            {
                case "htm":
                case "html":
                case "ascx":
                case "css":
                    if (file.Path.ToLower().IndexOf(Globals.glbAboutPage.ToLower()) < 0)
                    {
                        SkinFiles.Add(PhysicalBasePath + file.FullName);
                    }
                    break;
            }
            
			//Call base method to set up for file processing
			base.ProcessFile(file, nav);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The ReadCustomManifest method reads the custom manifest items
        /// </summary>
        /// <param name="nav">The XPathNavigator representing the node</param>
        /// -----------------------------------------------------------------------------
        protected override void ReadCustomManifest(XPathNavigator nav)
        {
            SkinPackage = new SkinPackageInfo();
            SkinPackage.PortalID = Package.PortalID;

            //Get the Skin name
            SkinPackage.SkinName = Util.ReadElement(nav, SkinNameNodeName);

            //Call base class
            base.ReadCustomManifest(nav);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The UnInstallFile method unInstalls a single file.
        /// </summary>
        /// <param name="unInstallFile">The InstallFile to unInstall.</param>
        /// -----------------------------------------------------------------------------
        protected override void UnInstallFile(InstallFile unInstallFile)
        {
			//Uninstall file
            base.UnInstallFile(unInstallFile);

            if (unInstallFile.Extension == "htm" || unInstallFile.Extension == "html")
            {
				//Try to remove "processed file"
                string fileName = unInstallFile.FullName;
                fileName = fileName.Replace(Path.GetExtension(fileName), ".ascx");
                Util.DeleteFile(fileName, PhysicalBasePath, Log);
            }
        }
		
		#endregion

		#region "Public Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Install method installs the skin component
        /// </summary>
        public override void Install()
        {
            bool bAdd = Null.NullBoolean;
            try
            {
				//Attempt to get the Skin Package
                TempSkinPackage = SkinController.GetSkinPackage(SkinPackage.PortalID, SkinPackage.SkinName, SkinType);
                if (TempSkinPackage == null)
                {
                    bAdd = true;
                    SkinPackage.PackageID = Package.PackageID;
                }
                else
                {
                    SkinPackage.SkinPackageID = TempSkinPackage.SkinPackageID;
                    if (TempSkinPackage.PackageID != Package.PackageID)
                    {
                        Completed = false;
                        Log.AddFailure(Util.SKIN_Installed);
                        return;
                    }
                    else
                    {
                        SkinPackage.PackageID = TempSkinPackage.PackageID;
                    }
                }
                SkinPackage.SkinType = SkinType;
                if (bAdd)
                {
					//Add new skin package
                    SkinPackage.SkinPackageID = SkinController.AddSkinPackage(SkinPackage);
                }
                else
                {
					//Update skin package
                    SkinController.UpdateSkinPackage(SkinPackage);
                }
                Log.AddInfo(string.Format(Util.SKIN_Registered, SkinPackage.SkinName));

                //install (copy the files) by calling the base class
                base.Install();

                //process the list of skin files
                if (SkinFiles.Count > 0)
                {
                    Log.StartJob(Util.SKIN_BeginProcessing);
                    string strMessage = Null.NullString;
                    var NewSkin = new SkinFileProcessor(RootPath, SkinRoot, SkinPackage.SkinName);
                    foreach (string skinFile in SkinFiles)
                    {
                        strMessage += NewSkin.ProcessFile(skinFile, SkinParser.Portable);
                        skinFile.Replace(Globals.HostMapPath + "\\", "[G]");
                        switch (Path.GetExtension(skinFile))
                        {
                            case ".htm":
                                SkinController.AddSkin(SkinPackage.SkinPackageID, skinFile.Replace("htm", "ascx"));
                                break;
                            case ".html":
                                SkinController.AddSkin(SkinPackage.SkinPackageID, skinFile.Replace("html", "ascx"));
                                break;
                            case ".ascx":
                                SkinController.AddSkin(SkinPackage.SkinPackageID, skinFile);
                                break;
                        }
                    }
                    Array arrMessage = strMessage.Split(new[] {"<br />"}, StringSplitOptions.None);
                    foreach (string strRow in arrMessage)
                    {
                        Log.AddInfo(HtmlUtils.StripTags(strRow, true));
                    }
                    Log.EndJob(Util.SKIN_EndProcessing);
                }
                Completed = true;
            }
            catch (Exception ex)
            {
                Log.AddFailure(ex);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Rollback method undoes the installation of the component in the event 
        /// that one of the other components fails
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void Rollback()
        {
			//If Temp Skin exists then we need to update the DataStore with this 
            if (TempSkinPackage == null)
            {
				//No Temp Skin - Delete newly added Skin
                DeleteSkinPackage();
            }
            else
            {
				//Temp Skin - Rollback to Temp
                SkinController.UpdateSkinPackage(TempSkinPackage);
            }
            
			//Call base class to prcoess files
			base.Rollback();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The UnInstall method uninstalls the skin component
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void UnInstall()
        {
            DeleteSkinPackage();

            //Call base class to prcoess files
            base.UnInstall();
        }
		
		#endregion
    }
}
