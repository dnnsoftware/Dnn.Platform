// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Groups.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Localization;
    using Microsoft.Extensions.DependencyInjection;

    public class GroupViewParser
    {
        public GroupViewParser(PortalSettings portalSettings, RoleInfo roleInfo, UserInfo currentUser, string template, int groupViewTabId)
        {
            this.PortalSettings = portalSettings;
            this.RoleInfo = roleInfo;
            this.CurrentUser = currentUser;
            this.Template = template;
            this.GroupViewTabId = groupViewTabId;
            this.NavigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        public string Template { get; set; }

        public int GroupViewTabId { get; set; }

        public string GroupEditUrl { get; set; }

        protected INavigationManager NavigationManager { get; }

        private PortalSettings PortalSettings { get; set; }

        private RoleInfo RoleInfo { get; set; }

        private UserInfo CurrentUser { get; set; }

        public string ParseView()
        {
            var membershipPending = false;
            var isOwner = false;

            if (HttpContext.Current.Request.IsAuthenticated)
            {
                var userRoleInfo = this.CurrentUser.Social.Roles.FirstOrDefault(r => r.RoleID == this.RoleInfo.RoleID);

                if (userRoleInfo != null)
                {
                    isOwner = userRoleInfo.IsOwner;
                    if (userRoleInfo.Status == RoleStatus.Pending)
                    {
                        membershipPending = true;
                    }
                }

                if (this.RoleInfo.CreatedByUserID == this.CurrentUser.UserID || this.CurrentUser.IsSuperUser)
                {
                    isOwner = true;
                }
            }

            var editUrl = Localization.GetString("GroupEditUrl", Constants.SharedResourcesPath);

            if (isOwner)
            {
                this.Template = this.Template.Replace("[GROUPEDITBUTTON]", string.Format(editUrl, this.GroupEditUrl));
                this.Template = Utilities.ParseTokenWrapper(this.Template, "IsNotOwner", false);
                this.Template = Utilities.ParseTokenWrapper(this.Template, "IsOwner", true);
            }
            else if (this.CurrentUser.IsInRole(this.RoleInfo.RoleName))
            {
                this.Template = Utilities.ParseTokenWrapper(this.Template, "IsNotOwner", true);
                this.Template = Utilities.ParseTokenWrapper(this.Template, "IsOwner", false);
            }

            this.Template = Utilities.ParseTokenWrapper(this.Template, "IsNotOwner", false);
            this.Template = Utilities.ParseTokenWrapper(this.Template, "IsOwner", false);

            if (this.CurrentUser.IsInRole(this.RoleInfo.RoleName) || !HttpContext.Current.Request.IsAuthenticated || membershipPending)
            {
                this.Template = Utilities.ParseTokenWrapper(this.Template, "IsNotMember", false);
            }
            else
            {
                this.Template = Utilities.ParseTokenWrapper(this.Template, "IsNotMember", true);
            }

            if (this.CurrentUser.IsInRole(this.RoleInfo.RoleName))
            {
                this.Template = Utilities.ParseTokenWrapper(this.Template, "IsMember", true);
                this.Template = Utilities.ParseTokenWrapper(this.Template, "IsPendingMember", false);
            }
            else
            {
                this.Template = Utilities.ParseTokenWrapper(this.Template, "IsMember", false);
            }

            this.Template = Utilities.ParseTokenWrapper(this.Template, "AllowJoin", this.RoleInfo.IsPublic);

            this.Template = this.Template.Replace("[GROUPEDITBUTTON]", string.Empty);

            var url = this.NavigationManager.NavigateURL(this.GroupViewTabId, string.Empty, new string[] { "groupid=" + this.RoleInfo.RoleID.ToString() });

            this.Template = Utilities.ParseTokenWrapper(this.Template, "IsPendingMember", membershipPending);
            this.Template = this.Template.Replace("[groupviewurl]", url);
            Components.GroupItemTokenReplace tokenReplace = new Components.GroupItemTokenReplace(this.RoleInfo);
            this.Template = tokenReplace.ReplaceGroupItemTokens(this.Template);
            return this.Template;
        }
    }
}
