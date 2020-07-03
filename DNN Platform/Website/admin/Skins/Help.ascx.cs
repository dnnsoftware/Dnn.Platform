// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;

    using DotNetNuke.Entities.Host;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;

    /// -----------------------------------------------------------------------------
    /// <summary></summary>
    /// <returns></returns>
    /// <remarks></remarks>
    /// -----------------------------------------------------------------------------
    public partial class Help : SkinObjectBase
    {
        public string CssClass { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                if (!string.IsNullOrEmpty(this.CssClass))
                {
                    this.hypHelp.CssClass = this.CssClass;
                }

                if (this.Request.IsAuthenticated)
                {
                    if (TabPermissionController.CanAdminPage())
                    {
                        this.hypHelp.Text = Localization.GetString("Help");
                        this.hypHelp.NavigateUrl = "mailto:" + Host.HostEmail + "?subject=" + this.PortalSettings.PortalName + " Support Request";
                        this.hypHelp.Visible = true;
                    }
                    else
                    {
                        this.hypHelp.Text = Localization.GetString("Help");
                        this.hypHelp.NavigateUrl = "mailto:" + this.PortalSettings.Email + "?subject=" + this.PortalSettings.PortalName + " Support Request";
                        this.hypHelp.Visible = true;
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void InitializeComponent()
        {
        }
    }
}
