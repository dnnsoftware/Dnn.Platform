// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins;
using DotNetNuke.UI.WebControls;

#endregion

namespace DotNetNuke.UI.Containers
{
    /// -----------------------------------------------------------------------------
    /// <summary></summary>
    /// <remarks></remarks>
    /// -----------------------------------------------------------------------------
    public partial class Title : SkinObjectBase
    {
        private const string MyFileName = "Title.ascx";
        #region "Public Members"
        public string CssClass { get; set; }

        #endregion

        private bool CanEditModule()
        {
            var canEdit = false;
            if (ModuleControl != null && ModuleControl.ModuleContext.ModuleId > Null.NullInteger)
            {
                canEdit = (PortalSettings.UserMode == PortalSettings.Mode.Edit) && TabPermissionController.CanAdminPage() && !Globals.IsAdminControl();
            }
            return canEdit;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            titleLabel.UpdateLabel += UpdateTitle;
        }


        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            //public attributes
            if (!String.IsNullOrEmpty(CssClass))
            {
                titleLabel.CssClass = CssClass;
            }
            string moduleTitle = Null.NullString;
            if (ModuleControl != null)
            {
                moduleTitle = Localization.LocalizeControlTitle(ModuleControl);
            }
            if (moduleTitle == Null.NullString)
            {
                moduleTitle = " ";
            }

            titleLabel.Text = moduleTitle;
            titleLabel.EditEnabled = false;
            titleToolbar.Visible = false;

            if (CanEditModule() && PortalSettings.InlineEditorEnabled)
            {
                titleLabel.EditEnabled = true;
                titleToolbar.Visible = true;
            }

        }

        private void UpdateTitle(object source, DNNLabelEditEventArgs e)
        {
            if (CanEditModule())
            {
                ModuleInfo moduleInfo = ModuleController.Instance.GetModule(ModuleControl.ModuleContext.ModuleId, ModuleControl.ModuleContext.TabId, false);

                var ps = PortalSecurity.Instance;
                var mt = ps.InputFilter(e.Text, PortalSecurity.FilterFlag.NoScripting);
                moduleInfo.ModuleTitle = mt;

                ModuleController.Instance.UpdateModule(moduleInfo);
            }
        }
    }
}
