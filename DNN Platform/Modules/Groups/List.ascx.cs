using System;

using DotNetNuke.Common;
using DotNetNuke.Framework;

namespace DotNetNuke.Modules.Groups
{
    public partial class List : GroupsModuleBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

            panelSearch.Visible = GroupListSearchEnabled;

            ctlGroupList.TabId = TabId;
            ctlGroupList.GroupViewTabId = GroupViewTabId;
            ctlGroupList.RoleGroupId = DefaultRoleGroupId;
            ctlGroupList.PageSize = GroupListPageSize;
            ctlGroupList.DisplayCurrentUserGroups = GroupListUserGroupsOnly;
            ctlGroupList.SearchFilter = GroupListFilter;
            ctlGroupList.SortField = GroupListSortField;
            ctlGroupList.SortDirection = GroupListSortDirection;

            if (!string.IsNullOrEmpty(GroupListSortField))
            {
                ctlGroupList.SortField = GroupListSortField;
            }

            if (!string.IsNullOrEmpty(GroupListSortDirection))
            {
                ctlGroupList.SortDirection = GroupListSortDirection;
            }


            if (!String.IsNullOrEmpty(GroupListTemplate))
            {
                ctlGroupList.ItemTemplate = GroupListTemplate;
            }

            if (!string.IsNullOrEmpty(GroupListFilter))
            {
                txtFilter.Text = GroupListFilter;
            }

        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if(!Page.IsValid) return;

            Response.Redirect(Globals.NavigateURL(TabId, "", "filter=" + txtFilter.Text.Trim()));
        }
    }
}