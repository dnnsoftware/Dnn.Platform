// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Authentication
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins.Controls;
    using DotNetNuke.UI.UserControls;

    /// <summary>
    /// Manages the Authentication settings.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public partial class Authentication : PortalModuleBase
    {
        private readonly List<AuthenticationSettingsBase> _settingControls = new List<AuthenticationSettingsBase>();

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.cmdUpdate.Click += this.OnUpdateClick;

            var authSystems = AuthenticationController.GetEnabledAuthenticationServices();

            foreach (var authSystem in authSystems)
            {
                // Add a Section Header
                var sectionHeadControl = (SectionHeadControl)this.LoadControl("~/controls/SectionHeadControl.ascx");
                sectionHeadControl.IncludeRule = true;
                sectionHeadControl.CssClass = "Head";

                // Create a <div> to hold the control
                var container = new HtmlGenericControl();
                container.ID = authSystem.AuthenticationType;

                var authSettingsControl = (AuthenticationSettingsBase)this.LoadControl("~/" + authSystem.SettingsControlSrc);

                // set the control ID to the resource file name ( ie. controlname.ascx = controlname )
                // this is necessary for the Localization in PageBase
                authSettingsControl.ID = Path.GetFileNameWithoutExtension(authSystem.SettingsControlSrc) + "_" + authSystem.AuthenticationType;

                // Add Settings Control to Container
                container.Controls.Add(authSettingsControl);
                this._settingControls.Add(authSettingsControl);

                // Add Section Head Control to Container
                this.pnlSettings.Controls.Add(sectionHeadControl);

                // Add Container to Controls
                this.pnlSettings.Controls.Add(container);

                // Attach Settings Control's container to Section Head Control
                sectionHeadControl.Section = container.ID;

                // Get Section Head Text from the setting controls LocalResourceFile
                authSettingsControl.LocalResourceFile = authSettingsControl.TemplateSourceDirectory + "/" + Localization.LocalResourceDirectory + "/" +
                                                        Path.GetFileNameWithoutExtension(authSystem.SettingsControlSrc);
                sectionHeadControl.Text = Localization.GetString("Title", authSettingsControl.LocalResourceFile);
                this.pnlSettings.Controls.Add(new LiteralControl("<br/>"));
                this.cmdUpdate.Visible = this.IsEditable;
            }
        }

        protected void OnUpdateClick(object sender, EventArgs e)
        {
            foreach (var settingControl in this._settingControls)
            {
                settingControl.UpdateSettings();
            }

            // Validate Enabled
            var enabled = false;
            var authSystems = AuthenticationController.GetEnabledAuthenticationServices();
            foreach (var authSystem in authSystems)
            {
                var authLoginControl = (AuthenticationLoginBase)this.LoadControl("~/" + authSystem.LoginControlSrc);

                // Check if AuthSystem is Enabled
                if (authLoginControl.Enabled)
                {
                    enabled = true;
                    break;
                }
            }

            if (!enabled)
            {
                // Display warning
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("NoProvidersEnabled", this.LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
            }
        }
    }
}
