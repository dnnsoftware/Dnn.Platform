// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.Console
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web.UI.WebControls;

    using Dnn.Modules.Console.Components;
    using DotNetNuke.Abstractions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using Microsoft.Extensions.DependencyInjection;

    public partial class ViewConsole : PortalModuleBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ViewConsole));
        private readonly INavigationManager _navigationManager;
        private ConsoleController _consoleCtrl;
        private string _defaultSize = string.Empty;
        private string _defaultView = string.Empty;
        private int _groupTabID = -1;
        private IList<TabInfo> _tabs;

        public ViewConsole()
        {
            this._navigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        public bool AllowSizeChange
        {
            get { return !this.Settings.ContainsKey("AllowSizeChange") || bool.Parse(this.Settings["AllowSizeChange"].ToString()); }
        }

        public bool AllowViewChange
        {
            get { return !this.Settings.ContainsKey("AllowViewChange") || bool.Parse(this.Settings["AllowViewChange"].ToString()); }
        }

        public bool IncludeHiddenPages
        {
            get { return this.Settings.ContainsKey("IncludeHiddenPages") && bool.Parse(this.Settings["IncludeHiddenPages"].ToString()); }
        }

        public ConsoleController ConsoleCtrl
        {
            get
            {
                if (this._consoleCtrl == null)
                {
                    this._consoleCtrl = new ConsoleController();
                }

                return this._consoleCtrl;
            }
        }

        public int ConsoleTabID
        {
            get
            {
                return (this.Mode == "Profile")
                                   ? this.PortalSettings.UserTabId
                                   : (this.Settings.ContainsKey("ParentTabID")
                                        ? int.Parse(this.Settings["ParentTabID"].ToString())
                                        : this.TabId);
            }
        }

        public string ConsoleWidth
        {
            get
            {
                return this.Settings.ContainsKey("ConsoleWidth") ? this.Settings["ConsoleWidth"].ToString() : string.Empty;
            }
        }

        public string DefaultSize
        {
            get
            {
                if (this._defaultSize == string.Empty && this.AllowSizeChange && this.UserId > Null.NullInteger)
                {
                    object personalizedValue = this.GetUserSetting("DefaultSize");
                    if (personalizedValue != null)
                    {
                        this._defaultSize = Convert.ToString(personalizedValue);
                    }
                }

                if (this._defaultSize == string.Empty)
                {
                    this._defaultSize = this.Settings.ContainsKey("DefaultSize") ? Convert.ToString(this.Settings["DefaultSize"]) : "IconFile";
                }

                return this._defaultSize;
            }
        }

        public string DefaultView
        {
            get
            {
                if (this._defaultView == string.Empty && this.AllowViewChange && this.UserId > Null.NullInteger)
                {
                    object personalizedValue = this.GetUserSetting("DefaultView");
                    if (personalizedValue != null)
                    {
                        this._defaultView = Convert.ToString(personalizedValue);
                    }
                }

                if (this._defaultView == string.Empty)
                {
                    this._defaultView = this.Settings.ContainsKey("DefaultView") ? Convert.ToString(this.Settings["DefaultView"]) : "Hide";
                }

                return this._defaultView;
            }
        }

        public int GroupId
        {
            get
            {
                var groupId = Null.NullInteger;
                if (!string.IsNullOrEmpty(this.Request.Params["GroupId"]))
                {
                    groupId = int.Parse(this.Request.Params["GroupId"]);
                }

                return groupId;
            }
        }

        public bool IncludeParent
        {
            get
            {
                return (this.Mode == "Profile") || (this.Settings.ContainsKey("IncludeParent") && bool.Parse(this.Settings["IncludeParent"].ToString()));
            }
        }

        public string Mode
        {
            get
            {
                return this.Settings.ContainsKey("Mode") ? this.Settings["Mode"].ToString() : "Normal";
            }
        }

        public int ProfileUserId
        {
            get
            {
                var userId = Null.NullInteger;
                if (!string.IsNullOrEmpty(this.Request.Params["UserId"]))
                {
                    userId = int.Parse(this.Request.Params["UserId"]);
                }

                return userId;
            }
        }

        public bool ShowTooltip
        {
            get { return !this.Settings.ContainsKey("ShowTooltip") || bool.Parse(this.Settings["ShowTooltip"].ToString()); }
        }

        public bool OrderTabsByHierarchy
        {
            get
            {
                return this.Settings.ContainsKey("OrderTabsByHierarchy") && bool.Parse(this.Settings["OrderTabsByHierarchy"].ToString());
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            try
            {
                JavaScript.RequestRegistration(CommonJs.jQuery);

                ClientResourceManager.RegisterScript(this.Page, "~/desktopmodules/admin/console/scripts/jquery.console.js");

                this.DetailView.ItemDataBound += this.RepeaterItemDataBound;

                // Save User Preferences
                this.SavePersonalizedSettings();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                this.IconSize.Visible = this.AllowSizeChange;
                this.View.Visible = this.AllowViewChange;

                foreach (string val in ConsoleController.GetSizeValues())
                {
                    this.IconSize.Items.Add(new ListItem(Localization.GetString(val + ".Text", this.LocalResourceFile), val));
                }

                foreach (string val in ConsoleController.GetViewValues())
                {
                    this.View.Items.Add(new ListItem(Localization.GetString(val + ".Text", this.LocalResourceFile), val));
                }

                this.IconSize.SelectedValue = this.DefaultSize;
                this.View.SelectedValue = this.DefaultView;

                if (!this.IsPostBack)
                {
                    this.Console.Attributes["class"] = this.Console.Attributes["class"] + " " + this.Mode.ToLower(CultureInfo.InvariantCulture);

                    this.SettingsBreak.Visible = this.AllowSizeChange && this.AllowViewChange;

                    List<TabInfo> tempTabs = this.IsHostTab()
                                        ? TabController.GetTabsBySortOrder(Null.NullInteger).OrderBy(t => t.Level).ThenBy(t => t.LocalizedTabName).ToList()
                                        : TabController.GetTabsBySortOrder(this.PortalId).OrderBy(t => t.Level).ThenBy(t => t.LocalizedTabName).ToList();

                    this._tabs = new List<TabInfo>();

                    IList<int> tabIdList = new List<int>();
                    tabIdList.Add(this.ConsoleTabID);

                    if (this.IncludeParent)
                    {
                        TabInfo consoleTab = TabController.Instance.GetTab(this.ConsoleTabID, this.PortalId);
                        if (consoleTab != null)
                        {
                            this._tabs.Add(consoleTab);
                        }
                    }

                    foreach (TabInfo tab in tempTabs)
                    {
                        if (!this.CanShowTab(tab))
                        {
                            continue;
                        }

                        if (tabIdList.Contains(tab.ParentId))
                        {
                            if (!tabIdList.Contains(tab.TabID))
                            {
                                tabIdList.Add(tab.TabID);
                            }

                            this._tabs.Add(tab);
                        }
                    }

                    // if OrderTabsByHierarchy set to true, we need reorder the tab list to move tabs which have child tabs to the end of list.
                    // so that the list display in UI can show tabs in same level in same area, and not break by child tabs.
                    if (this.OrderTabsByHierarchy)
                    {
                        this._tabs = this._tabs.OrderBy(t => t.HasChildren).ToList();
                    }

                    int minLevel = -1;
                    if (this._tabs.Count > 0)
                    {
                        minLevel = this._tabs.Min(t => t.Level);
                    }

                    this.DetailView.DataSource = (minLevel > -1) ? this._tabs.Where(t => t.Level == minLevel) : this._tabs;
                    this.DetailView.DataBind();
                }

                if (this.ConsoleWidth != string.Empty)
                {
                    this.Console.Attributes.Add("style", "width:" + this.ConsoleWidth);
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected string GetHtml(TabInfo tab)
        {
            string returnValue = string.Empty;
            if (this._groupTabID > -1 && this._groupTabID != tab.ParentId)
            {
                this._groupTabID = -1;
                if (!tab.DisableLink)
                {
                    returnValue = "<br style=\"clear:both;\" /><br />";
                }
            }

            if (tab.DisableLink)
            {
                const string headerHtml = "<br style=\"clear:both;\" /><br /><h1><span class=\"TitleHead\">{0}</span></h1><br style=\"clear:both\" />";
                returnValue += string.Format(headerHtml, tab.TabName);
                this._groupTabID = tab.TabID;
            }
            else
            {
                var sb = new StringBuilder();
                if (tab.TabID == this.PortalSettings.ActiveTab.TabID)
                {
                    sb.Append("<div class=\"active console-none \">");
                }
                else
                {
                    sb.Append("<div class=\"console-none \">");
                }

                sb.Append("<a href=\"{0}\" aria-label=\"{3}\">");

                if (this.DefaultSize != "IconNone" || (this.AllowSizeChange || this.AllowViewChange))
                {
                    sb.Append("<img src=\"{1}\" alt=\"{3}\" width=\"16px\" height=\"16px\"/>");
                    sb.Append("<img src=\"{2}\" alt=\"{3}\" width=\"32px\" height=\"32px\"/>");
                }

                sb.Append("</a>");
                sb.Append("<h3>{3}</h3>");
                sb.Append("<div>{4}</div>");
                sb.Append("</div>");

                // const string contentHtml = "<div>" + "<a href=\"{0}\"><img src=\"{1}\" alt=\"{3}\" width=\"16px\" height=\"16px\"/><img src=\"{2}\" alt=\"{3}\" width=\"32px\" height=\"32px\"/></a>" + "<h3>{3}</h3>" + "<div>{4}</div>" + "</div>";
                var tabUrl = tab.FullUrl;
                if (this.ProfileUserId > -1)
                {
                    tabUrl = this._navigationManager.NavigateURL(tab.TabID, string.Empty, "UserId=" + this.ProfileUserId.ToString(CultureInfo.InvariantCulture));
                }

                if (this.GroupId > -1)
                {
                    tabUrl = this._navigationManager.NavigateURL(tab.TabID, string.Empty, "GroupId=" + this.GroupId.ToString(CultureInfo.InvariantCulture));
                }

                returnValue += string.Format(
                    sb.ToString(),
                    tabUrl,
                    this.GetIconUrl(tab.IconFile, "IconFile"),
                    this.GetIconUrl(tab.IconFileLarge, "IconFileLarge"),
                    tab.LocalizedTabName,
                    tab.Description);
            }

            return returnValue;
        }

        protected string GetClientSideSettings()
        {
            string tmid = "-1";
            if (this.UserId > -1)
            {
                tmid = this.TabModuleId.ToString(CultureInfo.InvariantCulture);
            }

            return string.Format(
                "allowIconSizeChange: {0}, allowDetailChange: {1}, selectedSize: '{2}', showDetails: '{3}', tabModuleID: {4}, showTooltip: {5}",
                this.AllowSizeChange.ToString(CultureInfo.InvariantCulture).ToLowerInvariant(),
                this.AllowViewChange.ToString(CultureInfo.InvariantCulture).ToLowerInvariant(),
                this.DefaultSize,
                this.DefaultView,
                tmid,
                this.ShowTooltip.ToString(CultureInfo.InvariantCulture).ToLowerInvariant());
        }

        private bool CanShowTab(TabInfo tab)
        {
            bool canShowTab = TabPermissionController.CanViewPage(tab) &&
                                !tab.IsDeleted &&
                                (this.IncludeHiddenPages || tab.IsVisible) &&
                                (tab.StartDate < DateTime.Now || tab.StartDate == Null.NullDate);

            if (canShowTab)
            {
                var key = string.Format("TabVisibility{0}", tab.TabPath.Replace("//", "-"));
                var visibility = this.Settings.ContainsKey(key) ? this.Settings[key].ToString() : "AllUsers";

                switch (visibility)
                {
                    case "Owner":
                        canShowTab = this.UserInfo.Social.Roles.SingleOrDefault(ur => ur.RoleID == this.GroupId && ur.IsOwner) != null;
                        break;
                    case "Members":
                        var group = RoleController.Instance.GetRole(this.PortalId, (r) => r.RoleID == this.GroupId);
                        canShowTab = (group != null) && this.UserInfo.IsInRole(group.RoleName);
                        break;
                    case "Friends":
                        var profileUser = UserController.GetUserById(this.PortalId, this.ProfileUserId);
                        canShowTab = ((profileUser != null) && (profileUser.Social.Friend != null)) || (this.UserId == this.ProfileUserId);
                        break;
                    case "User":
                        canShowTab = this.UserId == this.ProfileUserId;
                        break;
                    case "AllUsers":
                        break;
                }
            }

            return canShowTab;
        }

        private string GetIconUrl(string iconURL, string size)
        {
            if (string.IsNullOrEmpty(iconURL))
            {
                iconURL = (size == "IconFile") ? "~/images/icon_unknown_16px.gif" : "~/images/icon_unknown_32px.gif";
            }

            if (iconURL.Contains("~") == false)
            {
                iconURL = Path.Combine(this.PortalSettings.HomeDirectory, iconURL);
            }

            return this.ResolveUrl(iconURL);
        }

        private object GetUserSetting(string key)
        {
            return Personalization.GetProfile(this.ModuleConfiguration.ModuleDefinition.FriendlyName, this.PersonalizationKey(key));
        }

        private bool IsHostTab()
        {
            var returnValue = false;
            if (this.ConsoleTabID != this.TabId)
            {
                if (this.UserInfo != null && this.UserInfo.IsSuperUser)
                {
                    var hostTabs = TabController.Instance.GetTabsByPortal(Null.NullInteger);
                    if (hostTabs.Keys.Any(key => key == this.ConsoleTabID))
                    {
                        returnValue = true;
                    }
                }
            }
            else
            {
                returnValue = this.PortalSettings.ActiveTab.IsSuperTab;
            }

            return returnValue;
        }

        private string PersonalizationKey(string key)
        {
            return string.Format("{0}_{1}_{2}", this.PortalId, this.TabModuleId, key);
        }

        private void SavePersonalizedSettings()
        {
            if (this.UserId > -1)
            {
                int consoleModuleID = -1;
                try
                {
                    if (this.Request.QueryString["CTMID"] != null)
                    {
                        consoleModuleID = Convert.ToInt32(this.Request.QueryString["CTMID"]);
                    }
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);

                    consoleModuleID = -1;
                }

                if (consoleModuleID == this.TabModuleId)
                {
                    string consoleSize = string.Empty;
                    if (this.Request.QueryString["CS"] != null)
                    {
                        consoleSize = this.Request.QueryString["CS"];
                    }

                    string consoleView = string.Empty;
                    if (this.Request.QueryString["CV"] != null)
                    {
                        consoleView = this.Request.QueryString["CV"];
                    }

                    if (consoleSize != string.Empty && ConsoleController.GetSizeValues().Contains(consoleSize))
                    {
                        this.SaveUserSetting("DefaultSize", consoleSize);
                    }

                    if (consoleView != string.Empty && ConsoleController.GetViewValues().Contains(consoleView))
                    {
                        this.SaveUserSetting("DefaultView", consoleView);
                    }
                }
            }
        }

        private void SaveUserSetting(string key, object val)
        {
            Personalization.SetProfile(this.ModuleConfiguration.ModuleDefinition.FriendlyName, this.PersonalizationKey(key), val);
        }

        private void RepeaterItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            var tab = e.Item.DataItem as TabInfo;
            e.Item.Controls.Add(new Literal() { Text = this.GetHtml(tab) });
            if (this._tabs.Any(t => t.ParentId == tab.TabID))
            {
                var repeater = new Repeater();
                repeater.ItemDataBound += this.RepeaterItemDataBound;
                e.Item.Controls.Add(repeater);
                repeater.DataSource = this._tabs.Where(t => t.ParentId == tab.TabID);
                repeater.DataBind();
            }
        }
    }
}
