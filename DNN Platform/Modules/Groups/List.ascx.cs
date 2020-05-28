// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.Common;
using DotNetNuke.Abstractions;
using DotNetNuke.Framework;

namespace DotNetNuke.Modules.Groups
{
    public partial class List : GroupsModuleBase
    {
        public INavigationManager _navigationManager { get; }
        public List()
        {
            _navigationManager = DependencyProvider.GetRequiredService<INavigationManager>();
        }
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

            Response.Redirect(_navigationManager.NavigateURL(TabId, "", "filter=" + txtFilter.Text.Trim()));
        }
    }
}
