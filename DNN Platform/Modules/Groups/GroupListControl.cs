// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Groups.Controls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Modules.Groups.Components;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Localization;

    [DefaultProperty("Text")]
    [ToolboxData("<{0}:GroupListControl runat=server></{0}:GroupListControl>")]

    public class GroupListControl : WebControl
    {
        public UserInfo currentUser;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public PortalSettings PortalSettings
        {
            get
            {
                return PortalController.Instance.GetCurrentPortalSettings();
            }
        }

        [DefaultValue("")]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public string ItemTemplate { get; set; }

        [DefaultValue("")]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public string HeaderTemplate { get; set; }

        [DefaultValue("")]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public string FooterTemplate { get; set; }

        [DefaultValue("")]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public string RowHeaderTemplate { get; set; }

        [DefaultValue("")]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public string RowFooterTemplate { get; set; }

        [DefaultValue(1)]
        public int ItemsPerRow { get; set; }

        [DefaultValue(0)]
        public int RoleGroupId { get; set; }

        [DefaultValue(20)]
        public int PageSize { get; set; }

        [DefaultValue(0)]
        public int CurrentIndex { get; set; }

        [DefaultValue(-1)]
        public int TabId { get; set; }

        [DefaultValue("")]
        public string SearchFilter { get; set; }

        [DefaultValue("rolename")]
        public string SortField { get; set; }

        [DefaultValue("asc")]
        public string SortDirection { get; set; }

        [DefaultValue(false)]
        public bool DisplayCurrentUserGroups { get; set; }

        public int GroupViewTabId { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.currentUser = UserController.Instance.GetCurrentUserInfo();
        }

        protected override void Render(HtmlTextWriter output)
        {
            var whereCls = new List<Func<RoleInfo, bool>>
            {
                grp => grp.SecurityMode != SecurityMode.SecurityRole && grp.Status == RoleStatus.Approved,
            };

            if (this.RoleGroupId >= -1)
            {
                whereCls.Add(grp => grp.RoleGroupID == this.RoleGroupId);
            }

            if (this.DisplayCurrentUserGroups)
            {
                whereCls.Add(grp => this.currentUser.IsInRole(grp.RoleName));
            }
            else
            {
                whereCls.Add(grp => grp.IsPublic || this.currentUser.IsInRole(grp.RoleName) || this.currentUser.IsInRole(this.PortalSettings.AdministratorRoleName));
            }

            if (!string.IsNullOrEmpty(this.SearchFilter))
            {
                whereCls.Add(grp => grp.RoleName.ToLowerInvariant().Contains(this.SearchFilter.ToLowerInvariant()) || grp.Description.ToLowerInvariant().Contains(this.SearchFilter.ToLowerInvariant()));
            }

            var roles = RoleController.Instance.GetRoles(this.PortalSettings.PortalId, grp => TestPredicateGroup(whereCls, grp));

            if (this.SortDirection.ToLowerInvariant() == "asc")
            {
                roles = roles.OrderBy(info => GetOrderByProperty(info, this.SortField)).ToList();
            }
            else
            {
                roles = roles.OrderByDescending(info => GetOrderByProperty(info, this.SortField)).ToList();
            }

            decimal pages = (decimal)roles.Count / (decimal)this.PageSize;

            output.Write(this.HeaderTemplate);

            this.ItemTemplate = this.ItemTemplate.Replace("{resx:posts}", Localization.GetString("posts", Constants.SharedResourcesPath));
            this.ItemTemplate = this.ItemTemplate.Replace("{resx:members}", Localization.GetString("members", Constants.SharedResourcesPath));
            this.ItemTemplate = this.ItemTemplate.Replace("{resx:photos}", Localization.GetString("photos", Constants.SharedResourcesPath));
            this.ItemTemplate = this.ItemTemplate.Replace("{resx:documents}", Localization.GetString("documents", Constants.SharedResourcesPath));

            this.ItemTemplate = this.ItemTemplate.Replace("{resx:Join}", Localization.GetString("Join", Constants.SharedResourcesPath));
            this.ItemTemplate = this.ItemTemplate.Replace("{resx:Pending}", Localization.GetString("Pending", Constants.SharedResourcesPath));
            this.ItemTemplate = this.ItemTemplate.Replace("{resx:LeaveGroup}", Localization.GetString("LeaveGroup", Constants.SharedResourcesPath));
            this.ItemTemplate = this.ItemTemplate.Replace("[GroupViewTabId]", this.GroupViewTabId.ToString());

            if (roles.Count == 0)
            {
                output.Write(string.Format("<div class=\"dnnFormMessage dnnFormInfo\"><span>{0}</span></div>", Localization.GetString("NoGroupsFound", Constants.SharedResourcesPath)));
            }

            if (!string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["page"]))
            {
                this.CurrentIndex = Convert.ToInt32(HttpContext.Current.Request.QueryString["page"].ToString());
                this.CurrentIndex = this.CurrentIndex - 1;
            }

            int rowItem = 0;
            int recordStart = this.CurrentIndex * this.PageSize;

            if (this.CurrentIndex == 0)
            {
                recordStart = 0;
            }

            for (int x = recordStart; x < (recordStart + this.PageSize); x++)
            {
                if (x > roles.Count - 1)
                {
                    break;
                }

                var role = roles[x];

                string rowTemplate = this.ItemTemplate;

                if (rowItem == 0)
                {
                    output.Write(this.RowHeaderTemplate);
                }

                var groupParser = new GroupViewParser(this.PortalSettings, role, this.currentUser, rowTemplate, this.GroupViewTabId);
                output.Write(groupParser.ParseView());

                rowItem += 1;

                if (rowItem == this.ItemsPerRow)
                {
                    output.Write(this.RowFooterTemplate);
                    rowItem = 0;
                }
            }

            if (rowItem > 0)
            {
                output.Write(this.RowFooterTemplate);
            }

            output.Write(this.FooterTemplate);

            int TotalPages = Convert.ToInt32(System.Math.Ceiling(pages));

            if (TotalPages == 0)
            {
                TotalPages = 1;
            }

            string sUrlFormat = "<a href=\"{0}\" class=\"{1}\">{2}</a>";
            string[] currParams = new string[] { };

            StringBuilder sb = new StringBuilder();

            if (TotalPages > 1)
            {
                for (int x = 1; x <= TotalPages; x++)
                {
                    string[] @params = new string[] { };

                    if (currParams.Length > 0 & x > 1)
                    {
                        @params = Utilities.AddParams("page=" + x.ToString(), currParams);
                    }
                    else if (currParams.Length > 0 & x == 1)
                    {
                        @params = currParams;
                    }
                    else if (x > 1)
                    {
                        @params = new string[] { "page=" + x.ToString() };
                    }

                    string sUrl = Utilities.NavigateUrl(this.TabId, @params);

                    string cssClass = "pagerItem";

                    if (x - 1 == this.CurrentIndex)
                    {
                        cssClass = "pagerItemSelected";
                    }

                    sb.AppendFormat(sUrlFormat, sUrl, cssClass, x.ToString());
                }
            }

            output.Write("<div class=\"dnnClear groupPager\">");
            output.Write(sb.ToString());
            output.Write("</div>");
        }

        private static bool TestPredicateGroup(IEnumerable<Func<RoleInfo, bool>> predicates, RoleInfo ri)
        {
            return predicates.All(p => p(ri));
        }

        private static object GetOrderByProperty(object obj, string property)
        {
            var propertyInfo = obj.GetType().GetProperty(property, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            return propertyInfo == null ? null : propertyInfo.GetValue(obj, null);
        }
    }
}
