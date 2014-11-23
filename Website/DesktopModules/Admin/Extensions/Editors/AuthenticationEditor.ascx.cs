#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using DotNetNuke.Common;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.WebControls;

#endregion

namespace DotNetNuke.Modules.Admin.Extensions
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The AuthenticationEditor.ascx control is used to edit the Authentication Properties
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	01/04/2008	Created
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class AuthenticationEditor : PackageEditorBase
    {
		#region "Private Members"

        private AuthenticationInfo _AuthSystem;
        private AuthenticationSettingsBase _SettingsControl;

		#endregion

		#region "Protected Properties"

        protected AuthenticationInfo AuthSystem
        {
            get
            {
                if (_AuthSystem == null)
                {
                    _AuthSystem = AuthenticationController.GetAuthenticationServiceByPackageID(PackageID);
                }
                return _AuthSystem;
            }
        }

        protected override string EditorID
        {
            get
            {
                return "AuthenticationEditor";
            }
        }

        protected AuthenticationSettingsBase SettingsControl
        {
            get
            {
                if (_SettingsControl == null && !string.IsNullOrEmpty(AuthSystem.SettingsControlSrc))
                {
                    _SettingsControl = (AuthenticationSettingsBase) LoadControl("~/" + AuthSystem.SettingsControlSrc);
                }
                return _SettingsControl;
            }
        }

		#endregion

		#region "Private Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This routine Binds the Authentication System
        /// </summary>
        /// <history>
        /// 	[cnurse]	08/15/2007	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void BindAuthentication()
        {
            if (AuthSystem != null)
            {
                if (AuthSystem.AuthenticationType == "DNN")
                {
                    authenticationFormReadOnly.DataSource = AuthSystem;
                    authenticationFormReadOnly.DataBind();
                }
                else
                {
                    authenticationForm.DataSource = AuthSystem;
                    authenticationForm.DataBind();
                }
                authenticationFormReadOnly.Visible = IsSuperTab && (AuthSystem.AuthenticationType == "DNN");
                authenticationForm.Visible = IsSuperTab && AuthSystem.AuthenticationType != "DNN";


                if (SettingsControl != null)
                {
					//set the control ID to the resource file name ( ie. controlname.ascx = controlname )
                    //this is necessary for the Localization in PageBase
                    SettingsControl.ID = Path.GetFileNameWithoutExtension(AuthSystem.SettingsControlSrc);

                    //Add Container to Controls
                    pnlSettings.Controls.AddAt(0, SettingsControl);
                }
                else
                {
                    cmdUpdate.Visible = false;
                }
            }
        }

		#endregion

		#region "Public Methods"

        public override void Initialize()
        {
            pnlSettings.Visible = !IsSuperTab;
            if (IsSuperTab)
            {
                lblHelp.Text = Localization.GetString("HostHelp", LocalResourceFile);
            }
            else
            {
                if (SettingsControl == null)
                {
                    lblHelp.Text = Localization.GetString("NoSettings", LocalResourceFile);
                }
                else
                {
                    lblHelp.Text = Localization.GetString("AdminHelp", LocalResourceFile);
                }
            }
            BindAuthentication();
        }

        public override void UpdatePackage()
        {
            if (authenticationForm.IsValid)
            {
                var authInfo = authenticationForm.DataSource as AuthenticationInfo;
                if (authInfo != null)
                {
                    AuthenticationController.UpdateAuthentication(authInfo);
                }
            }
        }

		#endregion

		#region "Event Handlers"

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            cmdUpdate.Click += cmdUpdate_Click;
        }

        protected void cmdUpdate_Click(object sender, EventArgs e)
        {
            if (SettingsControl != null)
            {
                SettingsControl.UpdateSettings();
            }

            Response.Redirect(Globals.NavigateURL(), true);
        }
		
		#endregion
    }
}