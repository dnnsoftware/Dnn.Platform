#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Collections;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Instrumentation;
using MembershipProvider = DotNetNuke.Security.Membership.MembershipProvider;

#endregion

namespace DotNetNuke.Entities.Users
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Users
    /// Class:      UserOnlineController
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The UserOnlineController class provides Business Layer methods for Users Online
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class UserOnlineController
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (UserOnlineController));
        private static readonly MembershipProvider memberProvider = MembershipProvider.Instance();
        private static readonly object Locker = new object();
        private static readonly string CacheKey = "OnlineUserList";

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Clears the cached Users Online Information
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void ClearUserList()
        {
            string key = "OnlineUserList";
            DataCache.RemoveCache(key);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Online time window
        /// </summary>
        /// -----------------------------------------------------------------------------
        public int GetOnlineTimeWindow()
        {
            return Host.Host.UsersOnlineTimeWindow;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the cached Users Online Information
        /// </summary>
        /// -----------------------------------------------------------------------------
        public Hashtable GetUserList()
        {
            var userList = (Hashtable)DataCache.GetCache(CacheKey);
            if (userList == null)
            {
                lock (Locker)
                {
                    userList = (Hashtable)DataCache.GetCache(CacheKey);
                    if (userList == null)
                    {
                        userList = Hashtable.Synchronized(new Hashtable());
                        DataCache.SetCache(CacheKey, userList);
                    }
                }
            }
            return userList;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether the Users Online functionality is enabled
        /// </summary>
        /// -----------------------------------------------------------------------------
        public bool IsEnabled()
        {
            return Host.Host.EnableUsersOnline;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Determines whether a User is online
        /// </summary>
        /// -----------------------------------------------------------------------------
        public bool IsUserOnline(UserInfo user)
        {
            bool isOnline = false;
            if (IsEnabled())
            {
                isOnline = memberProvider.IsUserOnline(user);
            }
            return isOnline;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Sets the cached Users Online Information
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void SetUserList(Hashtable userList)
        {
            DataCache.SetCache(CacheKey, userList);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Tracks an Anonymous User
        /// </summary>
        /// <param name="context">An HttpContext Object</param>
        /// -----------------------------------------------------------------------------
        private void TrackAnonymousUser(HttpContext context)
        {
            string cookieName = "DotNetNukeAnonymous";
            var portalSettings = (PortalSettings) context.Items["PortalSettings"];
            if (portalSettings == null)
            {
                return;
            }
            AnonymousUserInfo user;
            Hashtable userList = GetUserList();
            string userID;

            //Check if the Tracking cookie exists
            HttpCookie cookie = context.Request.Cookies[cookieName];
            //Track Anonymous User
			if ((cookie == null))
            {  	
                //Create a temporary userId
                userID = Guid.NewGuid().ToString();

                //Create a new cookie
                cookie = new HttpCookie(cookieName, userID)
                {
                    Expires = DateTime.Now.AddMinutes(20),
                    Path = (!string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/")
                };
                context.Response.Cookies.Add(cookie);

                //Create a user
                user = new AnonymousUserInfo
                {
                    UserID = userID,
                    PortalID = portalSettings.PortalId,
                    TabID = portalSettings.ActiveTab.TabID,
                    CreationDate = DateTime.Now,
                    LastActiveDate = DateTime.Now
                };

                //Add the user
                if (!userList.Contains(userID))
                {
                    userList[userID] = user;
                }
            }
            else
            {
                if ((cookie.Value == null))
                {
					//Expire the cookie, there is something wrong with it
                    context.Response.Cookies[cookieName].Expires = new DateTime(1999, 10, 12);

                    //No need to do anything else
                    return;
                }
				
                //Get userID out of cookie
                userID = cookie.Value;

                //Find the cookie in the user list
                if ((userList[userID] == null))
                {
                    userList[userID] = new AnonymousUserInfo();
                    ((AnonymousUserInfo) userList[userID]).CreationDate = DateTime.Now;
                }
				
                user = (AnonymousUserInfo) userList[userID];
                user.UserID = userID;
                user.PortalID = portalSettings.PortalId;
                user.TabID = portalSettings.ActiveTab.TabID;
                user.LastActiveDate = DateTime.Now;

                //Reset the expiration on the cookie
                cookie = new HttpCookie(cookieName, userID)
                {
                    Expires = DateTime.Now.AddMinutes(20),
                    Path = (!string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/")
                };
                context.Response.Cookies.Add(cookie);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Tracks an Authenticated User
        /// </summary>
        /// <param name="context">An HttpContext Object</param>
        /// -----------------------------------------------------------------------------
        private void TrackAuthenticatedUser(HttpContext context)
        {
            //Retrieve Portal Settings
            var portalSettings = (PortalSettings) context.Items["PortalSettings"];

            if (portalSettings == null)
            {
                return;
            }
            //Get the logged in User ID
            UserInfo objUserInfo = UserController.Instance.GetCurrentUserInfo();

            //Get user list
            Hashtable userList = GetUserList();

            var user = new OnlineUserInfo();
            if (objUserInfo.UserID > 0)
            {
                user.UserID = objUserInfo.UserID;
            }
            user.PortalID = portalSettings.PortalId;
            user.TabID = portalSettings.ActiveTab.TabID;
            user.LastActiveDate = DateTime.Now;
            if ((userList[objUserInfo.UserID.ToString()] == null))
            {
                user.CreationDate = user.LastActiveDate;
            }
            userList[objUserInfo.UserID.ToString()] = user;
            SetUserList(userList);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Tracks an online User
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void TrackUsers()
        {
            HttpContext context = HttpContext.Current;

            //Have we already done the work for this request?
            if (context.Items["CheckedUsersOnlineCookie"] != null)
            {
                return;
            }
            else
            {
                context.Items["CheckedUsersOnlineCookie"] = "true";
            }
            if ((context.Request.IsAuthenticated))
            {
                TrackAuthenticatedUser(context);
            }
            else if ((context.Request.Browser.Cookies))
            {
                TrackAnonymousUser(context);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Update the Users Online information
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void UpdateUsersOnline()
        {
            //Get a Current User List
            Hashtable userList = GetUserList();

            //Create a shallow copy of the list to Process
            var listToProcess = (Hashtable) userList.Clone();

            //Clear the list
            ClearUserList();
			
			//Persist the current User List
            try
            {
                memberProvider.UpdateUsersOnline(listToProcess);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);

            }
			
            //Remove users that have expired
            memberProvider.DeleteUsersOnline(GetOnlineTimeWindow());
        }
    }
}