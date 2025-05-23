﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Containers
{
    using System;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.UI.Skins;
    using DotNetNuke.UI.WebControls;

    /// <summary>A container object which displays the module's title.</summary>
    public partial class Title : SkinObjectBase
    {
        private const string MyFileName = "Title.ascx";

        public string CssClass { get; set; }

        /// <inheritdoc/>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.titleLabel.UpdateLabel += this.UpdateTitle;
        }

        /// <inheritdoc/>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // public attributes
            if (!string.IsNullOrEmpty(this.CssClass))
            {
                this.titleLabel.CssClass = this.CssClass;
            }

            string moduleTitle = Null.NullString;
            if (this.ModuleControl != null)
            {
                moduleTitle = Localization.LocalizeControlTitle(this.ModuleControl);
            }

            if (moduleTitle == Null.NullString)
            {
                moduleTitle = " ";
            }

            this.titleLabel.Text = moduleTitle;
            this.titleLabel.EditEnabled = false;
            this.titleToolbar.Visible = false;

            if (this.CanEditModule() && this.PortalSettings.InlineEditorEnabled)
            {
                this.titleLabel.EditEnabled = true;
                this.titleToolbar.Visible = true;
            }
        }

        private bool CanEditModule()
        {
            var canEdit = false;
            if (this.ModuleControl != null && this.ModuleControl.ModuleContext.ModuleId > Null.NullInteger)
            {
                canEdit = (Personalization.GetUserMode() == PortalSettings.Mode.Edit) && TabPermissionController.CanAdminPage() && !Globals.IsAdminControl();
            }

            return canEdit;
        }

        private void UpdateTitle(object source, DNNLabelEditEventArgs e)
        {
            if (this.CanEditModule())
            {
                ModuleInfo moduleInfo = ModuleController.Instance.GetModule(this.ModuleControl.ModuleContext.ModuleId, this.ModuleControl.ModuleContext.TabId, false);

#pragma warning disable CS0618 // PortalSecurity.FilterFlag.NoScripting is deprecated
                var ps = PortalSecurity.Instance;
                var mt = ps.InputFilter(e.Text, PortalSecurity.FilterFlag.NoScripting);
                moduleInfo.ModuleTitle = mt;
#pragma warning restore CS0618 // PortalSecurity.FilterFlag.NoScripting is deprecated

                ModuleController.Instance.UpdateModule(moduleInfo);
            }
        }
    }
}
