#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using System.Web;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Log.EventLog;

#endregion

namespace DotNetNuke.Services.Authentication
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The AuthenticationController class provides the Business Layer for the
    /// Authentication Systems.
    /// </summary>
    /// <history>
    /// 	[cnurse]	07/10/2007  Created
    /// </history>
    /// -----------------------------------------------------------------------------
    public class AuthenticationController
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (AuthenticationController));
		#region "Private Members"

        private static readonly DataProvider provider = DataProvider.Instance();

		#endregion

		#region "Private Shared Methods"

        private static object GetAuthenticationServicesCallBack(CacheItemArgs cacheItemArgs)
        {
            return CBO.FillCollection<AuthenticationInfo>(provider.GetAuthenticationServices());
        }
		
		#endregion

		#region "Public Shared Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddAuthentication adds a new Authentication System to the Data Store.
        /// </summary>
        /// <param name="authSystem">The new Authentication System to add</param>
        /// <history>
        /// 	[cnurse]	07/10/2007  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static int AddAuthentication(AuthenticationInfo authSystem)
        {
            var objEventLog = new EventLogController();
            objEventLog.AddLog(authSystem, PortalController.GetCurrentPortalSettings(), UserController.GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.AUTHENTICATION_CREATED);
            return provider.AddAuthentication(authSystem.PackageID,
                                              authSystem.AuthenticationType,
                                              authSystem.IsEnabled,
                                              authSystem.SettingsControlSrc,
                                              authSystem.LoginControlSrc,
                                              authSystem.LogoffControlSrc,
                                              UserController.GetCurrentUserInfo().UserID);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddUserAuthentication adds a new UserAuthentication to the User.
        /// </summary>
        /// <param name="userID">The new Authentication System to add</param>
        /// <param name="authenticationType">The authentication type</param>
        /// <param name="authenticationToken">The authentication token</param>
        /// <history>
        /// 	[cnurse]	07/12/2007  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static int AddUserAuthentication(int userID, string authenticationType, string authenticationToken)
        {
            var objEventLog = new EventLogController();
            objEventLog.AddLog("userID/authenticationType",
                               userID + "/" + authenticationType,
                               PortalController.GetCurrentPortalSettings(),
                               UserController.GetCurrentUserInfo().UserID,
                               EventLogController.EventLogType.AUTHENTICATION_USER_CREATED);
            return provider.AddUserAuthentication(userID, authenticationType, authenticationToken, UserController.GetCurrentUserInfo().UserID);
        }

        public static void DeleteAuthentication(AuthenticationInfo authSystem)
        {
            provider.DeleteAuthentication(authSystem.AuthenticationID);
            var objEventLog = new EventLogController();
            objEventLog.AddLog(authSystem, PortalController.GetCurrentPortalSettings(), UserController.GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.AUTHENTICATION_DELETED);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetAuthenticationService fetches a single Authentication Systems
        /// </summary>
        /// <param name="authenticationID">The ID of the Authentication System</param>
        /// <returns>An AuthenticationInfo object</returns>
        /// <history>
        /// 	[cnurse]	07/31/2007  Created
        /// </history>
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
				//Go to database
                return CBO.FillObject<AuthenticationInfo>(provider.GetAuthenticationService(authenticationID));
            }
            return authInfo;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetAuthenticationServiceByPackageID fetches a single Authentication System
        /// </summary>
        /// <param name="packageID">The id of the Package</param>
        /// <returns>An AuthenticationInfo object</returns>
        /// <history>
        /// 	[cnurse]	07/31/2007  Created
        /// </history>
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
				//Go to database
                return CBO.FillObject<AuthenticationInfo>(provider.GetAuthenticationServiceByPackageID(packageID));
            }
            return authInfo;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetAuthenticationServiceByType fetches a single Authentication Systems
        /// </summary>
        /// <param name="authenticationType">The type of the Authentication System</param>
        /// <returns>An AuthenticationInfo object</returns>
        /// <history>
        /// 	[cnurse]	07/31/2007  Created
        /// </history>
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
				//Go to database
                return CBO.FillObject<AuthenticationInfo>(provider.GetAuthenticationServiceByType(authenticationType));
            }
            return authInfo;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetAuthenticationServices fetches a list of all the Authentication Systems
        /// installed in the system
        /// </summary>
        /// <returns>A List of AuthenticationInfo objects</returns>
        /// <history>
        /// 	[cnurse]	07/10/2007  Created
        /// </history>
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
        /// GetAuthenticationType fetches the authentication method used by the currently logged on user
        /// </summary>
        /// <returns>An AuthenticationInfo object</returns>
        /// <history>
        /// 	[cnurse]	07/23/2007  Created
        /// </history>
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
        /// installed in the system that have been enabled by the Host user
        /// </summary>
        /// <returns>A List of AuthenticationInfo objects</returns>
        /// <history>
        /// 	[cnurse]	07/10/2007  Created
        /// </history>
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetLogoffRedirectURL fetches the url to redirect too after logoff
        /// </summary>
        /// <param name="settings">A PortalSettings object</param>
        /// <param name="request">The current Request</param>
        /// <returns>The Url</returns>
        /// <history>
        /// 	[cnurse]	08/15/2007  Created
        ///     [cnurse]    02/28/2008  DNN-6881 Logoff redirect
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string GetLogoffRedirectURL(PortalSettings settings, HttpRequest request)
        {
            string _RedirectURL = "";
            object setting = UserModuleBase.GetSetting(settings.PortalId, "Redirect_AfterLogout");
            if (Convert.ToInt32(setting) == Null.NullInteger)
            {
                if (TabPermissionController.CanViewPage())
                {
					//redirect to current page (or home page if current page is a profile page to reduce redirects)
		    if (settings.ActiveTab.TabID == settings.UserTabId || settings.ActiveTab.ParentId == settings.UserTabId)
		    {
			_RedirectURL = Globals.NavigateURL(settings.HomeTabId);
		    }
		    else
		    {
			_RedirectURL = Globals.NavigateURL(settings.ActiveTab.TabID);
		    }

                }
                else if (settings.HomeTabId != -1)
                {
					//redirect to portal home page specified
                    _RedirectURL = Globals.NavigateURL(settings.HomeTabId);
                }
                else //redirect to default portal root
                {
                    _RedirectURL = Globals.GetPortalDomainName(settings.PortalAlias.HTTPAlias, request, true) + "/" + Globals.glbDefaultPage;
                }
            }
            else //redirect to after logout page
            {
                _RedirectURL = Globals.NavigateURL(Convert.ToInt32(setting));
            }
            return _RedirectURL;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// SetAuthenticationType sets the authentication method used by the currently logged on user
        /// </summary>
        /// <param name="value">The Authentication type</param>
        /// <history>
        /// 	[cnurse]	07/23/2007  Created
        /// </history>
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
				
                //save the authenticationmethod as a cookie
                HttpCookie cookie = null;
                cookie = Response.Cookies.Get("authentication");
                if ((cookie == null))
                {
                    if (!String.IsNullOrEmpty(value))
                    {
                        cookie = new HttpCookie("authentication", value);
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
                    if (!String.IsNullOrEmpty(value))
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
        /// <param name="authSystem">The new Authentication System to update</param>
        /// <history>
        /// 	[cnurse]	07/10/2007  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static void UpdateAuthentication(AuthenticationInfo authSystem)
        {
            provider.UpdateAuthentication(authSystem.AuthenticationID,
                                          authSystem.PackageID,
                                          authSystem.AuthenticationType,
                                          authSystem.IsEnabled,
                                          authSystem.SettingsControlSrc,
                                          authSystem.LoginControlSrc,
                                          authSystem.LogoffControlSrc,
                                          UserController.GetCurrentUserInfo().UserID);
            var objEventLog = new EventLogController();
            objEventLog.AddLog(authSystem, PortalController.GetCurrentPortalSettings(), UserController.GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.AUTHENTICATION_UPDATED);
        }
		
		#endregion
    }
}