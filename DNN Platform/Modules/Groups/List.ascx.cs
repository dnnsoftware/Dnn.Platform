// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Groups
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Framework;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Display the group list.</summary>
    public partial class List : GroupsModuleBase
    {
        private readonly IServicesFramework servicesFramework;

        /// <summary>Initializes a new instance of the <see cref="List"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IServicesFramework. Scheduled removal in v12.0.0.")]
        public List()
            : this(null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="List"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="servicesFramework">The web API service framework.</param>
        public List(INavigationManager navigationManager, IServicesFramework servicesFramework)
        {
            this._navigationManager = navigationManager ?? this.DependencyProvider.GetRequiredService<INavigationManager>();
            this.servicesFramework = servicesFramework ?? this.DependencyProvider.GetRequiredService<IServicesFramework>();
        }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
#pragma warning disable CS3008 // Identifier beginning with an underscore is not CLS-compliant
        public INavigationManager _navigationManager { get; }
#pragma warning restore CS3008 // Identifier beginning with an underscore is not CLS-compliant

        protected void Page_Load(object sender, EventArgs e)
        {
            this.servicesFramework.RequestAjaxAntiForgerySupport();

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

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]

        // ReSharper disable once InconsistentNaming
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
