// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Groups
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Modules.Groups.Components;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Security.Roles.Internal;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins.Controls;
    using DotNetNuke.UI.WebControls;

    public partial class GroupView : GroupsModuleBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            RoleInfo role = RoleController.Instance.GetRole(this.PortalId, r => r.SecurityMode != SecurityMode.SecurityRole && r.RoleID == this.GroupId);
            if (role == null && this.GroupId > 0)
            {
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("GroupIdNotFound", Constants.SharedResourcesPath), ModuleMessage.ModuleMessageType.YellowWarning);
            }

            if (role == null && (this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName) || this.UserInfo.IsSuperUser))
            {
                role = new RoleInfo();
                role.RoleID = -1;
                role.RoleName = Localization.GetString("Sample_RoleName", this.LocalResourceFile);
                role.Description = Localization.GetString("Sample_RoleDescription", this.LocalResourceFile);
            }

            if (role == null)
            {
                this.litOutput.Text = string.Empty;
            }
            else
            {
                var resxPath = Constants.SharedResourcesPath;

                var template = this.GroupViewTemplate;
                template = template.Replace("{resx:posts}", Localization.GetString("posts", resxPath));
                template = template.Replace("{resx:members}", Localization.GetString("members", resxPath));
                template = template.Replace("{resx:photos}", Localization.GetString("photos", resxPath));
                template = template.Replace("{resx:documents}", Localization.GetString("documents", resxPath));

                template = template.Replace("{resx:Join}", Localization.GetString("Join", resxPath));
                template = template.Replace("{resx:JoinGroup}", Localization.GetString("JoinGroup", resxPath));
                template = template.Replace("{resx:Pending}", Localization.GetString("Pending", resxPath));
                template = template.Replace("{resx:LeaveGroup}", Localization.GetString("LeaveGroup", resxPath));
                template = template.Replace("{resx:EditGroup}", Localization.GetString("EditGroup", resxPath));
                template = template.Replace("[GroupViewTabId]", this.GroupViewTabId.ToString());

                var groupParser = new GroupViewParser(this.PortalSettings, role, this.UserInfo, template, this.TabId);
                groupParser.GroupEditUrl = this.GetEditUrl();

                this.litOutput.Text = groupParser.ParseView();
            }
        }
    }
}
