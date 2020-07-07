// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Installers
{
    using System;
    using System.IO;
    using System.Xml.XPath;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The SkinControlInstaller installs SkinControl (SkinObject) Components to a DotNetNuke site.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class SkinControlInstaller : ComponentInstallerBase
    {
        private SkinControlInfo InstalledSkinControl;
        private SkinControlInfo SkinControl;

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
                return "ascx, vb, cs, js, resx, xml, vbproj, csproj, sln";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Commit method finalises the Install and commits any pending changes.
        /// </summary>
        /// <remarks>In the case of Modules this is not neccessary.</remarks>
        /// -----------------------------------------------------------------------------
        public override void Commit()
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Install method installs the Module component.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void Install()
        {
            try
            {
                // Attempt to get the SkinControl
                this.InstalledSkinControl = SkinControlController.GetSkinControlByKey(this.SkinControl.ControlKey);

                if (this.InstalledSkinControl != null)
                {
                    this.SkinControl.SkinControlID = this.InstalledSkinControl.SkinControlID;
                }

                // Save SkinControl
                this.SkinControl.PackageID = this.Package.PackageID;
                this.SkinControl.SkinControlID = SkinControlController.SaveSkinControl(this.SkinControl);

                this.Completed = true;
                this.Log.AddInfo(string.Format(Util.MODULE_Registered, this.SkinControl.ControlKey));
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(ex);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The ReadManifest method reads the manifest file for the SkinControl compoent.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void ReadManifest(XPathNavigator manifestNav)
        {
            // Load the SkinControl from the manifest
            this.SkinControl = CBO.DeserializeObject<SkinControlInfo>(new StringReader(manifestNav.InnerXml));

            if (this.Log.Valid)
            {
                this.Log.AddInfo(Util.MODULE_ReadSuccess);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Rollback method undoes the installation of the component in the event
        /// that one of the other components fails.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void Rollback()
        {
            // If Temp SkinControl exists then we need to update the DataStore with this
            if (this.InstalledSkinControl == null)
            {
                // No Temp SkinControl - Delete newly added SkinControl
                this.DeleteSkinControl();
            }
            else
            {
                // Temp SkinControl - Rollback to Temp
                SkinControlController.SaveSkinControl(this.InstalledSkinControl);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The UnInstall method uninstalls the SkinControl component.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void UnInstall()
        {
            this.DeleteSkinControl();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The DeleteSkinControl method deletes the SkinControl from the data Store.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void DeleteSkinControl()
        {
            try
            {
                // Attempt to get the SkinControl
                SkinControlInfo skinControl = SkinControlController.GetSkinControlByPackageID(this.Package.PackageID);
                if (skinControl != null)
                {
                    SkinControlController.DeleteSkinControl(skinControl);
                }

                this.Log.AddInfo(string.Format(Util.MODULE_UnRegistered, skinControl.ControlKey));
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(ex);
            }
        }
    }
}
