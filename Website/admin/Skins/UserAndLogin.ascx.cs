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
using System.Linq;
using System.Web;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Social.Notifications;
using DotNetNuke.Services.Social.Messaging.Internal;

#endregion

namespace DotNetNuke.UI.Skins.Controls
{
    public partial class UserAndLogin : SkinObjectBase
    {
        private const string MyFileName = "UserAndLogin.ascx";

        protected string AvatarImageUrl
        {
            get
            {
                return string.Format(Globals.UserProfilePicFormattedUrl(), PortalSettings.UserId, 32, 32); 
            }
        }

        protected bool CanRegister
        {
            get
            {
                return ((PortalSettings.UserRegistration != (int) Globals.PortalRegistrationType.NoRegistration)
                    && (PortalSettings.Users < PortalSettings.UserQuota || PortalSettings.UserQuota == 0));
            }
        }

        protected string DisplayName
        {
            get
            {
                return PortalSettings.UserInfo.DisplayName;
            }
        }

        protected bool IsAuthenticated
        {
            get
            {
                return Request.IsAuthenticated;
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

                string url = Globals.LoginURL(returnUrl, (Request.QueryString["override"] != null));

                if (UsePopUp)
                {
                    url = "return " + UrlUtils.PopUpUrl(url, this, PortalSettings, true, false, 300, 650);
                }
                return url;
            }
        }

        protected bool UsePopUp
        {
            get
            {
                return PortalSettings.EnablePopUps 
                    && PortalSettings.LoginTabId == Null.NullInteger
                    && !HasSocialAuthenticationEnabled();
            }
        }

        protected string RegisterUrl
        {
            get
            {
                string url = Globals.RegisterURL(HttpUtility.UrlEncode(Globals.NavigateURL()), Null.NullString);

                if (UsePopUp)
                {
                    url = "return " + UrlUtils.PopUpUrl(url, this, PortalSettings, true, false, 600, 950);
                }
                return url;
            }
        }

        protected string UserProfileUrl
        {
            get
            {
                return Globals.UserProfileURL(PortalSettings.UserInfo.UserID); ;
            }
        }

        protected string LocalizeString(string key)
        {
            return Localization.GetString(key, Localization.GetResourceFile(this, MyFileName)); 
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            registerLink.NavigateUrl = RegisterUrl;
            loginLink.NavigateUrl = LoginUrl;
            viewProfileLink.NavigateUrl = Globals.UserProfileURL(PortalSettings.UserId);
            viewProfileImageLink.NavigateUrl = Globals.UserProfileURL(PortalSettings.UserId);
            logoffLink.NavigateUrl = Globals.NavigateURL(PortalSettings.ActiveTab.TabID, "Logoff");
            editProfileLink.NavigateUrl = Globals.NavigateURL(PortalSettings.UserTabId, "Profile", "userId=" + PortalSettings.UserId, "pageno=3");
            accountLink.NavigateUrl = Globals.NavigateURL(PortalSettings.UserTabId, "Profile", "userId=" + PortalSettings.UserId, "pageno=1");
            messagesLink.NavigateUrl = Globals.NavigateURL(GetMessageTab(), "", string.Format("userId={0}", PortalSettings.UserId));
            notificationsLink.NavigateUrl = Globals.NavigateURL(GetMessageTab(), "", string.Format("userId={0}", PortalSettings.UserId), "view=notifications", "action=notifications");

            var unreadMessages = InternalMessagingController.Instance.CountUnreadMessages(PortalSettings.UserId, PortalSettings.PortalId);
            var unreadAlerts = NotificationsController.Instance.CountNotifications(PortalSettings.UserId, PortalSettings.PortalId);

            if(unreadMessages > 0)
            {
                messageCount.Text = unreadMessages.ToString(CultureInfo.InvariantCulture);
                messageCount.Visible = true;

                messages.Text = unreadMessages.ToString(CultureInfo.InvariantCulture);
                messages.ToolTip = unreadMessages == 1 
                                    ? LocalizeString("OneMessage") 
                                    : String.Format(LocalizeString("MessageCount"), unreadMessages);
                messages.Visible = true;
            }

            if (unreadAlerts > 0)
            {
                notificationCount.Text = unreadAlerts.ToString(CultureInfo.InvariantCulture);
                notificationCount.Visible = true;
            }


            if (UsePopUp)
            {
                registerLink.Attributes.Add("onclick", RegisterUrl);
                loginLink.Attributes.Add("onclick", LoginUrl);
            }

            profilePicture.ImageUrl = AvatarImageUrl;
            profilePicture.AlternateText = Localization.GetString("ProfilePicture", Localization.GetResourceFile(this, MyFileName));
        }

        //protected override void OnLoad(EventArgs e)
        //{
        //    base.OnLoad(e);

        //    try
        //    {
        //        if (Request.IsAuthenticated == false)
        //        {
        //        }
        //        else
        //        {
        //            var userInfo = UserController.GetCurrentUserInfo();
        //            if (userInfo.UserID != -1)
        //            {
        //                if (ShowUnreadMessages)
        //                {
        //                    var unreadMessages = InternalMessagingController.Instance.CountUnreadMessages(userInfo.UserID, userInfo.PortalID);
        //                    var unreadAlerts = NotificationsController.Instance.CountNotifications(userInfo.UserID, userInfo.PortalID);

        //                    messageLink.Text = unreadMessages > 0 ? string.Format(Localization.GetString("Messages", Localization.GetResourceFile(this, MyFileName)), unreadMessages) : Localization.GetString("NoMessages", Localization.GetResourceFile(this, MyFileName));
        //                    notificationLink.Text = unreadAlerts > 0 ? string.Format(Localization.GetString("Notifications", Localization.GetResourceFile(this, MyFileName)), unreadAlerts) : Localization.GetString("NoNotifications", Localization.GetResourceFile(this, MyFileName));

        //                    var messageTabUrl = Globals.NavigateURL(GetMessageTab(), "", string.Format("userId={0}", userInfo.UserID));
        //                    messageLink.NavigateUrl = messageTabUrl;
        //                    notificationLink.NavigateUrl = messageTabUrl + "?view=notifications&action=notifications";
        //                    notificationLink.ToolTip = Localization.GetString("CheckNotifications", Localization.GetResourceFile(this, MyFileName));
        //                    messageLink.ToolTip = Localization.GetString("CheckMessages", Localization.GetResourceFile(this, MyFileName));
        //                    messageGroup.Visible = true;
        //                    notificationGroup.Visible = true;

        //                    if (LegacyMode && unreadMessages > 0)
        //                    {
        //                        registerLink.Text = registerLink.Text + string.Format(Localization.GetString("NewMessages", Localization.GetResourceFile(this, MyFileName)), unreadMessages);
        //                    }
        //                }
        //                else
        //                {
        //                    messageGroup.Visible = false;
        //                    notificationGroup.Visible = false;
        //                }

        //                if (ShowAvatar)
        //                {
        //                    avatar.ImageUrl = string.Format(Globals.UserProfilePicFormattedUrl(), userInfo.UserID, 32, 32);
        //                    avatar.NavigateUrl = enhancedRegisterLink.NavigateUrl;
        //                    avatar.ToolTip = Localization.GetString("ProfileAvatar", Localization.GetResourceFile(this, MyFileName));
        //                    avatarGroup.Visible = true;                            
        //                }
        //                else
        //                {
        //                    avatarGroup.Visible = false;
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception exc)
        //    {
        //        Exceptions.ProcessModuleLoadException(this, exc);
        //    }
        //}

        private int GetMessageTab()
        {
            var cacheKey = string.Format("MessageCenterTab:{0}", PortalSettings.PortalId);
            var messageTabId = DataCache.GetCache<int>(cacheKey);
            if (messageTabId > 0)
                return messageTabId;

            //Find the Message Tab
            messageTabId = FindMessageTab();

            //save in cache
            //NOTE - This cache is not being cleared. There is no easy way to clear this, except Tools->Clear Cache
            DataCache.SetCache(cacheKey, messageTabId, TimeSpan.FromMinutes(20));

            return messageTabId;
        }

        private int FindMessageTab()
        {
            var tabController = new TabController();
            var moduleController = new ModuleController();

            //On brand new install the new Message Center Module is on the child page of User Profile Page 
            //On Upgrade to 6.2.0, the Message Center module is on the User Profile Page
            var profileTab = tabController.GetTab(PortalSettings.UserTabId, PortalSettings.PortalId, false);
            if (profileTab != null)
            {
                var childTabs = tabController.GetTabsByPortal(profileTab.PortalID).DescendentsOf(profileTab.TabID);
                foreach (TabInfo tab in childTabs)
                {
                    foreach (KeyValuePair<int, ModuleInfo> kvp in moduleController.GetTabModules(tab.TabID))
                    {
                        var module = kvp.Value;
                        if (module.DesktopModule.FriendlyName == "Message Center" && !module.IsDeleted)
                        {
                            return tab.TabID;                            
                        }
                    }
                }
            }

            //default to User Profile Page
            return PortalSettings.UserTabId;            
        }

        private bool HasSocialAuthenticationEnabled()
        {
            return (from a in AuthenticationController.GetEnabledAuthenticationServices()
                    let enabled = (a.AuthenticationType == "Facebook"
                                     || a.AuthenticationType == "Google"
                                     || a.AuthenticationType == "Live"
                                     || a.AuthenticationType == "Twitter")
                                  ? PortalController.GetPortalSettingAsBoolean(a.AuthenticationType + "_Enabled", PortalSettings.PortalId, false)
                                  : !string.IsNullOrEmpty(a.LoginControlSrc) && (LoadControl("~/" + a.LoginControlSrc) as AuthenticationLoginBase).Enabled
                    where a.AuthenticationType != "DNN" && enabled
                    select a).Any();
        }
    }
}