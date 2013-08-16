using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke;
using DotNetNuke.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Security.Roles;
using DotNetNuke.Security.Roles.Internal;
using DotNetNuke.Common.Utilities;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.Services.Localization;
using DotNetNuke.Modules.Groups.Components;

namespace DotNetNuke.Modules.Groups
{
    public partial class GroupView : GroupsModuleBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            RoleInfo role = TestableRoleController.Instance.GetRole(PortalId, r => r.SecurityMode != SecurityMode.SecurityRole && r.RoleID == GroupId);
            if (role == null && GroupId > 0)
            {
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("GroupIdNotFound", Constants.SharedResourcesPath), ModuleMessage.ModuleMessageType.YellowWarning);
            }

            if (role == null && (UserInfo.IsInRole(PortalSettings.AdministratorRoleName) || UserInfo.IsSuperUser))
            {
                role = new RoleInfo();
                role.RoleID = -1;
                role.RoleName = Localization.GetString("Sample_RoleName", LocalResourceFile);
                role.Description = Localization.GetString("Sample_RoleDescription", LocalResourceFile);

            }

            if (role == null)
                litOutput.Text = string.Empty;
            else
            {
                var resxPath = Constants.SharedResourcesPath;

                var template = GroupViewTemplate;
                template = template.Replace("{resx:posts}", Localization.GetString("posts", resxPath));
                template = template.Replace("{resx:members}", Localization.GetString("members", resxPath));
                template = template.Replace("{resx:photos}", Localization.GetString("photos", resxPath));
                template = template.Replace("{resx:documents}", Localization.GetString("documents", resxPath));

                template = template.Replace("{resx:Join}", Localization.GetString("Join", resxPath));
                template = template.Replace("{resx:JoinGroup}", Localization.GetString("JoinGroup", resxPath));
                template = template.Replace("{resx:Pending}", Localization.GetString("Pending", resxPath));
                template = template.Replace("{resx:LeaveGroup}", Localization.GetString("LeaveGroup", resxPath));
                template = template.Replace("{resx:EditGroup}", Localization.GetString("EditGroup", resxPath));
                template = template.Replace("[GroupViewTabId]", GroupViewTabId.ToString());


                var groupParser = new GroupViewParser(PortalSettings, role, UserInfo, template, TabId);
                groupParser.GroupEditUrl = GetEditUrl();
                
                litOutput.Text = groupParser.ParseView();

            }

        }
    }
}