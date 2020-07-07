// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Authentication
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.UI;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.UI.Skins;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The AuthenticationController class provides the Business Layer for the
    /// Authentication Systems.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class AuthenticationController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AuthenticationController));
        private static readonly DataProvider provider = DataProvider.Instance();

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddAuthentication adds a new Authentication System to the Data Store.
        /// </summary>
        /// <param name="authSystem">The new Authentication System to add.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static int AddAuthentication(AuthenticationInfo authSystem)
        {
            EventLogController.Instance.AddLog(authSystem, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.AUTHENTICATION_CREATED);
            return provider.AddAuthentication(
                authSystem.PackageID,
                authSystem.AuthenticationType,
                authSystem.IsEnabled,
                authSystem.SettingsControlSrc,
                authSystem.LoginControlSrc,
                authSystem.LogoffControlSrc,
                UserController.Instance.GetCurrentUserInfo().UserID);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddUserAuthentication adds a new UserAuthentication to the User.
        /// </summary>
        /// <param name="userID">The new Authentication System to add.</param>
        /// <param name="authenticationType">The authentication type.</param>
        /// <param name="authenticationToken">The authentication token.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static int AddUserAuthentication(int userID, string authenticationType, string authenticationToken)
        {
            UserAuthenticationInfo userAuth = GetUserAuthentication(userID);

            if (userAuth == null || string.IsNullOrEmpty(userAuth.AuthenticationType))
            {
                EventLogController.Instance.AddLog(
                    "userID/authenticationType",
                    userID + "/" + authenticationType,
                    PortalController.Instance.GetCurrentPortalSettings(),
                    UserController.Instance.GetCurrentUserInfo().UserID,
                    EventLogController.EventLogType.AUTHENTICATION_USER_CREATED);
                return provider.AddUserAuthentication(userID, authenticationType, authenticationToken, UserController.Instance.GetCurrentUserInfo().UserID);
            }
            else
            {
                EventLogController.Instance.AddLog(
                    "userID/authenticationType already exists",
                    userID + "/" + authenticationType,
                    PortalController.Instance.GetCurrentPortalSettings(),
                    UserController.Instance.GetCurrentUserInfo().UserID,
                    EventLogController.EventLogType.AUTHENTICATION_USER_UPDATED);

                return userAuth.UserAuthenticationID;
            }
        }

        /// <summary>
        /// Retrieves authentication information for an user.
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public static UserAuthenticationInfo GetUserAuthentication(int userID)
        {
            // Go to database
            return CBO.FillObject<UserAuthenticationInfo>(provider.GetUserAuthentication(userID));
        }

        public static void DeleteAuthentication(AuthenticationInfo authSystem)
        {
            provider.DeleteAuthentication(authSystem.AuthenticationID);
            EventLogController.Instance.AddLog(authSystem, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.AUTHENTICATION_DELETED);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetAuthenticationService fetches a single Authentication Systems.
        /// </summary>
        /// <param name="authenticationID">The ID of the Authentication System.</param>
        /// <returns>An AuthenticationInfo object.</returns>
        /// -----------------------------------------------------------------------------
        public static AuthenticationInfo GetAuthenticationService(int authenticationID)
        {
            AuthenticationInfo authInfo = null;
            foreach (AuthenticationInfo authService in GetAuthenticationServices())
            {
                if (authService.AuthenticationID == authenticationID)
                {
                    authInfo = authService;
                    break;
                }
            }

            if (authInfo == null)
            {
                // Go to database
                return CBO.FillObject<AuthenticationInfo>(provider.GetAuthenticationService(authenticationID));
            }

            return authInfo;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetAuthenticationServiceByPackageID fetches a single Authentication System.
        /// </summary>
        /// <param name="packageID">The id of the Package.</param>
        /// <returns>An AuthenticationInfo object.</returns>
        /// -----------------------------------------------------------------------------
        public static AuthenticationInfo GetAuthenticationServiceByPackageID(int packageID)
        {
            AuthenticationInfo authInfo = null;
            foreach (AuthenticationInfo authService in GetAuthenticationServices())
            {
                if (authService.PackageID == packageID)
                {
                    authInfo = authService;
                    break;
                }
            }

            if (authInfo == null)
            {
                // Go to database
                return CBO.FillObject<AuthenticationInfo>(provider.GetAuthenticationServiceByPackageID(packageID));
            }

            return authInfo;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetAuthenticationServiceByType fetches a single Authentication Systems.
        /// </summary>
        /// <param name="authenticationType">The type of the Authentication System.</param>
        /// <returns>An AuthenticationInfo object.</returns>
        /// -----------------------------------------------------------------------------
        public static AuthenticationInfo GetAuthenticationServiceByType(string authenticationType)
        {
            AuthenticationInfo authInfo = null;
            foreach (AuthenticationInfo authService in GetAuthenticationServices())
            {
                if (authService.AuthenticationType == authenticationType)
                {
                    authInfo = authService;
                    break;
                }
            }

            if (authInfo == null)
            {
                // Go to database
                return CBO.FillObject<AuthenticationInfo>(provider.GetAuthenticationServiceByType(authenticationType));
            }

            return authInfo;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetAuthenticationServices fetches a list of all the Authentication Systems
        /// installed in the system.
        /// </summary>
        /// <returns>A List of AuthenticationInfo objects.</returns>
        /// -----------------------------------------------------------------------------
        public static List<AuthenticationInfo> GetAuthenticationServices()
        {
            return
                CBO.GetCachedObject<List<AuthenticationInfo>>(
                    new CacheItemArgs(DataCache.AuthenticationServicesCacheKey, DataCache.AuthenticationServicesCacheTimeOut, DataCache.AuthenticationServicesCachePriority),
                    GetAuthenticationServicesCallBack);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetAuthenticationType fetches the authentication method used by the currently logged on user.
        /// </summary>
        /// <returns>An AuthenticationInfo object.</returns>
        /// -----------------------------------------------------------------------------
        public static AuthenticationInfo GetAuthenticationType()
        {
            AuthenticationInfo objAuthentication = null;
            if (HttpContext.Current != null && HttpContext.Current.Request != null)
            {
                try
                {
                    objAuthentication = GetAuthenticationServiceByType(HttpContext.Current.Request["authentication"]);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }

            return objAuthentication;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetEnabledAuthenticationServices fetches a list of all the Authentication Systems
        /// installed in the system that have been enabled by the Host user.
        /// </summary>
        /// <returns>A List of AuthenticationInfo objects.</returns>
        /// -----------------------------------------------------------------------------
        public static List<AuthenticationInfo> GetEnabledAuthenticationServices()
        {
            var enabled = new List<AuthenticationInfo>();
            foreach (AuthenticationInfo authService in GetAuthenticationServices())
            {
                if (authService.IsEnabled)
                {
                    enabled.Add(authService);
                }
            }

            return enabled;
        }

        /// <summary>
        /// Determines whether the current portal has any Non-DNN authentication providers enabled.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns><c>true</c> if the portal has any Non-DNN authentication enabled, Otherwise <c>false</c>.</returns>
        public static bool HasSocialAuthenticationEnabled(UserControl control = null)
        {
            return (from a in GetEnabledAuthenticationServices()
                    let enabled = (a.AuthenticationType.Equals("Facebook")
                                     || a.AuthenticationType.Equals("Google")
                                     || a.AuthenticationType.Equals("Live")
                                     || a.AuthenticationType.Equals("Twitter"))
                                  ? IsEnabledForPortal(a, PortalSettings.Current.PortalId)
                                  : !string.IsNullOrEmpty(a.LoginControlSrc) && ((control?.LoadControl("~/" + a.LoginControlSrc) as AuthenticationLoginBase)?.Enabled ?? true)
                    where !a.AuthenticationType.Equals("DNN") && enabled
                    select a).Any();
        }

        /// <summary>
        /// Determines whether the authentication is enabled for the specified portal.
        /// </summary>
        /// <param name="authentication">The authentication.</param>
        /// <param name="portalId">The portal identifier.</param>
        /// <returns><c>true</c> if OAuth Provider and it is enabled for the portal, Otherwise <c>false</c>.</returns>
        public static bool IsEnabledForPortal(AuthenticationInfo authentication, int portalId)
        {
            return !string.IsNullOrEmpty(PortalController.GetPortalSetting(authentication.AuthenticationType + "_Enabled", portalId, string.Empty))
                ? PortalController.GetPortalSettingAsBoolean(authentication.AuthenticationType + "_Enabled", portalId, false)
                : HostController.Instance.GetBoolean(authentication.AuthenticationType + "_Enabled", false);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetLogoffRedirectURL fetches the url to redirect too after logoff.
        /// </summary>
        /// <param name="settings">A PortalSettings object.</param>
        /// <param name="request">The current Request.</param>
        /// <returns>The Url.</returns>
        /// -----------------------------------------------------------------------------
        public static string GetLogoffRedirectURL(PortalSettings settings, HttpRequest request)
        {
            string _RedirectURL = string.Empty;
            if (settings.Registration.RedirectAfterLogout == Null.NullInteger)
            {
                if (TabPermissionController.CanViewPage())
                {
                    // redirect to current page (or home page if current page is a profile page to reduce redirects)
                    if (settings.ActiveTab.TabID == settings.UserTabId || settings.ActiveTab.ParentId == settings.UserTabId)
                    {
                        _RedirectURL = TestableGlobals.Instance.NavigateURL(settings.HomeTabId);
                    }
                    else
                    {
                        _RedirectURL = (request != null && request.UrlReferrer != null) ? request.UrlReferrer.PathAndQuery : TestableGlobals.Instance.NavigateURL(settings.ActiveTab.TabID);
                    }
                }
                else if (settings.HomeTabId != -1)
                {
                    // redirect to portal home page specified
                    _RedirectURL = TestableGlobals.Instance.NavigateURL(settings.HomeTabId);
                }
                else // redirect to default portal root
                {
                    _RedirectURL = TestableGlobals.Instance.GetPortalDomainName(settings.PortalAlias.HTTPAlias, request, true) + "/" + Globals.glbDefaultPage;
                }
            }
            else // redirect to after logout page
            {
                _RedirectURL = TestableGlobals.Instance.NavigateURL(settings.Registration.RedirectAfterLogout);
            }

            return _RedirectURL;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// SetAuthenticationType sets the authentication method used by the currently logged on user.
        /// </summary>
        /// <param name="value">The Authentication type.</param>
        /// -----------------------------------------------------------------------------
        public static void SetAuthenticationType(string value)
        {
            SetAuthenticationType(value, false);
        }

        public static void SetAuthenticationType(string value, bool CreatePersistentCookie)
        {
            try
            {
                int PersistentCookieTimeout = Config.GetPersistentCookieTimeout();
                HttpResponse Response = HttpContext.Current.Response;
                if (Response == null)
                {
                    return;
                }

                // save the authenticationmethod as a cookie
                HttpCookie cookie = null;
                cookie = Response.Cookies.Get("authentication");
                if (cookie == null)
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        cookie = new HttpCookie("authentication", value) { Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/" };
                        if (CreatePersistentCookie)
                        {
                            cookie.Expires = DateTime.Now.AddMinutes(PersistentCookieTimeout);
                        }

                        Response.Cookies.Add(cookie);
                    }
                }
                else
                {
                    cookie.Value = value;
                    if (!string.IsNullOrEmpty(value))
                    {
                        if (CreatePersistentCookie)
                        {
                            cookie.Expires = DateTime.Now.AddMinutes(PersistentCookieTimeout);
                        }

                        Response.Cookies.Set(cookie);
                    }
                    else
                    {
                        Response.Cookies.Remove("authentication");
                    }
                }
            }
            catch
            {
                return;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UpdateAuthentication updates an existing Authentication System in the Data Store.
        /// </summary>
        /// <param name="authSystem">The new Authentication System to update.</param>
        /// -----------------------------------------------------------------------------
        public static void UpdateAuthentication(AuthenticationInfo authSystem)
        {
            provider.UpdateAuthentication(
                authSystem.AuthenticationID,
                authSystem.PackageID,
                authSystem.AuthenticationType,
                authSystem.IsEnabled,
                authSystem.SettingsControlSrc,
                authSystem.LoginControlSrc,
                authSystem.LogoffControlSrc,
                UserController.Instance.GetCurrentUserInfo().UserID);
            EventLogController.Instance.AddLog(authSystem, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.AUTHENTICATION_UPDATED);
        }

        private static object GetAuthenticationServicesCallBack(CacheItemArgs cacheItemArgs)
        {
            return CBO.FillCollection<AuthenticationInfo>(provider.GetAuthenticationServices());
        }
    }
}
