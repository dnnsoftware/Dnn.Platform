#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

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

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    /// <summary>
    /// This control is used for displaying a template based list of users based upon various filter and sorting capabilities.
    /// </summary>
    [ToolboxData("<{0}:DnnMemberListControl runat=\"server\"></{0}:DnnMemberListControl>")]
    public class DnnMemberListControl : WebControl
    {
        #region Private Variables

        private UserInfo _currentUser;
        private RelationshipController _relationshipController;

        #endregion

        #region Properties

        #region Layout Properties

        /// <summary>
        /// Gets or sets the template for displaying the header section of a DnnMemberListControl object.
        /// </summary>
        [DefaultValue(""), PersistenceMode(PersistenceMode.InnerProperty)]
        public string HeaderTemplate { get; set; }
        
        /// <summary>
        /// Gets or sets the template for the row header.
        /// </summary>
        [DefaultValue(""), PersistenceMode(PersistenceMode.InnerProperty)]
        public string RowHeaderTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for displaying an item in a DnnMemberListControl object.
        /// </summary>
        [DefaultValue(""), PersistenceMode(PersistenceMode.InnerProperty)]
        public string ItemTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for the row footer.
        /// </summary>
        [DefaultValue(""), PersistenceMode(PersistenceMode.InnerProperty)]
        public string RowFooterTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for displaying the alternating row headers in a DnnMemberListControl object.
        /// </summary>
        [DefaultValue(""), PersistenceMode(PersistenceMode.InnerProperty)]
        public string AlternatingRowHeaderTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for displaying the alternating items in a DnnMemberListControl object.
        /// </summary>
        [DefaultValue(""), PersistenceMode(PersistenceMode.InnerProperty)]
        public string AlternatingItemTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for displaying the alternating row footers in a DnnMemberListControl object.
        /// </summary>
        [DefaultValue(""), PersistenceMode(PersistenceMode.InnerProperty)]
        public string AlternatingRowFooterTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for displaying the footer section of a DnnMemberListControl object.
        /// </summary>
        [DefaultValue(""), PersistenceMode(PersistenceMode.InnerProperty)]
        public string FooterTemplate { get; set; }

        #endregion

        #region Filter Properties
        
        /// <summary>
        /// Gets or sets the index of the currently displayed page.
        /// </summary>
        [DefaultValue(1)]
        public int PageIndex { get; set; }

        /// <summary>
        /// Gets or sets the number of records to display on a page in a DnnMemberListControl object.
        /// </summary>
        [DefaultValue(10)]
        public int PageSize { get; set; }
        
        /// <summary>
        /// Gets or sets the number of items displayed on each row.
        /// </summary>
        [DefaultValue(1)]
        public int RowSize { get; set; }
        
        /// <summary>
        /// Sets the property value to sort by.
        /// </summary>
        [DefaultValue("UserId")]
        public string SortBy { get; set; }

        /// <summary>
        /// Gets or sets the sort direction
        /// </summary>
        [DefaultValue(true)]
        public bool SortAscending { get; set; }

        /// <summary>
        /// Gets or sets the collection of filters to apply when getting the list of members.
        /// </summary>
        /// <remarks>
        /// Posible keys are: RoleId, RelationshipTypeId, UserId, Profile:PropertyName, FirstName, LastName, DisplayName, Username, Email.
        /// </remarks>
        public IDictionary<string, string> Filters { get; set; }

        #endregion
        
        #endregion
        
        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            
            _currentUser = UserController.Instance.GetCurrentUserInfo();
            _relationshipController = new RelationshipController();
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (ItemTemplate == "") return;

            writer.Write(HeaderTemplate);

            // Filters
            if (Filters == null) Filters = new Dictionary<string, string>();
            var additionalFilters = new Dictionary<string, string>();
            additionalFilters.Add("Records", PageSize.ToString());
            additionalFilters.Add("PageIndex", PageIndex.ToString());
            additionalFilters.Add("Rowsize", RowSize.ToString());
            additionalFilters.Add("SortBy", SortBy);
            additionalFilters.Add("SortAscending", SortAscending.ToString());

            // Currently Not Used by the SPROC
            var filterUser = Filters.ContainsKey("UserId") && Filters["UserId"] != null ? new UserInfo() { UserID = int.Parse(Filters["UserId"]) } : new UserInfo() { PortalID = _currentUser.PortalID };
            var role = Filters.ContainsKey("RoleId") && Filters["RoleId"] != null ? new UserRoleInfo() { RoleID = int.Parse(Filters["RoleId"]) } : null;
            var relationship = Filters.ContainsKey("RelationshipTypeId") && Filters["RelationshipTypeId"] != null ? new RelationshipType() { RelationshipTypeId = int.Parse(Filters["RelationshipTypeId"]) } : null;
            
            foreach (var filter in Filters.Where(filter => !additionalFilters.ContainsKey(filter.Key)))
            {
                additionalFilters.Add(filter.Key, filter.Value);
            }

            var row = 0;
            var users = new DataTable();

            //users.Load(_relationshipController.GetUsersAdvancedSearch(_currentUser, filterUser, role, relationship, Filters, additionalFilters));

            if (users.Rows.Count > 0)
            {
                foreach (DataRow user in users.Rows)
                {
                    //Row Header
                    writer.Write(string.IsNullOrEmpty(AlternatingRowHeaderTemplate) || row%2 == 0 ? RowHeaderTemplate : AlternatingRowHeaderTemplate);

                    var tokenReplace = new TokenReplace();
                    var tokenKeyValues = new Dictionary<string, string>();

                    foreach (var col in user.Table.Columns.Cast<DataColumn>().Where(col => !tokenKeyValues.ContainsKey(col.ColumnName)))
                    {
                        tokenKeyValues.Add(col.ColumnName, user[col.ColumnName].ToString());
                    }

                    var listItem = string.IsNullOrEmpty(AlternatingItemTemplate) || row%2 == 0 ? ItemTemplate : AlternatingItemTemplate;
                    listItem = tokenReplace.ReplaceEnvironmentTokens(listItem, tokenKeyValues, "Member");
                    writer.Write(listItem);

                    //Row Footer
                    writer.Write(string.IsNullOrEmpty(AlternatingRowFooterTemplate) || row%2 == 0 ? RowFooterTemplate : AlternatingRowFooterTemplate);

                    row++;
                }
            }

            writer.Write(FooterTemplate);
        }

        #endregion
    }
}
