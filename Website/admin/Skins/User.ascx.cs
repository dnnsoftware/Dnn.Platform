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
    /// -----------------------------------------------------------------------------
    /// <summary></summary>
    /// <remarks></remarks>
    /// <history>
    /// 	[cniknet]	10/15/2004	Replaced public members with properties and removed
    ///                             brackets from property names
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class User : SkinObjectBase
    {
        private const string MyFileName = "User.ascx";

        public User()
        {
            ShowUnreadMessages = true;
            ShowAvatar = true;
            LegacyMode = true;
        }

        public string CssClass { get; set; }

        public bool ShowUnreadMessages { get; set; }

        public bool ShowAvatar { get; set; }

        /// <summary>
        /// Set this to false in the skin to take advantage of the enhanced markup
        /// </summary>
        public bool LegacyMode { get; set; }

        public string Text { get; set; }

        public string URL { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                if (LegacyMode)
                    registerGroup.Visible = false;
                else
                    registerLink.Visible = false;

                if (!String.IsNullOrEmpty(CssClass))
                {
                    registerLink.CssClass = CssClass;
                    enhancedRegisterLink.CssClass = CssClass;
                }

                if (Request.IsAuthenticated == false)
                {
                    messageGroup.Visible = false;
                    notificationGroup.Visible = false;
                    avatarGroup.Visible = false;

                    if (PortalSettings.UserRegistration != (int) Globals.PortalRegistrationType.NoRegistration)
                    {
                        if (!String.IsNullOrEmpty(Text))
                        {
                            if (Text.IndexOf("src=") != -1)
                            {
                                Text = Text.Replace("src=\"", "src=\"" + PortalSettings.ActiveTab.SkinPath);
                            }
                            registerLink.Text = Text;
                            enhancedRegisterLink.Text = Text;
                        }
                        else
                        {
                            registerLink.Text = Localization.GetString("Register", Localization.GetResourceFile(this, MyFileName));
                            enhancedRegisterLink.Text = registerLink.Text;
                            registerLink.ToolTip = registerLink.Text;
                            enhancedRegisterLink.ToolTip = registerLink.Text;
                        }
                        if (PortalSettings.Users < PortalSettings.UserQuota || PortalSettings.UserQuota == 0)
                        {                            
                            if (LegacyMode) registerLink.Visible = true;
                            else  enhancedRegisterLink.Visible = true;
                        }
                        else
                        {
                            registerGroup.Visible = false;
                            registerLink.Visible = false;
                        }

                        registerLink.NavigateUrl = !String.IsNullOrEmpty(URL) 
                                            ? URL 
                                            : Globals.RegisterURL(HttpUtility.UrlEncode(Globals.NavigateURL()), Null.NullString);
                        enhancedRegisterLink.NavigateUrl = registerLink.NavigateUrl;

                        if (PortalSettings.EnablePopUps && PortalSettings.RegisterTabId == Null.NullInteger
                            && !HasSocialAuthenticationEnabled())
                        {
                            var clickEvent = "return " + UrlUtils.PopUpUrl(registerLink.NavigateUrl, this, PortalSettings, true, false, 600, 950);
                            registerLink.Attributes.Add("onclick", clickEvent);
                            enhancedRegisterLink.Attributes.Add("onclick", clickEvent);
                        }

                    }
                    else
                    {
                        registerGroup.Visible = false;
                        registerLink.Visible = false;
                    }
                }
                else
                {
                    var userInfo = UserController.GetCurrentUserInfo();
                    if (userInfo.UserID != -1)
                    {
                        registerLink.Text = userInfo.DisplayName;                                                
                        registerLink.NavigateUrl = Globals.UserProfileURL(userInfo.UserID);                        
                        registerLink.ToolTip = Localization.GetString("VisitMyProfile", Localization.GetResourceFile(this, MyFileName));

                        enhancedRegisterLink.Text = registerLink.Text;
                        enhancedRegisterLink.NavigateUrl = registerLink.NavigateUrl;
                        enhancedRegisterLink.ToolTip = registerLink.ToolTip;

                        if (ShowUnreadMessages)
                        {
                            var unreadMessages = InternalMessagingController.Instance.CountUnreadMessages(userInfo.UserID, PortalController.GetEffectivePortalId(userInfo.PortalID));
                            var unreadAlerts = NotificationsController.Instance.CountNotifications(userInfo.UserID, PortalController.GetEffectivePortalId(userInfo.PortalID));

                            messageLink.Text = unreadMessages > 0 ? string.Format(Localization.GetString("Messages", Localization.GetResourceFile(this, MyFileName)), unreadMessages) : Localization.GetString("NoMessages", Localization.GetResourceFile(this, MyFileName));
                            notificationLink.Text = unreadAlerts > 0 ? string.Format(Localization.GetString("Notifications", Localization.GetResourceFile(this, MyFileName)), unreadAlerts) : Localization.GetString("NoNotifications", Localization.GetResourceFile(this, MyFileName));

                            messageLink.NavigateUrl = Globals.NavigateURL(GetMessageTab(), "", string.Format("userId={0}", userInfo.UserID));
                            notificationLink.NavigateUrl = Globals.NavigateURL(GetMessageTab(), "", string.Format("userId={0}", userInfo.UserID),"view=notifications","action=notifications");
                            notificationLink.ToolTip = Localization.GetString("CheckNotifications", Localization.GetResourceFile(this, MyFileName));
                            messageLink.ToolTip = Localization.GetString("CheckMessages", Localization.GetResourceFile(this, MyFileName));
                            messageGroup.Visible = true;
                            notificationGroup.Visible = true;

                            if (LegacyMode && unreadMessages > 0)
                            {
                                registerLink.Text = registerLink.Text + string.Format(Localization.GetString("NewMessages", Localization.GetResourceFile(this, MyFileName)), unreadMessages);
                            }
                        }
                        else
                        {
                            messageGroup.Visible = false;
                            notificationGroup.Visible = false;
                        }

                        if (ShowAvatar)
                        {
                            avatar.ImageUrl = string.Format(Globals.UserProfilePicFormattedUrl(), userInfo.UserID, 32, 32);
                            avatar.NavigateUrl = enhancedRegisterLink.NavigateUrl;
                            avatar.ToolTip = Localization.GetString("ProfileAvatar", Localization.GetResourceFile(this, MyFileName));
                            avatarGroup.Visible = true;                            
                        }
                        else
                        {
                            avatarGroup.Visible = false;
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

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