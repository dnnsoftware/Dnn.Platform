// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Groups
{
    using System;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Modules.Groups.Components;
    using DotNetNuke.Security.Permissions;
    using Microsoft.Extensions.DependencyInjection;

    public class GroupsModuleBase : PortalModuleBase
    {
        public GroupsModuleBase()
        {
            this.NavigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        public enum GroupMode
        {
            Setup = 0,
            List = 1,
            View = 2,
        }

        public GroupMode LoadView
        {
            get
            {
                var mode = GroupMode.Setup;
                if (this.Settings.ContainsKey(Constants.GroupLoadView))
                {
                    switch (this.Settings[Constants.GroupLoadView].ToString())
                    {
                        case "List":
                            mode = GroupMode.List;
                            break;
                        case "View":
                            mode = GroupMode.View;
                            break;
                    }
                }

                return mode;
            }
        }

        public int GroupId
        {
            get
            {
                int groupId = -1;
                if (string.IsNullOrEmpty(this.Request.QueryString["GroupId"]))
                {
                    return groupId;
                }

                if (int.TryParse(this.Request.QueryString["GroupId"], out groupId))
                {
                    return groupId;
                }

                return -1;
            }
        }

        public int DefaultRoleGroupId
        {
            get
            {
                var roleGroupId = Null.NullInteger;
                if (this.Settings.ContainsKey(Constants.DefaultRoleGroupSetting))
                {
                    int id;
                    if (int.TryParse(this.Settings[Constants.DefaultRoleGroupSetting].ToString(), out id))
                    {
                        roleGroupId = id;
                    }
                }

                return roleGroupId; // -2 is for "< All Roles >"
            }
        }

        public int GroupListTabId
        {
            get
            {
                if (this.Settings.ContainsKey(Constants.GroupListPage))
                {
                    return Convert.ToInt32(this.Settings[Constants.GroupListPage].ToString());
                }

                return this.TabId;
            }
        }

        public int GroupViewTabId
        {
            get
            {
                if (this.Settings.ContainsKey(Constants.GroupViewPage))
                {
                    return Convert.ToInt32(this.Settings[Constants.GroupViewPage].ToString());
                }

                return this.TabId;
            }
        }

        public string GroupViewTemplate
        {
            get
            {
                string template = this.LocalizeString("GroupViewTemplate.Text");
                if (this.Settings.ContainsKey(Constants.GroupViewTemplate))
                {
                    if (!string.IsNullOrEmpty(this.Settings[Constants.GroupViewTemplate].ToString()))
                    {
                        template = this.Settings[Constants.GroupViewTemplate].ToString();
                    }
                }

                return template;
            }
        }

        public string GroupListTemplate
        {
            get
            {
                string template = this.LocalizeString("GroupListTemplate.Text");
                if (this.Settings.ContainsKey(Constants.GroupListTemplate))
                {
                    if (!string.IsNullOrEmpty(this.Settings[Constants.GroupListTemplate].ToString()))
                    {
                        template = this.Settings[Constants.GroupListTemplate].ToString();
                    }
                }

                return template;
            }
        }

        public string DefaultGroupMode
        {
            get
            {
                if (this.Settings.ContainsKey(Constants.DefautlGroupViewMode))
                {
                    return this.Settings[Constants.DefautlGroupViewMode].ToString();
                }

                return string.Empty;
            }
        }

        public bool GroupModerationEnabled
        {
            get
            {
                if (this.Settings.ContainsKey(Constants.GroupModerationEnabled))
                {
                    return Convert.ToBoolean(this.Settings[Constants.GroupModerationEnabled].ToString());
                }

                return false;
            }
        }

        public bool CanCreate
        {
            get
            {
                if (this.Request.IsAuthenticated)
                {
                    if (this.UserInfo.IsSuperUser)
                    {
                        return true;
                    }

                    return ModulePermissionController.HasModulePermission(this.ModuleConfiguration.ModulePermissions, "CREATEGROUP");
                }

                return false;
            }
        }

        public string GroupListFilter
        {
            get
            {
                return this.Request.QueryString["Filter"];
            }
        }

        public int GroupListPageSize
        {
            get
            {
                if (this.Settings.ContainsKey(Constants.GroupListPageSize))
                {
                    if (!string.IsNullOrEmpty(this.Settings[Constants.GroupListPageSize].ToString()))
                    {
                        return Convert.ToInt32(this.Settings[Constants.GroupListPageSize].ToString());
                    }
                }

                return 20;
            }
        }

        public bool GroupListSearchEnabled
        {
            get
            {
                var enableSearch = false;

                if (this.Settings.ContainsKey(Constants.GroupListSearchEnabled))
                {
                    if (!string.IsNullOrEmpty(this.Settings[Constants.GroupListSearchEnabled].ToString()))
                    {
                        bool.TryParse(this.Settings[Constants.GroupListSearchEnabled].ToString(), out enableSearch);
                    }
                }

                return enableSearch;
            }
        }

        public string GroupListSortField
        {
            get
            {
                return this.Settings.ContainsKey(Constants.GroupListSortField) ? this.Settings[Constants.GroupListSortField].ToString() : string.Empty;
            }
        }

        public string GroupListSortDirection
        {
            get
            {
                return this.Settings.ContainsKey(Constants.GroupListSortDirection) ? this.Settings[Constants.GroupListSortDirection].ToString() : string.Empty;
            }
        }

        public bool GroupListUserGroupsOnly
        {
            get
            {
                var userGroupsOnly = false;

                if (this.Settings.ContainsKey(Constants.GroupListUserGroupsOnly))
                {
                    if (!string.IsNullOrEmpty(this.Settings[Constants.GroupListUserGroupsOnly].ToString()))
                    {
                        bool.TryParse(this.Settings[Constants.GroupListUserGroupsOnly].ToString(), out userGroupsOnly);
                    }
                }

                return userGroupsOnly;
            }
        }

        protected INavigationManager NavigationManager { get; }

        public string GetCreateUrl()
        {
            return this.ModuleContext.EditUrl("Create"); // .NavigateUrl(GroupCreateTabId,"",true,null);
        }

        public string GetClearFilterUrl()
        {
            return this.NavigationManager.NavigateURL(this.TabId, string.Empty);
        }

        public string GetEditUrl()
        {
            return this.ModuleContext.EditUrl("GroupId", this.GroupId.ToString("D"), "Edit");
        }
    }
}
