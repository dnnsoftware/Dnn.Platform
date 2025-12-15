// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Linq;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Users.Social;
    using DotNetNuke.Services.Tokens;

    /// <summary>This control is used for displaying a template based list of users based upon various filter and sorting capabilities.</summary>
    [ToolboxData("<{0}:DnnMemberListControl runat=\"server\"></{0}:DnnMemberListControl>")]
    public class DnnMemberListControl : WebControl
    {
        private UserInfo currentUser;
        private RelationshipController relationshipController;

        /// <summary>Gets or sets the template for displaying the header section of a DnnMemberListControl object.</summary>
        [DefaultValue("")]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public string HeaderTemplate { get; set; }

        /// <summary>Gets or sets the template for the row header.</summary>
        [DefaultValue("")]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public string RowHeaderTemplate { get; set; }

        /// <summary>Gets or sets the template for displaying an item in a DnnMemberListControl object.</summary>
        [DefaultValue("")]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public string ItemTemplate { get; set; }

        /// <summary>Gets or sets the template for the row footer.</summary>
        [DefaultValue("")]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public string RowFooterTemplate { get; set; }

        /// <summary>Gets or sets the template for displaying the alternating row headers in a DnnMemberListControl object.</summary>
        [DefaultValue("")]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public string AlternatingRowHeaderTemplate { get; set; }

        /// <summary>Gets or sets the template for displaying the alternating items in a DnnMemberListControl object.</summary>
        [DefaultValue("")]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public string AlternatingItemTemplate { get; set; }

        /// <summary>Gets or sets the template for displaying the alternating row footers in a DnnMemberListControl object.</summary>
        [DefaultValue("")]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public string AlternatingRowFooterTemplate { get; set; }

        /// <summary>Gets or sets the template for displaying the footer section of a DnnMemberListControl object.</summary>
        [DefaultValue("")]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public string FooterTemplate { get; set; }

        /// <summary>Gets or sets the index of the currently displayed page.</summary>
        [DefaultValue(1)]
        public int PageIndex { get; set; }

        /// <summary>Gets or sets the number of records to display on a page in a DnnMemberListControl object.</summary>
        [DefaultValue(10)]
        public int PageSize { get; set; }

        /// <summary>Gets or sets the number of items displayed on each row.</summary>
        [DefaultValue(1)]
        public int RowSize { get; set; }

        /// <summary>Gets or sets the property value to sort by.</summary>
        [DefaultValue("UserId")]
        public string SortBy { get; set; }

        /// <summary>Gets or sets a value indicating whether the sort is ascending.</summary>
        [DefaultValue(true)]
        public bool SortAscending { get; set; }

        /// <summary>Gets or sets the collection of filters to apply when getting the list of members.</summary>
        /// <remarks>
        /// Possible keys are: RoleId, RelationshipTypeId, UserId, Profile:PropertyName, FirstName, LastName, DisplayName, Username, Email.
        /// </remarks>
        public IDictionary<string, string> Filters { get; set; }

        /// <inheritdoc/>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.currentUser = UserController.Instance.GetCurrentUserInfo();
            this.relationshipController = new RelationshipController();
        }

        /// <inheritdoc/>
        protected override void Render(HtmlTextWriter writer)
        {
            if (this.ItemTemplate == string.Empty)
            {
                return;
            }

            writer.Write(this.HeaderTemplate);

            // Filters
            if (this.Filters == null)
            {
                this.Filters = new Dictionary<string, string>();
            }

            var additionalFilters = new Dictionary<string, string>();
            additionalFilters.Add("Records", this.PageSize.ToString());
            additionalFilters.Add("PageIndex", this.PageIndex.ToString());
            additionalFilters.Add("Rowsize", this.RowSize.ToString());
            additionalFilters.Add("SortBy", this.SortBy);
            additionalFilters.Add("SortAscending", this.SortAscending.ToString());

            // Currently Not Used by the SPROC
            var filterUser = this.Filters.TryGetValue("UserId", out var userId) && userId != null ? new UserInfo { UserID = int.Parse(userId) } : new UserInfo() { PortalID = this.currentUser.PortalID };
            var role = this.Filters.TryGetValue("RoleId", out var roleId) && roleId != null ? new UserRoleInfo { RoleID = int.Parse(roleId) } : null;
            var relationship = this.Filters.TryGetValue("RelationshipTypeId", out var relationshipTypeId) && relationshipTypeId != null ? new RelationshipType() { RelationshipTypeId = int.Parse(relationshipTypeId) } : null;

            foreach (var filter in this.Filters.Where(filter => !additionalFilters.ContainsKey(filter.Key)))
            {
                additionalFilters.Add(filter.Key, filter.Value);
            }

            var row = 0;
            var users = new DataTable();

            // users.Load(_relationshipController.GetUsersAdvancedSearch(_currentUser, filterUser, role, relationship, Filters, additionalFilters));
            if (users.Rows.Count > 0)
            {
                foreach (DataRow user in users.Rows)
                {
                    // Row Header
                    writer.Write(string.IsNullOrEmpty(this.AlternatingRowHeaderTemplate) || row % 2 == 0 ? this.RowHeaderTemplate : this.AlternatingRowHeaderTemplate);

                    var tokenReplace = new TokenReplace();
                    var tokenKeyValues = new Dictionary<string, string>();

                    foreach (var col in user.Table.Columns.Cast<DataColumn>().Where(col => !tokenKeyValues.ContainsKey(col.ColumnName)))
                    {
                        tokenKeyValues.Add(col.ColumnName, user[col.ColumnName].ToString());
                    }

                    var listItem = string.IsNullOrEmpty(this.AlternatingItemTemplate) || row % 2 == 0 ? this.ItemTemplate : this.AlternatingItemTemplate;
                    listItem = tokenReplace.ReplaceEnvironmentTokens(listItem, tokenKeyValues, "Member");
                    writer.Write(listItem);

                    // Row Footer
                    writer.Write(string.IsNullOrEmpty(this.AlternatingRowFooterTemplate) || row % 2 == 0 ? this.RowFooterTemplate : this.AlternatingRowFooterTemplate);

                    row++;
                }
            }

            writer.Write(this.FooterTemplate);
        }
    }
}
