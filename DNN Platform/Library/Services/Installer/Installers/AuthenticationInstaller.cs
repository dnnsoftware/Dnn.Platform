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
using System.Xml.XPath;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Authentication;

#endregion

namespace DotNetNuke.Services.Installer.Installers
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The AuthenticationInstaller installs Authentication Service Components to a DotNetNuke site
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class AuthenticationInstaller : ComponentInstallerBase
    {
		#region "Private Properties"

        private AuthenticationInfo AuthSystem;
        private AuthenticationInfo TempAuthSystem;

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
                return "ashx, aspx, ascx, vb, cs, resx, css, js, resources, config, vbproj, csproj, sln, htm, html";
            }
        }
		
		#endregion

		#region "Private Methods"


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
                AuthenticationInfo authSystem = AuthenticationController.GetAuthenticationServiceByPackageID(Package.PackageID);
                if (authSystem != null)
                {
                    AuthenticationController.DeleteAuthentication(authSystem);
                }
                Log.AddInfo(string.Format(Util.AUTHENTICATION_UnRegistered, authSystem.AuthenticationType));
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
        /// <remarks>In the case of Authentication systems this is not neccessary</remarks>
        /// -----------------------------------------------------------------------------
        public override void Commit()
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Install method installs the authentication component
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void Install()
        {
            bool bAdd = Null.NullBoolean;
            try
            {
				//Attempt to get the Authentication Service
                TempAuthSystem = AuthenticationController.GetAuthenticationServiceByType(AuthSystem.AuthenticationType);

                if (TempAuthSystem == null)
                {
					//Enable by default
                    AuthSystem.IsEnabled = true;
                    bAdd = true;
                }
                else
                {
                    AuthSystem.AuthenticationID = TempAuthSystem.AuthenticationID;
                    AuthSystem.IsEnabled = TempAuthSystem.IsEnabled;
                }
                AuthSystem.PackageID = Package.PackageID;
                if (bAdd)
                {
                    //Add new service
                    AuthenticationController.AddAuthentication(AuthSystem);
                }
                else
                {
					//Update service
                    AuthenticationController.UpdateAuthentication(AuthSystem);
                }
                Completed = true;
                Log.AddInfo(string.Format(Util.AUTHENTICATION_Registered, AuthSystem.AuthenticationType));
            }
            catch (Exception ex)
            {
            
                Log.AddFailure(ex);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The ReadManifest method reads the manifest file for the Authentication compoent.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void ReadManifest(XPathNavigator manifestNav)
        {
            AuthSystem = new AuthenticationInfo();

            //Get the type
            AuthSystem.AuthenticationType = Util.ReadElement(manifestNav, "authenticationService/type", Log, Util.AUTHENTICATION_TypeMissing);

            //Get the SettingsSrc
            AuthSystem.SettingsControlSrc = Util.ReadElement(manifestNav, "authenticationService/settingsControlSrc", Log, Util.AUTHENTICATION_SettingsSrcMissing);

            //Get the LoginSrc
            AuthSystem.LoginControlSrc = Util.ReadElement(manifestNav, "authenticationService/loginControlSrc", Log, Util.AUTHENTICATION_LoginSrcMissing);

            //Get the LogoffSrc
            AuthSystem.LogoffControlSrc = Util.ReadElement(manifestNav, "authenticationService/logoffControlSrc");

            if (Log.Valid)
            {
                Log.AddInfo(Util.AUTHENTICATION_ReadSuccess);
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
			//If Temp Auth System exists then we need to update the DataStore with this 
            if (TempAuthSystem == null)
            {
				//No Temp Auth System - Delete newly added system
                DeleteAuthentiation();
            }
            else
            {
				//Temp Auth System - Rollback to Temp
                AuthenticationController.UpdateAuthentication(TempAuthSystem);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The UnInstall method uninstalls the authentication component
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void UnInstall()
        {
            DeleteAuthentiation();
        }
		
		#endregion
    }
}
