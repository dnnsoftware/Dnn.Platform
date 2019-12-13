#region Usings

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Modules.Groups.Components;
using System;
using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Abstractions;

#endregion

namespace DotNetNuke.Modules.Groups
{
    public class GroupsModuleBase : PortalModuleBase
    {
        protected INavigationManager NavigationManager { get; }
        public GroupsModuleBase()
        {
            NavigationManager = DependencyProvider.GetRequiredService<INavigationManager>();
        }

        public enum GroupMode
        {
            Setup = 0,
            List = 1,
            View = 2
        }

        #region Public Properties
        public GroupMode LoadView
        {
            get
            {
                var mode = GroupMode.Setup;
                if (Settings.ContainsKey(Constants.GroupLoadView))
                {
                    switch (Settings[Constants.GroupLoadView].ToString())
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
                if (string.IsNullOrEmpty(Request.QueryString["GroupId"]))
                {
                    return groupId;
                }
                if (int.TryParse(Request.QueryString["GroupId"], out groupId))
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
                if (Settings.ContainsKey(Constants.DefaultRoleGroupSetting))
                {
                    int id;
                    if (int.TryParse(Settings[Constants.DefaultRoleGroupSetting].ToString(), out id))
                        roleGroupId = id;
                }

                return roleGroupId; // -2 is for "< All Roles >"
            }
        }
        public int GroupListTabId
        {
            get
            {
                if (Settings.ContainsKey(Constants.GroupListPage))
                {
                    return Convert.ToInt32(Settings[Constants.GroupListPage].ToString());
                }
                return TabId;
            }
        }
        public int GroupViewTabId
        {
            get
            {
                if (Settings.ContainsKey(Constants.GroupViewPage))
                {
                    return Convert.ToInt32(Settings[Constants.GroupViewPage].ToString());
                }
                return TabId;
            }
        }
        public string GroupViewTemplate
        {
            get
            {
                string template = LocalizeString("GroupViewTemplate.Text");
                if (Settings.ContainsKey(Constants.GroupViewTemplate))
                {
                    if (!string.IsNullOrEmpty(Settings[Constants.GroupViewTemplate].ToString()))
                    {
                        template = Settings[Constants.GroupViewTemplate].ToString();
                    }
                }
                return template;
            }
        }
        public string GroupListTemplate
        {
            get
            {
                string template = LocalizeString("GroupListTemplate.Text");
                if (Settings.ContainsKey(Constants.GroupListTemplate))
                {
                    if (!string.IsNullOrEmpty(Settings[Constants.GroupListTemplate].ToString()))
                    {
                        template = Settings[Constants.GroupListTemplate].ToString();
                    }
                }
                return template;
            }
        }
        public string DefaultGroupMode
        {
            get
            {
                if (Settings.ContainsKey(Constants.DefautlGroupViewMode))
                {
                    return Settings[Constants.DefautlGroupViewMode].ToString();
                }
                return "";
            }
        }
        public bool GroupModerationEnabled
        {
            get
            {
                if (Settings.ContainsKey(Constants.GroupModerationEnabled))
                {
                    return Convert.ToBoolean(Settings[Constants.GroupModerationEnabled].ToString());
                }
                return false;
            }
        }
        public bool CanCreate
        {
            get
            {
                if (Request.IsAuthenticated)
                {
                    if (UserInfo.IsSuperUser)
                    {
                        return true;
                    }
                    return ModulePermissionController.HasModulePermission(ModuleConfiguration.ModulePermissions, "CREATEGROUP");
                }
                return false;
            }
        }

        public string GroupListFilter
        {
            get
            {
                return Request.QueryString["Filter"];
            }
        }

        public int GroupListPageSize
        {
            get
            {
                if (Settings.ContainsKey(Constants.GroupListPageSize))
                {
                    if (!string.IsNullOrEmpty(Settings[Constants.GroupListPageSize].ToString()))
                    {
                        return Convert.ToInt32(Settings[Constants.GroupListPageSize].ToString());
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

                if (Settings.ContainsKey(Constants.GroupListSearchEnabled))
                {
                    if (!string.IsNullOrEmpty(Settings[Constants.GroupListSearchEnabled].ToString()))
                    {
                        bool.TryParse(Settings[Constants.GroupListSearchEnabled].ToString(), out enableSearch);
                    }
                }

                return enableSearch;
            }
        }
        public string GroupListSortField
        {
            get
            {
                return Settings.ContainsKey(Constants.GroupListSortField) ? Settings[Constants.GroupListSortField].ToString() : "";
            }
        }
        public string GroupListSortDirection
        {
            get
            {
                return Settings.ContainsKey(Constants.GroupListSortDirection) ? Settings[Constants.GroupListSortDirection].ToString() : "";
            }
        }
        public bool GroupListUserGroupsOnly
        {
            get
            {
                var userGroupsOnly = false;

                if (Settings.ContainsKey(Constants.GroupListUserGroupsOnly))
                {
                    if (!string.IsNullOrEmpty(Settings[Constants.GroupListUserGroupsOnly].ToString()))
                    {
                        bool.TryParse(Settings[Constants.GroupListUserGroupsOnly].ToString(), out userGroupsOnly);
                    }
                }

                return userGroupsOnly;
            }
        }


        #endregion

        #region Public Methods
        public string GetCreateUrl()
        {
            return ModuleContext.EditUrl("Create"); //.NavigateUrl(GroupCreateTabId,"",true,null);
        }

        public string GetClearFilterUrl()
        {
            return NavigationManager.NavigateURL(TabId, "");
        }

        public string GetEditUrl()
        {
            return ModuleContext.EditUrl("GroupId", GroupId.ToString("D"), "Edit");
        }
        #endregion
    }
}
