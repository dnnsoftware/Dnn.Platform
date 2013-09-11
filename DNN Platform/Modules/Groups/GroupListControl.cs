using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Entities.Users;
using System.Collections;

using DotNetNuke.Modules.Groups.Components;
using DotNetNuke.Services.Tokens;
using DotNetNuke.UI.WebControls;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Roles;
using DotNetNuke.Security.Roles.Internal;
using DotNetNuke.Entities.Groups;
using DotNetNuke.Common;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;
namespace DotNetNuke.Modules.Groups.Controls
{
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:GroupListControl runat=server></{0}:GroupListControl>")]

    public class GroupListControl : WebControl
    {
        [DefaultValue(""), PersistenceMode(PersistenceMode.InnerProperty)]
        public String ItemTemplate { get; set; }

        [DefaultValue(""), PersistenceMode(PersistenceMode.InnerProperty)]
        public String HeaderTemplate { get; set; }

        [DefaultValue(""), PersistenceMode(PersistenceMode.InnerProperty)]
        public String FooterTemplate { get; set; }

        [DefaultValue(""), PersistenceMode(PersistenceMode.InnerProperty)]
        public String RowHeaderTemplate { get; set; }

        [DefaultValue(""), PersistenceMode(PersistenceMode.InnerProperty)]
        public String RowFooterTemplate { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public PortalSettings PortalSettings
        {
            get
            {
                return PortalController.GetCurrentPortalSettings();
            }
        }

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

        public UserInfo currentUser;

        public int GroupViewTabId { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            currentUser = UserController.GetCurrentUserInfo();

        }

        private static bool TestPredicateGroup(List<Func<RoleInfo, bool>> predicates, RoleInfo ri)
        {
            foreach (var p in predicates)
            {
                if (!p(ri))
                {
                    return false;
                }
            }

            return true;
        }

        private static object GetOrderByProperty(object obj, string property)
        {
            var propertyInfo = obj.GetType().GetProperty(property, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            return propertyInfo == null ? null : propertyInfo.GetValue(obj, null);
        }

        protected override void Render(HtmlTextWriter output)
        {

            var rc = new RoleController();

            var whereCls = new List<Func<RoleInfo, bool>>();
            whereCls.Add(grp => grp.SecurityMode != SecurityMode.SecurityRole);
	        if (RoleGroupId >= -1)
	        {
		        whereCls.Add(grp => grp.RoleGroupID == RoleGroupId);
	        }
	        whereCls.Add(grp => grp.Status == RoleStatus.Approved);

            if (DisplayCurrentUserGroups)
                whereCls.Add(grp => currentUser.IsInRole(grp.RoleName));
            else
                whereCls.Add(grp => grp.IsPublic || currentUser.IsInRole(grp.RoleName) || currentUser.IsInRole(PortalSettings.AdministratorRoleName));

            if (!string.IsNullOrEmpty(SearchFilter))
            {
                whereCls.Add(grp => grp.RoleName.ToLower().Contains(SearchFilter.ToLower()) || grp.Description.ToLower().Contains(SearchFilter.ToLower()));
            }

            var roles = TestableRoleController.Instance.GetRoles(PortalSettings.PortalId, grp => TestPredicateGroup(whereCls, grp));

            if (SortDirection.ToLower() == "asc")
                roles = roles.OrderBy(info => GetOrderByProperty(info, SortField)).ToList();
            else
                roles = roles.OrderByDescending(info => GetOrderByProperty(info, SortField)).ToList();

            decimal pages = (decimal)roles.Count / (decimal)PageSize;


            output.Write(HeaderTemplate);


            ItemTemplate = ItemTemplate.Replace("{resx:posts}", Localization.GetString("posts", Constants.SharedResourcesPath));
            ItemTemplate = ItemTemplate.Replace("{resx:members}", Localization.GetString("members", Constants.SharedResourcesPath));
            ItemTemplate = ItemTemplate.Replace("{resx:photos}", Localization.GetString("photos", Constants.SharedResourcesPath));
            ItemTemplate = ItemTemplate.Replace("{resx:documents}", Localization.GetString("documents", Constants.SharedResourcesPath));

            ItemTemplate = ItemTemplate.Replace("{resx:Join}", Localization.GetString("Join", Constants.SharedResourcesPath));
            ItemTemplate = ItemTemplate.Replace("{resx:Pending}", Localization.GetString("Pending", Constants.SharedResourcesPath));
            ItemTemplate = ItemTemplate.Replace("{resx:LeaveGroup}", Localization.GetString("LeaveGroup", Constants.SharedResourcesPath));
            ItemTemplate = ItemTemplate.Replace("[GroupViewTabId]", GroupViewTabId.ToString());
            
            if (roles.Count == 0)
                output.Write(String.Format("<div class=\"dnnFormMessage dnnFormInfo\"><span>{0}</span></div>", Localization.GetString("NoGroupsFound", Constants.SharedResourcesPath)));


            if (!string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["page"]))
            {
                CurrentIndex = Convert.ToInt32(HttpContext.Current.Request.QueryString["page"].ToString());
                CurrentIndex = CurrentIndex - 1;
            }

            int rowItem = 0;
            int recordStart = (CurrentIndex * PageSize);

            if (CurrentIndex == 0)
                recordStart = 0;


            for (int x = recordStart; x < (recordStart + PageSize); x++)
            {
                if (x > roles.Count - 1)
                    break;

                var role = roles[x];

                string rowTemplate = ItemTemplate;

                if (rowItem == 0)
                    output.Write(RowHeaderTemplate);

                var groupParser = new GroupViewParser(PortalSettings, role, currentUser, rowTemplate, GroupViewTabId);
                output.Write(groupParser.ParseView());

                rowItem += 1;

                if (rowItem == ItemsPerRow)
                {
                    output.Write(RowFooterTemplate);
                    rowItem = 0;
                }
            }

            if (rowItem > 0)
                output.Write(RowFooterTemplate);


            output.Write(FooterTemplate);

            int TotalPages = Convert.ToInt32(System.Math.Ceiling(pages));


            if (TotalPages == 0)
                TotalPages = 1;

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

                    string sUrl = Utilities.NavigateUrl(TabId, @params);

                    string cssClass = "pagerItem";

                    if (x - 1 == CurrentIndex)
                        cssClass = "pagerItemSelected";


                    sb.AppendFormat(sUrlFormat, sUrl, cssClass, x.ToString());
                }

            }

            output.Write("<div class=\"dnnClear groupPager\">");
            output.Write(sb.ToString());
            output.Write("</div>");

        }


    }

}
