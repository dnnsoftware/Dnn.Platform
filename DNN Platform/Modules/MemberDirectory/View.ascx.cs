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

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    /// <summary>Display member directory.</summary>
    public partial class View : ProfileModuleUserControlBase
    {
        private readonly IClientResourceController clientResourceController;
        private readonly IApplicationStatusInfo appStatus;
        private readonly IEventLogger eventLogger;
        private readonly IServicesFramework servicesFramework;

        /// <summary>Initializes a new instance of the <see cref="View"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IClientResourceController. Scheduled removal in v12.0.0.")]
        public View()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="View"/> class.</summary>
        /// <param name="clientResourceController">The client resource controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="servicesFramework">The web API service framework.</param>
        public View(IClientResourceController clientResourceController, IApplicationStatusInfo appStatus, IEventLogger eventLogger, IServicesFramework servicesFramework)
        {
            this.clientResourceController = clientResourceController;
            this.appStatus = appStatus;
            this.eventLogger = eventLogger;
            this.servicesFramework = servicesFramework;
        }

        /// <inheritdoc />
        public override bool DisplayModule
            => !(this.ProfileUserId == this.ModuleContext.PortalSettings.UserId && this.FilterBy == "User") && this.ModuleContext.PortalSettings.UserId > -1;

        public string ProfileResourceFile
            => "~/DesktopModules/Admin/Security/App_LocalResources/Profile.ascx";

        protected string AlternateItemTemplate
#pragma warning disable CS0618 // Type or member is obsolete
            => GetSetting(this.ModuleContext.Configuration.TabModuleSettings, "AlternateItemTemplate", Settings.DefaultAlternateItemTemplate);
#pragma warning restore CS0618 // Type or member is obsolete

        protected bool DisablePaging
            => bool.Parse(GetSetting(this.ModuleContext.Configuration.TabModuleSettings, "DisablePaging", "false"));

        protected string DisplaySearch
            => GetSetting(this.ModuleContext.Configuration.TabModuleSettings, "DisplaySearch", "Both");

        protected bool EnablePopUp
            => bool.Parse(GetSetting(this.ModuleContext.Configuration.TabModuleSettings, "EnablePopUp", "false"));

        protected string FilterBy
            => GetSetting(this.ModuleContext.Configuration.ModuleSettings, "FilterBy", "None");

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
#pragma warning disable CS0618 // Type or member is obsolete
            => GetSetting(this.ModuleContext.Configuration.TabModuleSettings, "ItemTemplate", Settings.DefaultItemTemplate);
#pragma warning restore CS0618 // Type or member is obsolete

        protected int PageSize
#pragma warning disable CS0618 // Type or member is obsolete
            => GetSettingAsInt32(this.ModuleContext.Configuration.TabModuleSettings, "PageSize", Settings.DefaultPageSize);
#pragma warning restore CS0618 // Type or member is obsolete

        protected string PopUpTemplate
#pragma warning disable CS0618 // Type or member is obsolete
            => GetSetting(this.ModuleContext.Configuration.TabModuleSettings, "PopUpTemplate", Settings.DefaultPopUpTemplate);
#pragma warning restore CS0618 // Type or member is obsolete

        protected string ProfileUrlUserToken
            => "PROFILEUSER";

        protected string SearchField1
            => GetSetting(this.ModuleContext.Configuration.TabModuleSettings, "SearchField1", "DisplayName");

        protected string SearchField2
            => GetSetting(this.ModuleContext.Configuration.TabModuleSettings, "SearchField2", "Email");

        protected string SearchField3
            => GetSetting(this.ModuleContext.Configuration.TabModuleSettings, "SearchField3", "City");

        protected string SearchField4
            => GetSetting(this.ModuleContext.Configuration.TabModuleSettings, "SearchField4", "Country");

        protected string ViewProfileUrl
            => this.NavigationManager.NavigateURL(this.ModuleContext.PortalSettings.UserTabId, string.Empty, "userId=PROFILEUSER");

        protected bool DisablePrivateMessage
            => this.PortalSettings.DisablePrivateMessage && !this.UserInfo.IsSuperUser && !this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName);

        protected PortalSettings PortalSettings => PortalSettings.Current;

        protected UserInfo UserInfo => UserController.Instance.GetCurrentUserInfo();

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
            this.servicesFramework.RequestAjaxAntiForgerySupport();
            JavaScript.RequestRegistration(this.appStatus, this.eventLogger, this.PortalSettings, CommonJs.DnnPlugins);
            JavaScript.RequestRegistration(this.appStatus, this.eventLogger, this.PortalSettings, CommonJs.jQueryFileUpload);
            JavaScript.RequestRegistration(this.appStatus, this.eventLogger, this.PortalSettings, CommonJs.Knockout);

            this.clientResourceController.RegisterScript("~/DesktopModules/MemberDirectory/Scripts/MemberDirectory.js");
            this.AddIe7StyleSheet();

            this.searchBar.Visible = this.DisplaySearch != "None";
            this.advancedSearchBar.Visible = this.DisplaySearch == "Both";
            this.popUpPanel.Visible = this.EnablePopUp;
            this.loadMore.Visible = !this.DisablePaging;

            base.OnInit(e);
        }

        private static string GetSetting(Hashtable settings, string key, string defaultValue)
        {
            string setting = defaultValue;
            if (settings[key] != null)
            {
                setting = Convert.ToString(settings[key]);
            }

            return setting;
        }

        private static int GetSettingAsInt32(Hashtable settings, string key, int defaultValue)
        {
            int setting = defaultValue;
            if (settings[key] != null)
            {
                setting = Convert.ToInt32(settings[key]);
            }

            return setting;
        }

        private void AddIe7StyleSheet()
        {
            var browser = this.Request.Browser;
            if (browser.Type == "IE" || browser.MajorVersion < 8)
            {
                const string cssLink = """<link href="/DesktopModules/MemberDirectory/ie-member-directory.css" rel="stylesheet" type="text/css" />""";
                this.Page.Header.Controls.Add(new LiteralControl(cssLink));
            }
        }
    }
}
