// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.Common;
using DotNetNuke.Abstractions;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.Modules.Admin.EditExtension
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The AuthenticationEditor.ascx control is used to edit the Authentication Properties
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public partial class AuthenticationEditor : PackageEditorBase
    {
        private readonly INavigationManager _navigationManager;

        public AuthenticationEditor()
        {
            _navigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

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
            var displayMode = DisplayMode;
            if (displayMode == "editor" || displayMode == "settings")
            {
                AuthEditorHead.Visible = AuthEditorHead.EnableViewState = false;
            }
        }

        protected void cmdUpdate_Click(object sender, EventArgs e)
        {
            SettingsControl?.UpdateSettings();

            var displayMode = DisplayMode;
            if (displayMode != "editor" && displayMode != "settings")
                Response.Redirect(_navigationManager.NavigateURL(), true);
        }

		#endregion
    }
}
