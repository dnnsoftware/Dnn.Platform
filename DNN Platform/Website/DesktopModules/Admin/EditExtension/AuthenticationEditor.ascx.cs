// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.EditExtension
{
    using System;
    using System.IO;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Localization;
    using Microsoft.Extensions.DependencyInjection;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The AuthenticationEditor.ascx control is used to edit the Authentication Properties.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public partial class AuthenticationEditor : PackageEditorBase
    {
        private readonly INavigationManager _navigationManager;

        private AuthenticationInfo _AuthSystem;
        private AuthenticationSettingsBase _SettingsControl;

        public AuthenticationEditor()
        {
            this._navigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        protected AuthenticationInfo AuthSystem
        {
            get
            {
                if (this._AuthSystem == null)
                {
                    this._AuthSystem = AuthenticationController.GetAuthenticationServiceByPackageID(this.PackageID);
                }

                return this._AuthSystem;
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
                if (this._SettingsControl == null && !string.IsNullOrEmpty(this.AuthSystem.SettingsControlSrc))
                {
                    this._SettingsControl = (AuthenticationSettingsBase)this.LoadControl("~/" + this.AuthSystem.SettingsControlSrc);
                }

                return this._SettingsControl;
            }
        }

        public override void Initialize()
        {
            this.pnlSettings.Visible = !this.IsSuperTab;
            if (this.IsSuperTab)
            {
                this.lblHelp.Text = Localization.GetString("HostHelp", this.LocalResourceFile);
            }
            else
            {
                if (this.SettingsControl == null)
                {
                    this.lblHelp.Text = Localization.GetString("NoSettings", this.LocalResourceFile);
                }
                else
                {
                    this.lblHelp.Text = Localization.GetString("AdminHelp", this.LocalResourceFile);
                }
            }

            this.BindAuthentication();
        }

        public override void UpdatePackage()
        {
            if (this.authenticationForm.IsValid)
            {
                var authInfo = this.authenticationForm.DataSource as AuthenticationInfo;
                if (authInfo != null)
                {
                    AuthenticationController.UpdateAuthentication(authInfo);
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.cmdUpdate.Click += this.cmdUpdate_Click;
            var displayMode = this.DisplayMode;
            if (displayMode == "editor" || displayMode == "settings")
            {
                this.AuthEditorHead.Visible = this.AuthEditorHead.EnableViewState = false;
            }
        }

        protected void cmdUpdate_Click(object sender, EventArgs e)
        {
            this.SettingsControl?.UpdateSettings();

            var displayMode = this.DisplayMode;
            if (displayMode != "editor" && displayMode != "settings")
            {
                this.Response.Redirect(this._navigationManager.NavigateURL(), true);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This routine Binds the Authentication System.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void BindAuthentication()
        {
            if (this.AuthSystem != null)
            {
                if (this.AuthSystem.AuthenticationType == "DNN")
                {
                    this.authenticationFormReadOnly.DataSource = this.AuthSystem;
                    this.authenticationFormReadOnly.DataBind();
                }
                else
                {
                    this.authenticationForm.DataSource = this.AuthSystem;
                    this.authenticationForm.DataBind();
                }

                this.authenticationFormReadOnly.Visible = this.IsSuperTab && (this.AuthSystem.AuthenticationType == "DNN");
                this.authenticationForm.Visible = this.IsSuperTab && this.AuthSystem.AuthenticationType != "DNN";

                if (this.SettingsControl != null)
                {
                    // set the control ID to the resource file name ( ie. controlname.ascx = controlname )
                    // this is necessary for the Localization in PageBase
                    this.SettingsControl.ID = Path.GetFileNameWithoutExtension(this.AuthSystem.SettingsControlSrc);

                    // Add Container to Controls
                    this.pnlSettings.Controls.AddAt(0, this.SettingsControl);
                }
                else
                {
                    this.cmdUpdate.Visible = false;
                }
            }
        }
    }
}
