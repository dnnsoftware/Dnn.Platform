// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.MemberDirectory
{
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

    public partial class View : ProfileModuleUserControlBase
    {
        public override bool DisplayModule
        {
            get
            {
                return !(this.ProfileUserId == this.ModuleContext.PortalSettings.UserId && this.FilterBy == "User") && this.ModuleContext.PortalSettings.UserId > -1;
            }
        }

        public string ProfileResourceFile
        {
            get { return "~/DesktopModules/Admin/Security/App_LocalResources/Profile.ascx"; }
        }

        protected string AlternateItemTemplate
        {
            get { return this.GetSetting(this.ModuleContext.Configuration.TabModuleSettings, "AlternateItemTemplate", Settings.DefaultAlternateItemTemplate); }
        }

        protected bool DisablePaging
        {
            get { return bool.Parse(this.GetSetting(this.ModuleContext.Configuration.TabModuleSettings, "DisablePaging", "false")); }
        }

        protected string DisplaySearch
        {
            get { return this.GetSetting(this.ModuleContext.Configuration.TabModuleSettings, "DisplaySearch", "Both"); }
        }

        protected bool EnablePopUp
        {
            get { return bool.Parse(this.GetSetting(this.ModuleContext.Configuration.TabModuleSettings, "EnablePopUp", "false")); }
        }

        protected string FilterBy
        {
            get { return this.GetSetting(this.ModuleContext.Configuration.ModuleSettings, "FilterBy", "None"); }
        }

        protected int GroupId
        {
            get
            {
                int groupId = Null.NullInteger;
                if (!string.IsNullOrEmpty(this.Request.Params["GroupId"]))
                {
                    groupId = int.Parse(this.Request.Params["GroupId"]);
                }

                return groupId;
            }
        }

        protected string ItemTemplate
        {
            get { return this.GetSetting(this.ModuleContext.Configuration.TabModuleSettings, "ItemTemplate", Settings.DefaultItemTemplate); }
        }

        protected int PageSize
        {
            get
            {
                return this.GetSettingAsInt32(this.ModuleContext.Configuration.TabModuleSettings, "PageSize", Settings.DefaultPageSize);
            }
        }

        protected string PopUpTemplate
        {
            get { return this.GetSetting(this.ModuleContext.Configuration.TabModuleSettings, "PopUpTemplate", Settings.DefaultPopUpTemplate); }
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
            get { return this.GetSetting(this.ModuleContext.Configuration.TabModuleSettings, "SearchField1", "DisplayName"); }
        }

        protected string SearchField2
        {
            get { return this.GetSetting(this.ModuleContext.Configuration.TabModuleSettings, "SearchField2", "Email"); }
        }

        protected string SearchField3
        {
            get { return this.GetSetting(this.ModuleContext.Configuration.TabModuleSettings, "SearchField3", "City"); }
        }

        protected string SearchField4
        {
            get { return this.GetSetting(this.ModuleContext.Configuration.TabModuleSettings, "SearchField4", "Country"); }
        }

        protected string ViewProfileUrl
        {
            get
            {
                return this.NavigationManager.NavigateURL(this.ModuleContext.PortalSettings.UserTabId, string.Empty, "userId=PROFILEUSER");
            }
        }

        protected bool DisablePrivateMessage
        {
            get
            {
                return this.PortalSettings.DisablePrivateMessage && !this.UserInfo.IsSuperUser
                    && !this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName);
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

        protected override void OnInit(EventArgs e)
        {
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            JavaScript.RequestRegistration(CommonJs.DnnPlugins);
            JavaScript.RequestRegistration(CommonJs.jQueryFileUpload);
            JavaScript.RequestRegistration(CommonJs.Knockout);

            ClientResourceManager.RegisterScript(this.Page, "~/DesktopModules/MemberDirectory/Scripts/MemberDirectory.js");
            this.AddIe7StyleSheet();

            this.searchBar.Visible = this.DisplaySearch != "None";
            this.advancedSearchBar.Visible = this.DisplaySearch == "Both";
            this.popUpPanel.Visible = this.EnablePopUp;
            this.loadMore.Visible = !this.DisablePaging;

            base.OnInit(e);
        }

        private void AddIe7StyleSheet()
        {
            var browser = this.Request.Browser;
            if (browser.Type == "IE" || browser.MajorVersion < 8)
            {
                const string cssLink = @"<link href=""/DesktopModules/MemberDirectory/ie-member-directory.css"" rel=""stylesheet"" type=""text/css"" />";
                this.Page.Header.Controls.Add(new LiteralControl(cssLink));
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
    }
}
