// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Skins
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Web;
    using System.Web.Mvc;

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
    using DotNetNuke.Web.MvcPipeline.Models;

    public static partial class SkinHelpers
    {
        private static string userResourceFile = GetSkinsResourceFile("User.ascx");

        public static IHtmlString User(this HtmlHelper<PageModel> helper, string cssClass = "SkinObject", string text = "", string url = "", bool showUnreadMessages = true, bool showAvatar = true, bool legacyMode = true, bool showInErrorPage = false)
        {
            //TODO: CSP - enable when CSP implementation is ready
            var nonce = string.Empty; // helper.ViewData.Model.ContentSecurityPolicy.Nonce;
            var portalSettings = PortalSettings.Current;
            var navigationManager = helper.ViewData.Model.NavigationManager;

            if (portalSettings.InErrorPageRequest() && !showInErrorPage)
            {
                return MvcHtmlString.Empty;
            }

            var registerText = Localization.GetString("Register", userResourceFile);
            if (!string.IsNullOrEmpty(text))
            {
                registerText = text;
                if (text.IndexOf("src=") != -1)
                {
                    registerText = text.Replace("src=\"", "src=\"" + portalSettings.ActiveTab.SkinPath);
                }
            }

            if (legacyMode)
            {
                if (portalSettings.UserRegistration == (int)Globals.PortalRegistrationType.NoRegistration ||
                    (portalSettings.Users > portalSettings.UserQuota && portalSettings.UserQuota != 0))
                {
                    return MvcHtmlString.Empty;
                }

                var registerLink = new TagBuilder("a");
                registerLink.AddCssClass("dnnRegisterLink");
                if (!string.IsNullOrEmpty(cssClass))
                {
                    registerLink.AddCssClass(cssClass);
                }

                registerLink.Attributes.Add("rel", "nofollow");
                registerLink.InnerHtml = registerText;
                registerLink.Attributes.Add("href", !string.IsNullOrEmpty(url) ? url : Globals.RegisterURL(HttpUtility.UrlEncode(navigationManager.NavigateURL()), Null.NullString));

                string registerScript = string.Empty;
                if (portalSettings.EnablePopUps && portalSettings.RegisterTabId == Null.NullInteger && !AuthenticationController.HasSocialAuthenticationEnabled(null))
                {
                    // var clickEvent = "return " + UrlUtils.PopUpUrl(registerLink.Attributes["href"], portalSettings, true, false, 600, 950);
                    // registerLink.Attributes.Add("onclick", clickEvent);
                    registerScript = GetRegisterScript(registerLink.Attributes["href"], nonce);
                }

                return new MvcHtmlString(registerLink.ToString() + registerScript);
            }
            else
            {
                string registerScript = string.Empty;
                var userWrapperDiv = new TagBuilder("div");
                userWrapperDiv.AddCssClass("registerGroup");
                var ul = new TagBuilder("ul");
                ul.AddCssClass("buttonGroup");
                if (!HttpContext.Current.Request.IsAuthenticated)
                {
                    // Unauthenticated User Logic
                    if (portalSettings.UserRegistration != (int)Globals.PortalRegistrationType.NoRegistration &&
                        (portalSettings.Users < portalSettings.UserQuota || portalSettings.UserQuota == 0))
                    {
                        // User Register
                        var registerLi = new TagBuilder("li");
                        registerLi.AddCssClass("userRegister");

                        var registerLink = new TagBuilder("a");
                        registerLink.AddCssClass("dnnRegisterLink");
                        registerLink.AddCssClass(cssClass);
                        registerLink.Attributes.Add("rel", "nofollow");
                        registerLink.InnerHtml = !string.IsNullOrEmpty(text) ? text.Replace("src=\"", "src=\"" + portalSettings.ActiveTab.SkinPath) : Localization.GetString("Register", userResourceFile);
                        registerLink.Attributes.Add("href", !string.IsNullOrEmpty(url) ? url : Globals.RegisterURL(HttpUtility.UrlEncode(navigationManager.NavigateURL()), Null.NullString));

                        if (portalSettings.EnablePopUps && portalSettings.RegisterTabId == Null.NullInteger/*&& !AuthenticationController.HasSocialAuthenticationEnabled(portalSettings)*/)
                        {
                            // var clickEvent = "return " + UrlUtils.PopUpUrl(registerLink.Attributes["href"], portalSettings, true, false, 600, 950);
                            // registerLink.Attributes.Add("onclick", clickEvent);
                            registerScript = GetRegisterScript(registerLink.Attributes["href"], nonce);
                        }

                        registerLi.InnerHtml = registerLink.ToString();
                        ul.InnerHtml += registerLi.ToString();
                    }
                }
                else
                {
                    var userInfo = UserController.Instance.GetCurrentUserInfo();
                    if (userInfo.UserID != -1)
                    {
                        // Add menu-items (viewProfile, userMessages, userNotifications, etc.)
                        if (showUnreadMessages)
                        {
                            // Create Messages
                            var unreadMessages = InternalMessagingController.Instance.CountUnreadMessages(userInfo.UserID, PortalController.GetEffectivePortalId(userInfo.PortalID));

                            var messageLinkText = unreadMessages > 0 ? string.Format(Localization.GetString("Messages", userResourceFile), unreadMessages) : string.Format(Localization.GetString("NoMessages", userResourceFile));
                            ul.InnerHtml += CreateMenuItem(messageLinkText, "userMessages", navigationManager.NavigateURL(GetMessageTab(portalSettings)));

                            // Create Notifications
                            var unreadAlerts = NotificationsController.Instance.CountNotifications(userInfo.UserID, PortalController.GetEffectivePortalId(userInfo.PortalID));
                            var alertLink = navigationManager.NavigateURL(GetMessageTab(portalSettings), string.Empty, string.Format("userId={0}", userInfo.UserID), "view=notifications", "action=notifications");
                            var alertLinkText = unreadAlerts > 0 ? string.Format(Localization.GetString("Notifications", userResourceFile), unreadAlerts) : string.Format(Localization.GetString("NoNotifications", userResourceFile));

                            ul.InnerHtml += CreateMenuItem(alertLinkText, "userNotifications", alertLink);
                        }

                        // Create User Display Name Link
                        var userDisplayText = userInfo.DisplayName;
                        var userDisplayTextUrl = Globals.UserProfileURL(userInfo.UserID);
                        var userDisplayTextToolTip = Localization.GetString("VisitMyProfile", userResourceFile);

                        ul.InnerHtml += CreateMenuItem(userDisplayText, "userDisplayName", userDisplayTextUrl);

                        if (showAvatar)
                        {
                            var userProfileLi = new TagBuilder("li");
                            userProfileLi.AddCssClass("userProfile");

                            // Get the Profile Image
                            var profileImg = new TagBuilder("img");
                            profileImg.Attributes.Add("src", UserController.Instance.GetUserProfilePictureUrl(userInfo.UserID, 32, 32));
                            profileImg.Attributes.Add("alt", Localization.GetString("ProfilePicture", userResourceFile));

                            ul.InnerHtml += CreateMenuItem(profileImg.ToString(), "userProfileImg", userDisplayTextUrl);
                        }
                    }
                }

                userWrapperDiv.InnerHtml = ul.ToString();
                return new MvcHtmlString(userWrapperDiv.ToString() + registerScript);
            }
        }

        private static string CreateMenuItem(string cssClass, string href, string resourceKey, bool isStrong = false)
        {
            var li = new TagBuilder("li");
            li.AddCssClass(cssClass);

            var a = new TagBuilder("a");
            a.Attributes.Add("href", href);
            var text = Localization.GetString(resourceKey, userResourceFile);
            a.InnerHtml = isStrong ? $"<strong>{text}</strong>" : text;

            li.InnerHtml = a.ToString();
            return li.ToString();
        }

        private static string CreateMenuItem(string text, string cssClass, string href)
        {
            var li = new TagBuilder("li");
            li.AddCssClass(cssClass);

            var a = new TagBuilder("a");
            a.Attributes.Add("href", href);

            a.InnerHtml += text;

            li.InnerHtml = a.ToString();
            return li.ToString();
        }

        private static string CreateMessageMenuItem(string cssClass, string href, string resourceKey, int count)
        {
            var li = new TagBuilder("li");
            li.AddCssClass(cssClass);

            var a = new TagBuilder("a");
            a.Attributes.Add("href", href);

            if (count > 0 || AlwaysShowCount(PortalSettings.Current))
            {
                var span = new TagBuilder("span");
                span.AddCssClass(cssClass == "userMessages" ? "messageCount" : "notificationCount");
                span.InnerHtml = count.ToString();
                a.InnerHtml = span.ToString();
            }

            var innerText = Localization.GetString(resourceKey, userResourceFile);
            innerText = string.Format(innerText, count.ToString());
            a.InnerHtml += innerText;

            li.InnerHtml = a.ToString();
            return li.ToString();
        }

        private static int GetMessageTab(PortalSettings portalSettings)
        {
            var cacheKey = string.Format("MessageCenterTab:{0}:{1}", portalSettings.PortalId, portalSettings.CultureCode);
            var messageTabId = DataCache.GetCache<int>(cacheKey);
            if (messageTabId > 0)
            {
                return messageTabId;
            }

            // Find the Message Tab
            messageTabId = FindMessageTab(portalSettings);

            // save in cache
            DataCache.SetCache(cacheKey, messageTabId, TimeSpan.FromMinutes(20));

            return messageTabId;
        }

        private static int FindMessageTab(PortalSettings portalSettings)
        {
            var profileTab = TabController.Instance.GetTab(portalSettings.UserTabId, portalSettings.PortalId, false);
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
            return portalSettings.UserTabId;
        }

        private static bool AlwaysShowCount(PortalSettings portalSettings)
        {
            const string SettingKey = "UserAndLogin_AlwaysShowCount";
            var alwaysShowCount = false;

            var portalSetting = PortalController.GetPortalSetting(SettingKey, portalSettings.PortalId, string.Empty);
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

        private static string GetRegisterScript(string loginUrl, string nonce)
        {
            var portalSettings = PortalSettings.Current;
            var request = HttpContext.Current.Request;

            if (!request.IsAuthenticated)
            {
                var nonceAttribute = string.Empty;
                if (!string.IsNullOrEmpty(nonce))
                {
                    nonceAttribute = $"nonce=\"{nonce}\"";
                }
                var script = string.Format(
                    @"
                    <script {0} >
                    (function() {{
                        var registerLinks = document.querySelectorAll('.dnnRegisterLink');
                        if (registerLinks.length > 0) {{
                            registerLinks.forEach(function(link) {{
                                link.addEventListener('click', function(e) {{
                                    e.preventDefault();
                                    var url = this.getAttribute('href');
                                    
                                    if (!navigator.userAgent.match(/MSIE 8.0/)) {{
                                        this.disabled = true;
                                    }}
                                ",
                    nonceAttribute);

                if (portalSettings.EnablePopUps &&
                    portalSettings.RegisterTabId == Null.NullInteger &&
                    !AuthenticationController.HasSocialAuthenticationEnabled(null))
                {
                    script += UrlUtils.PopUpUrl(loginUrl, null, portalSettings, true, false, 300, 650);
                }
                else
                {
                    script += "window.location = url;";
                }

                script += @"
                            return false;
                            });
                        }
                    });
                    </script>";

                return script;
            }

            return string.Empty;
        }
    }
}
