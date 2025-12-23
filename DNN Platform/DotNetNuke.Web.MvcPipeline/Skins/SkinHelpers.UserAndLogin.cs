// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Skins
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
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
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Skin helper methods for rendering combined user and login UI elements.
    /// </summary>
    public static partial class SkinHelpers
    {
        /// <summary>
        /// Renders the combined user/login menu used by legacy DNN skins.
        /// </summary>
        /// <param name="helper">The HTML helper for the current <see cref="PageModel"/>.</param>
        /// <param name="showInErrorPage">If set to <c>true</c>, shows the control even on error pages.</param>
        /// <returns>An HTML string representing the user/login UI.</returns>
        public static IHtmlString UserAndLogin(this HtmlHelper<PageModel> helper, bool showInErrorPage = false)
        {
            var portalSettings = PortalSettings.Current;
            var navigationManager = helper.ViewData.Model.NavigationManager;

            if (!showInErrorPage && portalSettings.InErrorPageRequest())
            {
                return MvcHtmlString.Empty;
            }

            var sb = new StringBuilder();
            sb.Append("<div class=\"userProperties\"><ul>");

            if (!HttpContext.Current.Request.IsAuthenticated)
            {
                if (CanRegister(portalSettings))
                {
                    sb.Append($"<li class=\"userRegister\"><a id=\"registerLink\" href=\"{RegisterUrl(navigationManager)}\">{LocalizeString(helper, "Register")}</a></li>");
                }

                if (!portalSettings.HideLoginControl)
                {
                    sb.Append($"<li class=\"userLogin\"><a id=\"loginLink\" href=\"{LoginUrl()}\">{LocalizeString(helper, "Login")}</a></li>");
                }
            }
            else
            {
                var userInfo = portalSettings.UserInfo;
                sb.Append($"<li class=\"userName\"><a id=\"dnn_dnnUser_userNameLink\" href=\"#\">{userInfo.DisplayName}</a>");
                sb.Append("<ul class=\"userMenu\">");
                sb.Append($"<li class=\"viewProfile\"><a id=\"viewProfileLink\" href=\"{Globals.UserProfileURL(userInfo.UserID)}\">{LocalizeString(helper, "Profile")}</a></li>");

                var unreadMessages = InternalMessagingController.Instance.CountUnreadMessages(userInfo.UserID, portalSettings.PortalId);
                var unreadAlerts = NotificationsController.Instance.CountNotifications(userInfo.UserID, portalSettings.PortalId);
                var messageTabId = GetMessageTab(portalSettings);
                var alwaysShowCount = AlwaysShowCount(portalSettings);

                sb.Append($"<li class=\"userMessages\"><a id=\"messagesLink\" href=\"{navigationManager.NavigateURL(messageTabId, string.Empty, $"userId={userInfo.UserID}")}\"><span id=\"messageCount\" {(unreadMessages > 0 || alwaysShowCount ? string.Empty : "style=\"display:none;\"")}>{unreadMessages}</span>{LocalizeString(helper, "Messages")}</a></li>");
                sb.Append($"<li class=\"userNotifications\"><a id=\"notificationsLink\" href=\"{navigationManager.NavigateURL(messageTabId, string.Empty, $"userId={userInfo.UserID}", "view=notifications", "action=notifications")}\"><span id=\"notificationCount\" {(unreadAlerts > 0 || alwaysShowCount ? string.Empty : "style=\"display:none;\"")}>{unreadAlerts}</span>{LocalizeString(helper, "Notifications")}</a></li>");
                sb.Append($"<li class=\"userSettings\"><a id=\"accountLink\" href=\"{navigationManager.NavigateURL(portalSettings.UserTabId, "Profile", $"userId={userInfo.UserID}", "pageno=1")}\">{LocalizeString(helper, "Account")}</a></li>");
                sb.Append($"<li class=\"userProfilename\"><a id=\"editProfileLink\" href=\"{navigationManager.NavigateURL(portalSettings.UserTabId, "Profile", $"userId={userInfo.UserID}", "pageno=2")}\">{LocalizeString(helper, "EditProfile")}</a></li>");
                sb.Append($"<li class=\"userLogout\"><a id=\"logoffLink\" href=\"{navigationManager.NavigateURL(portalSettings.ActiveTab.TabID, "Logoff")}\"><strong>{LocalizeString(helper, "Logout")}</strong></a></li>");
                sb.Append("</ul></li>");

                sb.Append("<li class=\"userProfile\">");
                sb.Append($"<a id=\"viewProfileImageLink\" href=\"{Globals.UserProfileURL(userInfo.UserID)}\"><span class=\"userProfileImg\"><img id=\"profilePicture\" src=\"{UserController.Instance.GetUserProfilePictureUrl(userInfo.UserID, 32, 32)}\" alt=\"{LocalizeString(helper, "ProfilePicture")}\" /></span></a>");
                if (unreadMessages > 0)
                {
                    sb.Append($"<span id=\"messages\" class=\"userMessages\" title=\"{(unreadMessages == 1 ? LocalizeString(helper, "OneMessage") : string.Format(LocalizeString(helper, "MessageCount"), unreadMessages))}\">{unreadMessages}</span>");
                }

                sb.Append("</li>");
            }

            sb.Append("</ul></div>");

            var result = sb.ToString();

            if (UsePopUp(portalSettings))
            {
                result = result.Replace("id=\"registerLink\"", $"id=\"registerLink\" onclick=\"{RegisterUrlForClickEvent(navigationManager, portalSettings, helper)}\"");
                result = result.Replace("id=\"loginLink\"", $"id=\"loginLink\" onclick=\"{LoginUrlForClickEvent(portalSettings, helper)}\"");
            }

            return new MvcHtmlString(result);
        }

        private static bool CanRegister(PortalSettings portalSettings)
        {
            return (portalSettings.UserRegistration != (int)Globals.PortalRegistrationType.NoRegistration)
                && (portalSettings.Users < portalSettings.UserQuota || portalSettings.UserQuota == 0);
        }

        private static string RegisterUrl(INavigationManager navigationManager)
        {
            return Globals.RegisterURL(HttpUtility.UrlEncode(navigationManager.NavigateURL()), Null.NullString);
        }

        private static string LoginUrl()
        {
            string returnUrl = HttpContext.Current.Request.RawUrl;
            if (returnUrl.IndexOf("?returnurl=", StringComparison.OrdinalIgnoreCase) != -1)
            {
                returnUrl = returnUrl.Substring(0, returnUrl.IndexOf("?returnurl=", StringComparison.OrdinalIgnoreCase));
            }

            returnUrl = HttpUtility.UrlEncode(returnUrl);

            return Globals.LoginURL(returnUrl, HttpContext.Current.Request.QueryString["override"] != null);
        }

        private static bool UsePopUp(PortalSettings portalSettings)
        {
            return portalSettings.EnablePopUps
                && portalSettings.LoginTabId == Null.NullInteger
               /* && !AuthenticationController.HasSocialAuthenticationEnabled(portalSettings)*/;
        }

        private static string RegisterUrlForClickEvent(INavigationManager navigationManager, PortalSettings portalSettings, HtmlHelper helper)
        {
            return "return " + UrlUtils.PopUpUrl(HttpUtility.UrlDecode(RegisterUrl(navigationManager)), portalSettings, true, false, 600, 950);
        }

        private static string LoginUrlForClickEvent(PortalSettings portalSettings, HtmlHelper helper)
        {
            return "return " + UrlUtils.PopUpUrl(HttpUtility.UrlDecode(LoginUrl()), portalSettings, true, false, 300, 650);
        }

        private static string LocalizeString(HtmlHelper helper, string key)
        {
            return Localization.GetString(key, GetSkinsResourceFile("UserAndLogin.ascx"));
        }
    }
}
