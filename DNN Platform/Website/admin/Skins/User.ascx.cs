// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Social.Messaging.Internal;
    using DotNetNuke.Services.Social.Notifications;
    using Microsoft.Extensions.DependencyInjection;

    /// -----------------------------------------------------------------------------
    /// <summary></summary>
    /// <remarks></remarks>
    /// -----------------------------------------------------------------------------
    public partial class User : SkinObjectBase
    {
        private const string MyFileName = "User.ascx";
        private readonly INavigationManager _navigationManager;

        public User()
        {
            this._navigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
            this.ShowUnreadMessages = true;
            this.ShowAvatar = true;
            this.LegacyMode = true;
        }

        public string CssClass { get; set; }

        public bool ShowUnreadMessages { get; set; }

        public bool ShowAvatar { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether set this to false in the skin to take advantage of the enhanced markup.
        /// </summary>
        public bool LegacyMode { get; set; }

        public string Text { get; set; }

        public string URL { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether set this to true to show in custom 404/500 page.
        /// </summary>
        public bool ShowInErrorPage { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.Visible = !this.PortalSettings.InErrorPageRequest() || this.ShowInErrorPage;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                if (this.LegacyMode)
                {
                    this.registerGroup.Visible = false;
                }
                else
                {
                    this.registerLink.Visible = false;
                }

                if (!string.IsNullOrEmpty(this.CssClass))
                {
                    this.registerLink.CssClass = this.CssClass;
                    this.enhancedRegisterLink.CssClass = this.CssClass;
                }

                if (this.Request.IsAuthenticated == false)
                {
                    this.messageGroup.Visible = false;
                    this.notificationGroup.Visible = false;
                    this.avatarGroup.Visible = false;

                    if (this.PortalSettings.UserRegistration != (int)Globals.PortalRegistrationType.NoRegistration)
                    {
                        if (!string.IsNullOrEmpty(this.Text))
                        {
                            if (this.Text.IndexOf("src=") != -1)
                            {
                                this.Text = this.Text.Replace("src=\"", "src=\"" + this.PortalSettings.ActiveTab.SkinPath);
                            }

                            this.registerLink.Text = this.Text;
                            this.enhancedRegisterLink.Text = this.Text;
                        }
                        else
                        {
                            this.registerLink.Text = Localization.GetString("Register", Localization.GetResourceFile(this, MyFileName));
                            this.enhancedRegisterLink.Text = this.registerLink.Text;
                            this.registerLink.ToolTip = this.registerLink.Text;
                            this.enhancedRegisterLink.ToolTip = this.registerLink.Text;
                        }

                        if (this.PortalSettings.Users < this.PortalSettings.UserQuota || this.PortalSettings.UserQuota == 0)
                        {
                            if (this.LegacyMode)
                            {
                                this.registerLink.Visible = true;
                            }
                            else
                            {
                                this.enhancedRegisterLink.Visible = true;
                            }
                        }
                        else
                        {
                            this.registerGroup.Visible = false;
                            this.registerLink.Visible = false;
                        }

                        this.registerLink.NavigateUrl = !string.IsNullOrEmpty(this.URL)
                                            ? this.URL
                                            : Globals.RegisterURL(HttpUtility.UrlEncode(this._navigationManager.NavigateURL()), Null.NullString);
                        this.enhancedRegisterLink.NavigateUrl = this.registerLink.NavigateUrl;

                        if (this.PortalSettings.EnablePopUps && this.PortalSettings.RegisterTabId == Null.NullInteger
                            && !AuthenticationController.HasSocialAuthenticationEnabled(this))
                        {
                            var clickEvent = "return " + UrlUtils.PopUpUrl(this.registerLink.NavigateUrl, this, this.PortalSettings, true, false, 600, 950);
                            this.registerLink.Attributes.Add("onclick", clickEvent);
                            this.enhancedRegisterLink.Attributes.Add("onclick", clickEvent);
                        }
                    }
                    else
                    {
                        this.registerGroup.Visible = false;
                        this.registerLink.Visible = false;
                    }
                }
                else
                {
                    var userInfo = UserController.Instance.GetCurrentUserInfo();
                    if (userInfo.UserID != -1)
                    {
                        this.registerLink.Text = userInfo.DisplayName;
                        this.registerLink.NavigateUrl = Globals.UserProfileURL(userInfo.UserID);
                        this.registerLink.ToolTip = Localization.GetString("VisitMyProfile", Localization.GetResourceFile(this, MyFileName));

                        this.enhancedRegisterLink.Text = this.registerLink.Text;
                        this.enhancedRegisterLink.NavigateUrl = this.registerLink.NavigateUrl;
                        this.enhancedRegisterLink.ToolTip = this.registerLink.ToolTip;

                        if (this.ShowUnreadMessages)
                        {
                            var unreadMessages = InternalMessagingController.Instance.CountUnreadMessages(userInfo.UserID, PortalController.GetEffectivePortalId(userInfo.PortalID));
                            var unreadAlerts = NotificationsController.Instance.CountNotifications(userInfo.UserID, PortalController.GetEffectivePortalId(userInfo.PortalID));

                            this.messageLink.Text = unreadMessages > 0 ? string.Format(Localization.GetString("Messages", Localization.GetResourceFile(this, MyFileName)), unreadMessages) : Localization.GetString("NoMessages", Localization.GetResourceFile(this, MyFileName));
                            this.notificationLink.Text = unreadAlerts > 0 ? string.Format(Localization.GetString("Notifications", Localization.GetResourceFile(this, MyFileName)), unreadAlerts) : Localization.GetString("NoNotifications", Localization.GetResourceFile(this, MyFileName));

                            this.messageLink.NavigateUrl = this._navigationManager.NavigateURL(this.GetMessageTab(), string.Empty, string.Format("userId={0}", userInfo.UserID));
                            this.notificationLink.NavigateUrl = this._navigationManager.NavigateURL(this.GetMessageTab(), string.Empty, string.Format("userId={0}", userInfo.UserID), "view=notifications", "action=notifications");
                            this.notificationLink.ToolTip = Localization.GetString("CheckNotifications", Localization.GetResourceFile(this, MyFileName));
                            this.messageLink.ToolTip = Localization.GetString("CheckMessages", Localization.GetResourceFile(this, MyFileName));
                            this.messageGroup.Visible = true;
                            this.notificationGroup.Visible = true;

                            if (this.LegacyMode && unreadMessages > 0)
                            {
                                this.registerLink.Text = this.registerLink.Text + string.Format(Localization.GetString("NewMessages", Localization.GetResourceFile(this, MyFileName)), unreadMessages);
                            }
                        }
                        else
                        {
                            this.messageGroup.Visible = false;
                            this.notificationGroup.Visible = false;
                        }

                        if (this.ShowAvatar)
                        {
                            this.avatar.ImageUrl = this.GetAvatarUrl(userInfo);
                            this.avatar.NavigateUrl = this.enhancedRegisterLink.NavigateUrl;
                            this.avatar.ToolTip = this.avatar.Text = Localization.GetString("ProfileAvatar", Localization.GetResourceFile(this, MyFileName));
                            this.avatarGroup.Visible = true;
                        }
                        else
                        {
                            this.avatarGroup.Visible = false;
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private string GetAvatarUrl(UserInfo userInfo)
        {
            return UserController.Instance.GetUserProfilePictureUrl(userInfo.UserID, 32, 32);
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
    }
}
