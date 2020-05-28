// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;
using System.IO;
using System.Xml.XPath;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;

#endregion

namespace DotNetNuke.Services.Installer.Installers
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The SkinControlInstaller installs SkinControl (SkinObject) Components to a DotNetNuke site
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class SkinControlInstaller : ComponentInstallerBase
    {
		#region "Private Properties"

        private SkinControlInfo InstalledSkinControl;
        private SkinControlInfo SkinControl;
		
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
                return "ascx, vb, cs, js, resx, xml, vbproj, csproj, sln";
            }
        }
		
		#endregion

		#region "Private Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The DeleteSkinControl method deletes the SkinControl from the data Store.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void DeleteSkinControl()
        {
            try
            {
				//Attempt to get the SkinControl
                SkinControlInfo skinControl = SkinControlController.GetSkinControlByPackageID(Package.PackageID);
                if (skinControl != null)
                {
                    SkinControlController.DeleteSkinControl(skinControl);
                }
                Log.AddInfo(string.Format(Util.MODULE_UnRegistered, skinControl.ControlKey));
            }
            catch (Exception ex)
            {
                Log.AddFailure(ex);
            }
        }
		
		#endregion

		#region "Public Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Commit method finalises the Install and commits any pending changes.
        /// </summary>
        /// <remarks>In the case of Modules this is not neccessary</remarks>
        /// -----------------------------------------------------------------------------
        public override void Commit()
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Install method installs the Module component
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void Install()
        {
            try
            {
				//Attempt to get the SkinControl
                InstalledSkinControl = SkinControlController.GetSkinControlByKey(SkinControl.ControlKey);

                if (InstalledSkinControl != null)
                {
                    SkinControl.SkinControlID = InstalledSkinControl.SkinControlID;
                }
				
                //Save SkinControl
                SkinControl.PackageID = Package.PackageID;
                SkinControl.SkinControlID = SkinControlController.SaveSkinControl(SkinControl);

                Completed = true;
                Log.AddInfo(string.Format(Util.MODULE_Registered, SkinControl.ControlKey));
            }
            catch (Exception ex)
            {
                Log.AddFailure(ex);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The ReadManifest method reads the manifest file for the SkinControl compoent.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void ReadManifest(XPathNavigator manifestNav)
        {
            //Load the SkinControl from the manifest
            SkinControl = CBO.DeserializeObject<SkinControlInfo>(new StringReader(manifestNav.InnerXml));

            if (Log.Valid)
            {
                Log.AddInfo(Util.MODULE_ReadSuccess);
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
			//If Temp SkinControl exists then we need to update the DataStore with this 
            if (InstalledSkinControl == null)
            {
				//No Temp SkinControl - Delete newly added SkinControl
                DeleteSkinControl();
            }
            else
            {
				//Temp SkinControl - Rollback to Temp
                SkinControlController.SaveSkinControl(InstalledSkinControl);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The UnInstall method uninstalls the SkinControl component
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void UnInstall()
        {
            DeleteSkinControl();
        }
		
		#endregion
    }
}
