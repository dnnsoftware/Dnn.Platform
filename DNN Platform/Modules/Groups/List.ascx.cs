// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Groups
{
    using System;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Framework;
    using Microsoft.Extensions.DependencyInjection;

    public partial class List : GroupsModuleBase
    {
        public List()
        {
            this._navigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        public INavigationManager _navigationManager { get; }

        protected void Page_Load(object sender, EventArgs e)
        {
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

            this.panelSearch.Visible = this.GroupListSearchEnabled;

            this.ctlGroupList.TabId = this.TabId;
            this.ctlGroupList.GroupViewTabId = this.GroupViewTabId;
            this.ctlGroupList.RoleGroupId = this.DefaultRoleGroupId;
            this.ctlGroupList.PageSize = this.GroupListPageSize;
            this.ctlGroupList.DisplayCurrentUserGroups = this.GroupListUserGroupsOnly;
            this.ctlGroupList.SearchFilter = this.GroupListFilter;
            this.ctlGroupList.SortField = this.GroupListSortField;
            this.ctlGroupList.SortDirection = this.GroupListSortDirection;

            if (!string.IsNullOrEmpty(this.GroupListSortField))
            {
                this.ctlGroupList.SortField = this.GroupListSortField;
            }

            if (!string.IsNullOrEmpty(this.GroupListSortDirection))
            {
                this.ctlGroupList.SortDirection = this.GroupListSortDirection;
            }

            if (!string.IsNullOrEmpty(this.GroupListTemplate))
            {
                this.ctlGroupList.ItemTemplate = this.GroupListTemplate;
            }

            if (!string.IsNullOrEmpty(this.GroupListFilter))
            {
                this.txtFilter.Text = this.GroupListFilter;
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (!this.Page.IsValid)
            {
                return;
            }

            this.Response.Redirect(this._navigationManager.NavigateURL(this.TabId, string.Empty, "filter=" + this.txtFilter.Text.Trim()));
        }
    }
}
