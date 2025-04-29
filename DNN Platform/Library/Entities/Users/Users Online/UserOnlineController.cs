// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Users;

using System;
using System.Collections;
using System.Web;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Instrumentation;
using DotNetNuke.Internal.SourceGenerators;

using MembershipProvider = DotNetNuke.Security.Membership.MembershipProvider;

/// <summary>The UserOnlineController class provides Business Layer methods for Users Online.</summary>
[DnnDeprecated(8, 0, 0, "Other solutions exist outside of the DNN Platform", RemovalVersion = 11)]
public partial class UserOnlineController
{
    private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UserOnlineController));
    private static readonly MembershipProvider MemberProvider = MembershipProvider.Instance();
    private static readonly object Locker = new object();
    private static readonly string CacheKey = "OnlineUserList";

    /// <summary>Clears the cached Users Online Information.</summary>
    public void ClearUserList()
    {
        string key = "OnlineUserList";
        DataCache.RemoveCache(key);
    }

    /// <summary>Gets the Online time window.</summary>
    /// <returns>The window to use in minutes when determining if the user is online.</returns>
    public int GetOnlineTimeWindow()
    {
        return Host.Host.UsersOnlineTimeWindow;
    }

    /// <summary>Gets the cached Users Online Information.</summary>
    /// <returns>A <see cref="Hashtable"/> with <see cref="string"/> keys for the user ID (a GUID for anonymous users) and <see cref="BaseUserInfo"/> instances for the values.</returns>
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

    /// <summary>Gets whether the Users Online functionality is enabled.</summary>
    /// <returns><see langword="true"/> if the Users Online functionality is enabled, otherwise <see langword="false"/>.</returns>
    public bool IsEnabled()
    {
        return Host.Host.EnableUsersOnline;
    }

    /// <summary>Determines whether a User is online.</summary>
    /// <returns><see langword="true"/> if the user is online, otherwise <see langword="false"/>.</returns>
    public bool IsUserOnline(UserInfo user)
    {
        bool isOnline = false;
        if (this.IsEnabled())
        {
            isOnline = MemberProvider.IsUserOnline(user);
        }

        return isOnline;
    }

    /// <summary>Sets the cached Users Online Information.</summary>
    public void SetUserList(Hashtable userList)
    {
        DataCache.SetCache(CacheKey, userList);
    }

    /// <summary>Tracks an online User.</summary>
    public void TrackUsers()
    {
        HttpContext context = HttpContext.Current;

        // Have we already done the work for this request?
        if (context.Items["CheckedUsersOnlineCookie"] != null)
        {
            return;
        }
        else
        {
            context.Items["CheckedUsersOnlineCookie"] = "true";
        }

        if (context.Request.IsAuthenticated)
        {
            this.TrackAuthenticatedUser(context);
        }
        else if (context.Request.Browser.Cookies)
        {
            this.TrackAnonymousUser(context);
        }
    }

    /// <summary>Update the Users Online information.</summary>
    public void UpdateUsersOnline()
    {
        // Get a Current User List
        Hashtable userList = this.GetUserList();

        // Create a shallow copy of the list to Process
        var listToProcess = (Hashtable)userList.Clone();

        // Clear the list
        this.ClearUserList();

        // Persist the current User List
        try
        {
            MemberProvider.UpdateUsersOnline(listToProcess);
        }
        catch (Exception exc)
        {
            Logger.Error(exc);
        }

        // Remove users that have expired
        MemberProvider.DeleteUsersOnline(this.GetOnlineTimeWindow());
    }

    /// <summary>Tracks an Anonymous User.</summary>
    /// <param name="context">An HttpContext Object.</param>
    private void TrackAnonymousUser(HttpContext context)
    {
        string cookieName = "DotNetNukeAnonymous";
        var portalSettings = (PortalSettings)context.Items["PortalSettings"];
        if (portalSettings == null)
        {
            return;
        }

        AnonymousUserInfo user;
        Hashtable userList = this.GetUserList();
        string userID;

        // Check if the Tracking cookie exists
        HttpCookie cookie = context.Request.Cookies[cookieName];

        // Track Anonymous User
        if (cookie == null)
        {
            // Create a temporary userId
            userID = Guid.NewGuid().ToString();

            // Create a new cookie
            cookie = new HttpCookie(cookieName, userID)
            {
                Expires = DateTime.Now.AddMinutes(20),
                Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/",
            };
            context.Response.Cookies.Add(cookie);

            // Create a user
            user = new AnonymousUserInfo
            {
                UserID = userID,
                PortalID = portalSettings.PortalId,
                TabID = portalSettings.ActiveTab.TabID,
                CreationDate = DateTime.Now,
                LastActiveDate = DateTime.Now,
            };

            // Add the user
            if (!userList.Contains(userID))
            {
                userList[userID] = user;
            }
        }
        else
        {
            if (cookie.Value == null)
            {
                // Expire the cookie, there is something wrong with it
                context.Response.Cookies[cookieName].Expires = new DateTime(1999, 10, 12);

                // No need to do anything else
                return;
            }

            // Get userID out of cookie
            userID = cookie.Value;

            // Find the cookie in the user list
            if (userList[userID] == null)
            {
                userList[userID] = new AnonymousUserInfo();
                ((AnonymousUserInfo)userList[userID]).CreationDate = DateTime.Now;
            }

            user = (AnonymousUserInfo)userList[userID];
            user.UserID = userID;
            user.PortalID = portalSettings.PortalId;
            user.TabID = portalSettings.ActiveTab.TabID;
            user.LastActiveDate = DateTime.Now;

            // Reset the expiration on the cookie
            cookie = new HttpCookie(cookieName, userID)
            {
                Expires = DateTime.Now.AddMinutes(20),
                Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/",
            };
            context.Response.Cookies.Add(cookie);
        }
    }

    /// <summary>Tracks an Authenticated User.</summary>
    /// <param name="context">An HttpContext Object.</param>
    private void TrackAuthenticatedUser(HttpContext context)
    {
        // Retrieve Portal Settings
        var portalSettings = (PortalSettings)context.Items["PortalSettings"];

        if (portalSettings == null)
        {
            return;
        }

        // Get the logged in User ID
        UserInfo objUserInfo = UserController.Instance.GetCurrentUserInfo();

        // Get user list
        Hashtable userList = this.GetUserList();

        var user = new OnlineUserInfo();
        if (objUserInfo.UserID > 0)
        {
            user.UserID = objUserInfo.UserID;
        }

        user.PortalID = portalSettings.PortalId;
        user.TabID = portalSettings.ActiveTab.TabID;
        user.LastActiveDate = DateTime.Now;
        if (userList[objUserInfo.UserID.ToString()] == null)
        {
            user.CreationDate = user.LastActiveDate;
        }

        userList[objUserInfo.UserID.ToString()] = user;
        this.SetUserList(userList);
    }
}
