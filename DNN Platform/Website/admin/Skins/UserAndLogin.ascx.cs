// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Web;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Social.Messaging.Internal;
    using DotNetNuke.Services.Social.Notifications;
    using Microsoft.Extensions.DependencyInjection;

    public partial class UserAndLogin : SkinObjectBase
    {
        private const string MyFileName = "UserAndLogin.ascx";
        private readonly INavigationManager _navigationManager;

        public UserAndLogin()
        {
            this._navigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        /// <summary>
        /// Gets or sets a value indicating whether set this to true to show in custom 404/500 page.
        /// </summary>
        public bool ShowInErrorPage { get; set; }

        protected string AvatarImageUrl => UserController.Instance.GetUserProfilePictureUrl(this.PortalSettings.UserId, 32, 32);

        protected bool CanRegister
        {
            get
            {
                return (this.PortalSettings.UserRegistration != (int)Globals.PortalRegistrationType.NoRegistration)
                    && (this.PortalSettings.Users < this.PortalSettings.UserQuota || this.PortalSettings.UserQuota == 0);
            }
        }

        protected string DisplayName
        {
            get
            {
                return this.PortalSettings.UserInfo.DisplayName;
            }
        }

        protected bool IsAuthenticated
        {
            get
            {
                return this.Request.IsAuthenticated;
            }
        }

        protected string LoginUrl
        {
            get
            {
                string returnUrl = HttpContext.Current.Request.RawUrl;
                if (returnUrl.IndexOf("?returnurl=", StringComparison.Ordinal) != -1)
                {
                    returnUrl = returnUrl.Substring(0, returnUrl.IndexOf("?returnurl=", StringComparison.Ordinal));
                }

                returnUrl = HttpUtility.UrlEncode(returnUrl);

                return Globals.LoginURL(returnUrl, this.Request.QueryString["override"] != null);
            }
        }

        protected string LoginUrlForClickEvent
        {
            get
            {
                var url = this.LoginUrl;

                if (this.UsePopUp)
                {
                    return "return " + UrlUtils.PopUpUrl(HttpUtility.UrlDecode(this.LoginUrl), this, this.PortalSettings, true, false, 300, 650);
                }

                return string.Empty;
            }
        }

        protected bool UsePopUp
        {
            get
            {
                return this.PortalSettings.EnablePopUps
                    && this.PortalSettings.LoginTabId == Null.NullInteger
                    && !AuthenticationController.HasSocialAuthenticationEnabled(this);
            }
        }

        protected string RegisterUrl
        {
            get
            {
                return Globals.RegisterURL(HttpUtility.UrlEncode(this._navigationManager.NavigateURL()), Null.NullString);
            }
        }

        protected string RegisterUrlForClickEvent
        {
            get
            {
                if (this.UsePopUp)
                {
                    return "return " + UrlUtils.PopUpUrl(HttpUtility.UrlDecode(this.RegisterUrl), this, this.PortalSettings, true, false, 600, 950);
                }

                return string.Empty;
            }
        }

        protected string UserProfileUrl
        {
            get
            {
                return Globals.UserProfileURL(this.PortalSettings.UserInfo.UserID);
            }
        }

        protected string LocalizeString(string key)
        {
            return Localization.GetString(key, Localization.GetResourceFile(this, MyFileName));
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.Visible = !this.PortalSettings.InErrorPageRequest() || this.ShowInErrorPage;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.registerLink.NavigateUrl = this.RegisterUrl;
            this.loginLink.NavigateUrl = this.LoginUrl;

            if (this.PortalSettings.UserId > 0)
            {
                this.viewProfileLink.NavigateUrl = Globals.UserProfileURL(this.PortalSettings.UserId);
                this.viewProfileImageLink.NavigateUrl = Globals.UserProfileURL(this.PortalSettings.UserId);
                this.logoffLink.NavigateUrl = this._navigationManager.NavigateURL(this.PortalSettings.ActiveTab.TabID, "Logoff");
                this.editProfileLink.NavigateUrl = this._navigationManager.NavigateURL(this.PortalSettings.UserTabId, "Profile", "userId=" + this.PortalSettings.UserId, "pageno=2");
                this.accountLink.NavigateUrl = this._navigationManager.NavigateURL(this.PortalSettings.UserTabId, "Profile", "userId=" + this.PortalSettings.UserId, "pageno=1");
                this.messagesLink.NavigateUrl = this._navigationManager.NavigateURL(this.GetMessageTab(), string.Empty, string.Format("userId={0}", this.PortalSettings.UserId));
                this.notificationsLink.NavigateUrl = this._navigationManager.NavigateURL(this.GetMessageTab(), string.Empty, string.Format("userId={0}", this.PortalSettings.UserId), "view=notifications", "action=notifications");

                var unreadMessages = InternalMessagingController.Instance.CountUnreadMessages(this.PortalSettings.UserId, this.PortalSettings.PortalId);
                var unreadAlerts = NotificationsController.Instance.CountNotifications(this.PortalSettings.UserId, this.PortalSettings.PortalId);

                if (unreadMessages > 0)
                {
                    this.messageCount.Text = unreadMessages.ToString(CultureInfo.InvariantCulture);
                    this.messageCount.Visible = true;

                    this.messages.Text = unreadMessages.ToString(CultureInfo.InvariantCulture);
                    this.messages.ToolTip = unreadMessages == 1
                                        ? this.LocalizeString("OneMessage")
                                        : string.Format(this.LocalizeString("MessageCount"), unreadMessages);
                    this.messages.Visible = true;
                }

                if (unreadAlerts > 0)
                {
                    this.notificationCount.Text = unreadAlerts.ToString(CultureInfo.InvariantCulture);
                    this.notificationCount.Visible = true;
                }

                this.profilePicture.ImageUrl = this.AvatarImageUrl;
                this.profilePicture.AlternateText = Localization.GetString("ProfilePicture", Localization.GetResourceFile(this, MyFileName));

                if (this.AlwaysShowCount())
                {
                    this.messageCount.Visible = this.notificationCount.Visible = true;
                }
            }

            if (this.UsePopUp)
            {
                this.registerLink.Attributes.Add("onclick", this.RegisterUrlForClickEvent);
                this.loginLink.Attributes.Add("onclick", this.LoginUrlForClickEvent);
            }
        }

        private int GetMessageTab()
        {
            var cacheKey = string.Format("MessageCenterTab:{0}:{1}", this.PortalSettings.PortalId, this.PortalSettings.CultureCode);
            var messageTabId = DataCache.GetCache<int>(cacheKey);
            if (messageTabId > 0)
            {
                return messageTabId;
            }

            // Find the Message Tab
            messageTabId = this.FindMessageTab();

            // save in cache
            // NOTE - This cache is not being cleared. There is no easy way to clear this, except Tools->Clear Cache
            DataCache.SetCache(cacheKey, messageTabId, TimeSpan.FromMinutes(20));

            return messageTabId;
        }

        private int FindMessageTab()
        {
            // On brand new install the new Message Center Module is on the child page of User Profile Page
            // On Upgrade to 6.2.0, the Message Center module is on the User Profile Page
            var profileTab = TabController.Instance.GetTab(this.PortalSettings.UserTabId, this.PortalSettings.PortalId, false);
            if (profileTab != null)
            {
                var childTabs = TabController.Instance.GetTabsByPortal(profileTab.PortalID).DescendentsOf(profileTab.TabID);
                foreach (TabInfo tab in childTabs)
                {
                    foreach (KeyValuePair<int, ModuleInfo> kvp in ModuleController.Instance.GetTabModules(tab.TabID))
                    {
                        var module = kvp.Value;
                        if (module.DesktopModule.FriendlyName == "Message Center" && !module.IsDeleted)
                        {
                            return tab.TabID;
                        }
                    }
                }
            }

            // default to User Profile Page
            return this.PortalSettings.UserTabId;
        }

        private bool AlwaysShowCount()
        {
            const string SettingKey = "UserAndLogin_AlwaysShowCount";
            var alwaysShowCount = false;

            var portalSetting = PortalController.GetPortalSetting(SettingKey, this.PortalSettings.PortalId, string.Empty);
            if (!string.IsNullOrEmpty(portalSetting) && bool.TryParse(portalSetting, out alwaysShowCount))
            {
                return alwaysShowCount;
            }

            var hostSetting = HostController.Instance.GetString(SettingKey, string.Empty);
            if (!string.IsNullOrEmpty(hostSetting) && bool.TryParse(hostSetting, out alwaysShowCount))
            {
                return alwaysShowCount;
            }

            return false;
        }
    }
}
