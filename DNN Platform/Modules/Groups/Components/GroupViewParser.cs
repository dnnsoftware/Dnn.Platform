using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Roles;
using DotNetNuke.Entities.Users;
using DotNetNuke.Common;
using DotNetNuke.Services.Localization;

namespace DotNetNuke.Modules.Groups.Components
{
    public class GroupViewParser
    {
        
        PortalSettings PortalSettings { get; set; }
        RoleInfo RoleInfo { get; set; }
        UserInfo CurrentUser { get; set; }
        public string Template { get; set; }
        public int GroupViewTabId { get; set; }
        public string GroupEditUrl { get; set; }

        public GroupViewParser(PortalSettings portalSettings, RoleInfo roleInfo, UserInfo currentUser, string template, int groupViewTabId)
        {
            PortalSettings = portalSettings;
            RoleInfo = roleInfo;
            CurrentUser = currentUser;
            Template = template;
            GroupViewTabId = groupViewTabId;
        }

        public string ParseView()
        {
            var membershipPending = false;
            var isOwner = false;

            if (HttpContext.Current.Request.IsAuthenticated)
            {
                var userRoleInfo = CurrentUser.Social.Roles.FirstOrDefault(r => r.RoleID == RoleInfo.RoleID);

                if (userRoleInfo != null)
                {
                    isOwner = userRoleInfo.IsOwner;
                    if (userRoleInfo.Status == RoleStatus.Pending)
                    {
                        membershipPending = true;
                    }
                }
                if (RoleInfo.CreatedByUserID == CurrentUser.UserID || CurrentUser.IsSuperUser)
                {
                    isOwner = true;
                }
            }

            var editUrl = Localization.GetString("GroupEditUrl", Constants.SharedResourcesPath);

            if (isOwner)
            {

                Template = Template.Replace("[GROUPEDITBUTTON]", String.Format(editUrl, GroupEditUrl));
                Template = Utilities.ParseTokenWrapper(Template, "IsNotOwner", false);
                Template = Utilities.ParseTokenWrapper(Template, "IsOwner", true);
            }
            else if (CurrentUser.IsInRole(RoleInfo.RoleName))
            {
                Template = Utilities.ParseTokenWrapper(Template, "IsNotOwner", true);
                Template = Utilities.ParseTokenWrapper(Template, "IsOwner", false);
            }

            Template = Utilities.ParseTokenWrapper(Template, "IsNotOwner", false);
            Template = Utilities.ParseTokenWrapper(Template, "IsOwner", false);

            if (CurrentUser.IsInRole(RoleInfo.RoleName) || !HttpContext.Current.Request.IsAuthenticated || membershipPending)
                Template = Utilities.ParseTokenWrapper(Template, "IsNotMember", false);
            else
                Template = Utilities.ParseTokenWrapper(Template, "IsNotMember", true);

            if (CurrentUser.IsInRole(RoleInfo.RoleName))
            {
                Template = Utilities.ParseTokenWrapper(Template, "IsMember", true);
                Template = Utilities.ParseTokenWrapper(Template, "IsPendingMember", false);
            }
            else
                Template = Utilities.ParseTokenWrapper(Template, "IsMember", false);

            Template = Utilities.ParseTokenWrapper(Template, "AllowJoin", RoleInfo.IsPublic);

            Template = Template.Replace("[GROUPEDITBUTTON]", String.Empty);

            var url = Globals.NavigateURL(GroupViewTabId, "", new String[] { "groupid=" + RoleInfo.RoleID.ToString() });
            
            Template = Utilities.ParseTokenWrapper(Template, "IsPendingMember", membershipPending);
            Template = Template.Replace("[groupviewurl]", url);
            Components.GroupItemTokenReplace tokenReplace = new Components.GroupItemTokenReplace(RoleInfo);
            Template = tokenReplace.ReplaceGroupItemTokens(Template);
            return Template;
        }
    }
}