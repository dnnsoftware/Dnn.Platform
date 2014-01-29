#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Tabs.Internal;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Modules.Console.Components;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles.Internal;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Personalization;
using DotNetNuke.Web.Client.ClientResourceManagement;

#endregion

namespace DesktopModules.Admin.Console
{
	public partial class ViewConsole : PortalModuleBase
	{
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (ViewConsole));
	    private ConsoleController _consoleCtrl;
		private string _defaultSize = string.Empty;
		private string _defaultView = string.Empty;
	    private int _groupTabID = -1;
		private IList<TabInfo> _tabs; 

        #region Public Properties

        public bool AllowSizeChange
        {
            get { return !Settings.ContainsKey("AllowSizeChange") || bool.Parse(Settings["AllowSizeChange"].ToString()); }
        }

        public bool AllowViewChange
        {
            get { return !Settings.ContainsKey("AllowViewChange") || bool.Parse(Settings["AllowViewChange"].ToString()); }
        }

        public ConsoleController ConsoleCtrl
		{
			get
			{
				if ((_consoleCtrl == null))
				{
					_consoleCtrl = new ConsoleController();
				}
				return _consoleCtrl;
			}
		}

		public int ConsoleTabID
		{
			get
			{
                return (Mode == "Profile")
                                   ? PortalSettings.UserTabId
                                   : (Settings.ContainsKey("ParentTabID")
                                        ? int.Parse(Settings["ParentTabID"].ToString())
                                        : TabId);
			}
		}

        public string ConsoleWidth
        {
            get
            {
                return Settings.ContainsKey("ConsoleWidth") ? Settings["ConsoleWidth"].ToString() : String.Empty;
            }
        }

		public string DefaultSize
		{
			get
			{
				if ((_defaultSize == string.Empty && AllowSizeChange && UserId > Null.NullInteger))
				{
					object personalizedValue = GetUserSetting("DefaultSize");
					if ((personalizedValue != null))
					{
						_defaultSize = Convert.ToString(personalizedValue);
					}
				}
				if ((_defaultSize == string.Empty))
				{
					_defaultSize = Settings.ContainsKey("DefaultSize") ? Convert.ToString(Settings["DefaultSize"]) : "IconFile";
				}
				return _defaultSize;
			}
		}

		public string DefaultView
		{
			get
			{
				if ((_defaultView == string.Empty && AllowViewChange && UserId > Null.NullInteger))
				{
					object personalizedValue = GetUserSetting("DefaultView");
					if ((personalizedValue != null))
					{
						_defaultView = Convert.ToString(personalizedValue);
					}
				}
				if ((_defaultView == string.Empty))
				{
					_defaultView = Settings.ContainsKey("DefaultView") ? Convert.ToString(Settings["DefaultView"]) : "Hide";
				}
				return _defaultView;
			}
		}

        public int GroupId
        {
            get
            {
                var groupId = Null.NullInteger;
                if (!string.IsNullOrEmpty(Request.Params["GroupId"]))
                {
                    groupId = Int32.Parse(Request.Params["GroupId"]);
                }
                return groupId;
            }
        }

        public bool IncludeParent
        {
            get
            {
                return (Mode == "Profile") || (Settings.ContainsKey("IncludeParent") && bool.Parse(Settings["IncludeParent"].ToString()));
            }
        }

        public string Mode
        {
            get
            {
                return Settings.ContainsKey("Mode") ? Settings["Mode"].ToString() : "Normal";
            }
        }

        public int ProfileUserId
        {
            get
            {
                var userId = Null.NullInteger;
                if (!string.IsNullOrEmpty(Request.Params["UserId"]))
                {
                    userId = Int32.Parse(Request.Params["UserId"]);
                }
                return userId;
            }
        }

        public bool ShowTooltip
        {
            get { return !Settings.ContainsKey("ShowTooltip") || bool.Parse(Settings["ShowTooltip"].ToString()); }
        }

		public bool OrderTabsByHierarchy
		{
			get
			{
				return Settings.ContainsKey("OrderTabsByHierarchy") && bool.Parse(Settings["OrderTabsByHierarchy"].ToString());
			}
		}

        #endregion

        #region Private Methods

        private bool CanShowTab(TabInfo tab)
        {
            bool canShowTab = TabPermissionController.CanViewPage(tab) &&
                                !tab.IsDeleted &&
                                tab.IsVisible &&
                                (tab.StartDate < DateTime.Now || tab.StartDate == Null.NullDate);

            if (canShowTab)
            {
                var key = String.Format("TabVisibility{0}", tab.TabPath.Replace("//", "-"));
                var visibility = Settings.ContainsKey(key) ? Settings[key].ToString() : "AllUsers";

                switch (visibility)
                {
                    case "Owner":
                        canShowTab = (UserInfo.Social.Roles.SingleOrDefault(ur => ur.RoleID == GroupId && ur.IsOwner) != null);
                        break;
                    case "Members":
                        var group = TestableRoleController.Instance.GetRole(PortalId, (r) => r.RoleID == GroupId);
                        canShowTab = (group != null) && UserInfo.IsInRole(group.RoleName);
                        break;
                    case "Friends":
                        var profileUser = UserController.GetUserById(PortalId, ProfileUserId);
                        canShowTab = (profileUser != null) && (profileUser.Social.Friend != null) || (UserId == ProfileUserId);
                        break;
                    case "User":
                        canShowTab = (UserId == ProfileUserId);
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
                iconURL = Path.Combine(PortalSettings.HomeDirectory, iconURL);
            }
            return ResolveUrl(iconURL);
        }

        private object GetUserSetting(string key)
        {
            return Personalization.GetProfile(ModuleConfiguration.ModuleDefinition.FriendlyName, PersonalizationKey(key));
        }

        private bool IsHostTab()
        {
            var returnValue = false;
            if (ConsoleTabID != TabId)
            {
                if (UserInfo != null && UserInfo.IsSuperUser)
                {
                    var hostTabs = new TabController().GetTabsByPortal(Null.NullInteger);
                    if (hostTabs.Keys.Any(key => key == ConsoleTabID))
                    {
                        returnValue = true;
                    }
                }
            }
            else
            {
                returnValue = PortalSettings.ActiveTab.IsSuperTab;
            }
            return returnValue;
        }

        private string PersonalizationKey(string key)
        {
            return string.Format("{0}_{1}_{2}", PortalId, TabModuleId, key);
        }

        private void SavePersonalizedSettings()
        {
            if ((UserId > -1))
            {
                int consoleModuleID = -1;
                try
                {
                    if (Request.QueryString["CTMID"] != null)
                    {
                        consoleModuleID = Convert.ToInt32(Request.QueryString["CTMID"]);
                    }
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);

                    consoleModuleID = -1;
                }
                if ((consoleModuleID == TabModuleId))
                {
                    string consoleSize = string.Empty;
                    if (Request.QueryString["CS"] != null)
                    {
                        consoleSize = Request.QueryString["CS"];
                    }
                    string consoleView = string.Empty;
                    if (Request.QueryString["CV"] != null)
                    {
                        consoleView = Request.QueryString["CV"];
                    }
                    if ((consoleSize != string.Empty && ConsoleController.GetSizeValues().Contains(consoleSize)))
                    {
                        SaveUserSetting("DefaultSize", consoleSize);
                    }
                    if ((consoleView != string.Empty && ConsoleController.GetViewValues().Contains(consoleView)))
                    {
                        SaveUserSetting("DefaultView", consoleView);
                    }
                }
            }
        }

        private void SaveUserSetting(string key, object val)
        {
            Personalization.SetProfile(ModuleConfiguration.ModuleDefinition.FriendlyName, PersonalizationKey(key), val);
        }

        #endregion

        protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			try
			{
				jQuery.RequestRegistration();

                ClientResourceManager.RegisterScript(Page, "~/desktopmodules/admin/console/jquery.console.js");

				DetailView.ItemDataBound += RepeaterItemDataBound;

				//Save User Preferences
				SavePersonalizedSettings();
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
				if ((!IsPostBack))
				{
					IconSize.Visible = AllowSizeChange;
					View.Visible = AllowViewChange;

					foreach (string val in ConsoleController.GetSizeValues())
					{
						IconSize.Items.Add(new ListItem(Localization.GetString(val + ".Text", LocalResourceFile), val));
                        //IconSize.AddItem(Localization.GetString(val + ".Text", LocalResourceFile), val);
					}
					foreach (string val in ConsoleController.GetViewValues())
					{
						View.Items.Add(new ListItem(Localization.GetString(val + ".Text", LocalResourceFile), val));
                        //View.AddItem(Localization.GetString(val + ".Text", LocalResourceFile), val);
					}
					IconSize.SelectedValue = DefaultSize;
					View.SelectedValue = DefaultView;

					SettingsBreak.Visible = (IconSize.Visible && View.Visible);

				    List<TabInfo> tempTabs = (IsHostTab())
										? TabController.GetTabsBySortOrder(Null.NullInteger).OrderBy(t => t.Level).ThenBy(t => t.LocalizedTabName).ToList()
										: TabController.GetTabsBySortOrder(PortalId).OrderBy(t => t.Level).ThenBy(t => t.LocalizedTabName).ToList();

					_tabs = new List<TabInfo>();

					IList<int> tabIdList = new List<int>();
					tabIdList.Add(ConsoleTabID);

                    if(IncludeParent)
                    {
                        TabInfo consoleTab = TestableTabController.Instance.GetTab(ConsoleTabID, PortalId);
                        if (consoleTab != null)
                        {
							_tabs.Add(consoleTab);
                        }
                    }

					foreach (TabInfo tab in tempTabs)
					{
						if ((!CanShowTab(tab)))
						{
							continue;
						}
						if ((tabIdList.Contains(tab.ParentId)))
						{
							if ((!tabIdList.Contains(tab.TabID)))
							{
								tabIdList.Add(tab.TabID);
							}
							_tabs.Add(tab);  
						}
					}

					//if OrderTabsByHierarchy set to true, we need reorder the tab list to move tabs which have child tabs to the end of list.
					//so that the list display in UI can show tabs in same level in same area, and not break by child tabs.
					if (OrderTabsByHierarchy)
					{
						_tabs = _tabs.OrderBy(t => t.HasChildren).ToList();
					}

				    int minLevel = -1;
                    if (_tabs.Count > 0)
                    {
                        minLevel = _tabs.Min(t => t.Level);
                    }
					DetailView.DataSource = (minLevel > -1) ? _tabs.Where(t => t.Level == minLevel) : _tabs;
					DetailView.DataBind();
				}
				if ((ConsoleWidth != string.Empty))
				{
					Console.Attributes.Add("style", "width:" + ConsoleWidth);
				}
			}
			catch (Exception exc)
			{
				Exceptions.ProcessModuleLoadException(this, exc);
			}
		}

		private void RepeaterItemDataBound(object sender, RepeaterItemEventArgs e)
		{
			var tab = e.Item.DataItem as TabInfo;
			e.Item.Controls.Add(new Literal() { Text = GetHtml(tab) });
			if (_tabs.Any(t => t.ParentId == tab.TabID))
			{
				var repeater = new Repeater();
				repeater.ItemDataBound += RepeaterItemDataBound;
				e.Item.Controls.Add(repeater);
				repeater.DataSource = _tabs.Where(t => t.ParentId == tab.TabID);
				repeater.DataBind();
			}
		}

		protected string GetHtml(TabInfo tab)
		{
			string returnValue = string.Empty;
			if ((_groupTabID > -1 && _groupTabID != tab.ParentId))
			{
				_groupTabID = -1;
				if ((!tab.DisableLink))
				{
					returnValue = "<br style=\"clear:both;\" /><br />";
				}
			}
			if ((tab.DisableLink))
			{
				const string headerHtml = "<br style=\"clear:both;\" /><br /><h1><span class=\"TitleHead\">{0}</span></h1><br style=\"clear:both\" />";
				returnValue += string.Format(headerHtml, tab.TabName);
				_groupTabID = tab.TabID;
			}
			else
			{
			    var sb = new StringBuilder();
                if(tab.TabID == PortalSettings.ActiveTab.TabID)
                {
                    sb.Append("<div class=\"active console-none \">");
                }
                else
                {
                    sb.Append("<div class=\"console-none \">");
                }
                sb.Append("<a href=\"{0}\">");

                if (DefaultSize != "IconNone" || (AllowSizeChange || AllowViewChange))
                {
                    sb.Append("<img src=\"{1}\" alt=\"{3}\" width=\"16px\" height=\"16px\"/>");
                    sb.Append("<img src=\"{2}\" alt=\"{3}\" width=\"32px\" height=\"32px\"/>");
                }
                sb.Append("</a>");
                sb.Append("<h3>{3}</h3>");
                sb.Append("<div>{4}</div>");
			    sb.Append("</div>");

                //const string contentHtml = "<div>" + "<a href=\"{0}\"><img src=\"{1}\" alt=\"{3}\" width=\"16px\" height=\"16px\"/><img src=\"{2}\" alt=\"{3}\" width=\"32px\" height=\"32px\"/></a>" + "<h3>{3}</h3>" + "<div>{4}</div>" + "</div>";

			    var tabUrl = tab.FullUrl;
                if (ProfileUserId > -1)
                {
                    tabUrl = Globals.NavigateURL(tab.TabID, "", "UserId=" + ProfileUserId.ToString(CultureInfo.InvariantCulture));
                }

                if (GroupId > -1)
                {
                    tabUrl = Globals.NavigateURL(tab.TabID, "", "GroupId=" + GroupId.ToString(CultureInfo.InvariantCulture));
                }

				returnValue += string.Format(sb.ToString(),
                                             tabUrl,
											 GetIconUrl(tab.IconFile, "IconFile"),
											 GetIconUrl(tab.IconFileLarge, "IconFileLarge"),
											 tab.LocalizedTabName,
											 tab.Description);
			}
			return returnValue;
		}

		protected string GetClientSideSettings()
		{
			string tmid = "-1";
			if ((UserId > -1))
			{
				tmid = TabModuleId.ToString(CultureInfo.InvariantCulture);
			}
			return string.Format("allowIconSizeChange: {0}, allowDetailChange: {1}, selectedSize: '{2}', showDetails: '{3}', tabModuleID: {4}, showTooltip: {5}",
								 AllowSizeChange.ToString(CultureInfo.InvariantCulture).ToLower(),
								 AllowViewChange.ToString(CultureInfo.InvariantCulture).ToLower(),
								 DefaultSize,
								 DefaultView,
								 tmid,
								 ShowTooltip.ToString(CultureInfo.InvariantCulture).ToLower());
		}

	}
}