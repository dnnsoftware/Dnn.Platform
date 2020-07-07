// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Installers
{
    using System;
    using System.Xml.XPath;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Authentication;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The AuthenticationInstaller installs Authentication Service Components to a DotNetNuke site.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class AuthenticationInstaller : ComponentInstallerBase
    {
        private AuthenticationInfo AuthSystem;
        private AuthenticationInfo TempAuthSystem;

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
                return "ashx, aspx, ascx, vb, cs, resx, css, js, resources, config, vbproj, csproj, sln, htm, html";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Commit method finalises the Install and commits any pending changes.
        /// </summary>
        /// <remarks>In the case of Authentication systems this is not neccessary.</remarks>
        /// -----------------------------------------------------------------------------
        public override void Commit()
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Install method installs the authentication component.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void Install()
        {
            bool bAdd = Null.NullBoolean;
            try
            {
                // Attempt to get the Authentication Service
                this.TempAuthSystem = AuthenticationController.GetAuthenticationServiceByType(this.AuthSystem.AuthenticationType);

                if (this.TempAuthSystem == null)
                {
                    // Enable by default
                    this.AuthSystem.IsEnabled = true;
                    bAdd = true;
                }
                else
                {
                    this.AuthSystem.AuthenticationID = this.TempAuthSystem.AuthenticationID;
                    this.AuthSystem.IsEnabled = this.TempAuthSystem.IsEnabled;
                }

                this.AuthSystem.PackageID = this.Package.PackageID;
                if (bAdd)
                {
                    // Add new service
                    AuthenticationController.AddAuthentication(this.AuthSystem);
                }
                else
                {
                    // Update service
                    AuthenticationController.UpdateAuthentication(this.AuthSystem);
                }

                this.Completed = true;
                this.Log.AddInfo(string.Format(Util.AUTHENTICATION_Registered, this.AuthSystem.AuthenticationType));
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(ex);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The ReadManifest method reads the manifest file for the Authentication compoent.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void ReadManifest(XPathNavigator manifestNav)
        {
            this.AuthSystem = new AuthenticationInfo();

            // Get the type
            this.AuthSystem.AuthenticationType = Util.ReadElement(manifestNav, "authenticationService/type", this.Log, Util.AUTHENTICATION_TypeMissing);

            // Get the SettingsSrc
            this.AuthSystem.SettingsControlSrc = Util.ReadElement(manifestNav, "authenticationService/settingsControlSrc", this.Log, Util.AUTHENTICATION_SettingsSrcMissing);

            // Get the LoginSrc
            this.AuthSystem.LoginControlSrc = Util.ReadElement(manifestNav, "authenticationService/loginControlSrc", this.Log, Util.AUTHENTICATION_LoginSrcMissing);

            // Get the LogoffSrc
            this.AuthSystem.LogoffControlSrc = Util.ReadElement(manifestNav, "authenticationService/logoffControlSrc");

            if (this.Log.Valid)
            {
                this.Log.AddInfo(Util.AUTHENTICATION_ReadSuccess);
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
            // If Temp Auth System exists then we need to update the DataStore with this
            if (this.TempAuthSystem == null)
            {
                // No Temp Auth System - Delete newly added system
                this.DeleteAuthentiation();
            }
            else
            {
                // Temp Auth System - Rollback to Temp
                AuthenticationController.UpdateAuthentication(this.TempAuthSystem);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The UnInstall method uninstalls the authentication component.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void UnInstall()
        {
            this.DeleteAuthentiation();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The DeleteAuthentiation method deletes the Authentication System
        /// from the data Store.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void DeleteAuthentiation()
        {
            try
            {
                AuthenticationInfo authSystem = AuthenticationController.GetAuthenticationServiceByPackageID(this.Package.PackageID);
                if (authSystem != null)
                {
                    AuthenticationController.DeleteAuthentication(authSystem);
                }

                this.Log.AddInfo(string.Format(Util.AUTHENTICATION_UnRegistered, authSystem.AuthenticationType));
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(ex);
            }
        }
    }
}
