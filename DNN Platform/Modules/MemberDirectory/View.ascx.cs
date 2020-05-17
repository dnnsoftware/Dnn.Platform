﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections;
using System.Reflection;
using System.Web.Routing;
using System.Web.UI;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Client.ClientResourceManagement;

#endregion

namespace DotNetNuke.Modules.MemberDirectory
{
    public partial class View : ProfileModuleUserControlBase
    {
        protected override void OnInit(EventArgs e)
        {
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            JavaScript.RequestRegistration(CommonJs.DnnPlugins);
            JavaScript.RequestRegistration(CommonJs.jQueryFileUpload);
            JavaScript.RequestRegistration(CommonJs.Knockout);

            ClientResourceManager.RegisterScript(Page, "~/DesktopModules/MemberDirectory/Scripts/MemberDirectory.js");
            AddIe7StyleSheet();

            searchBar.Visible = DisplaySearch != "None";
	        advancedSearchBar.Visible = DisplaySearch == "Both";
            popUpPanel.Visible = EnablePopUp;
            loadMore.Visible = !DisablePaging;

            base.OnInit(e);
        }

        protected string AlternateItemTemplate
        {
            get { return GetSetting(ModuleContext.Configuration.TabModuleSettings, "AlternateItemTemplate", Settings.DefaultAlternateItemTemplate); }
        }

        protected bool DisablePaging
        {
            get { return bool.Parse(GetSetting(ModuleContext.Configuration.TabModuleSettings, "DisablePaging", "false")); }
        }

        protected string DisplaySearch
        {
            get { return GetSetting(ModuleContext.Configuration.TabModuleSettings, "DisplaySearch", "Both"); }
        }

        protected bool EnablePopUp
        {
            get { return bool.Parse(GetSetting(ModuleContext.Configuration.TabModuleSettings, "EnablePopUp", "false")); }
        }

        protected string FilterBy
        {
            get { return GetSetting(ModuleContext.Configuration.ModuleSettings, "FilterBy", "None"); }
        }

        protected int GroupId
        {
            get
            {
                int groupId = Null.NullInteger;
                if (!string.IsNullOrEmpty(Request.Params["GroupId"]))
                {
                    groupId = Int32.Parse(Request.Params["GroupId"]);
                }
                return groupId;
            }
        }

        protected string ItemTemplate
        {
            get { return GetSetting(ModuleContext.Configuration.TabModuleSettings, "ItemTemplate", Settings.DefaultItemTemplate); }
        }

        protected int PageSize
        {
            get
            {
                return GetSettingAsInt32(ModuleContext.Configuration.TabModuleSettings, "PageSize", Settings.DefaultPageSize);
            }
        }

        protected string PopUpTemplate
        {
            get { return GetSetting(ModuleContext.Configuration.TabModuleSettings, "PopUpTemplate", Settings.DefaultPopUpTemplate); }
        }

        public override bool DisplayModule
        {
            get
            {
                return !(ProfileUserId == ModuleContext.PortalSettings.UserId && FilterBy == "User") && ModuleContext.PortalSettings.UserId > -1;
            }
        }

        public string ProfileResourceFile
        {
            get { return "~/DesktopModules/Admin/Security/App_LocalResources/Profile.ascx"; }
        }

        protected string ProfileUrlUserToken
        {
            get
            {
                return "PROFILEUSER";
            }
        }

        protected string SearchField1
        {
            get { return GetSetting(ModuleContext.Configuration.TabModuleSettings, "SearchField1", "DisplayName"); }
        }

        protected string SearchField2
        {
            get { return GetSetting(ModuleContext.Configuration.TabModuleSettings, "SearchField2", "Email"); }
        }

        protected string SearchField3
        {
            get { return GetSetting(ModuleContext.Configuration.TabModuleSettings, "SearchField3", "City"); }
        }

        protected string SearchField4
        {
            get { return GetSetting(ModuleContext.Configuration.TabModuleSettings, "SearchField4", "Country"); }
        }

        protected string ViewProfileUrl
        {
            get
            {
                return NavigationManager.NavigateURL(ModuleContext.PortalSettings.UserTabId, "", "userId=PROFILEUSER");
            }
        }

		protected bool DisablePrivateMessage
		{
			get
			{
				return PortalSettings.DisablePrivateMessage && !UserInfo.IsSuperUser
					&& !UserInfo.IsInRole(PortalSettings.AdministratorRoleName);

			}
		}

	    protected PortalSettings PortalSettings
	    {
		    get { return PortalController.Instance.GetCurrentPortalSettings(); }
	    }

		protected UserInfo UserInfo
		{
			get { return UserController.Instance.GetCurrentUserInfo(); }
		}

        #region Private Helper Functions

        private void AddIe7StyleSheet()
        {
            var browser = Request.Browser;
            if (browser.Type == "IE" || browser.MajorVersion < 8)
            {
                const string cssLink = @"<link href=""/DesktopModules/MemberDirectory/ie-member-directory.css"" rel=""stylesheet"" type=""text/css"" />";
                Page.Header.Controls.Add(new LiteralControl(cssLink));
            }
        }

        private string GetSetting(Hashtable settings, string key, string defaultValue)
        {
            string setting = defaultValue;
            if (settings[key] != null)
            {
                setting = Convert.ToString(settings[key]);
            }
            return setting;
        }

        private int GetSettingAsInt32(Hashtable settings, string key, int defaultValue)
        {
            int setting = defaultValue;
            if (settings[key] != null)
            {
                setting = Convert.ToInt32(settings[key]);
            }
            return setting;
        }

        #endregion
    }
}
