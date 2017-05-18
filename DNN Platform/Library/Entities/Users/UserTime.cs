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
using System.Web;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.SystemDateTime;
using DotNetNuke.Entities.Portals;

#endregion

namespace DotNetNuke.Entities.Users
{
    public class UserTime
    {
        [Obsolete("Deprecated in DNN 6.0.1. Replaced by UserInfo.LocalTime")]
        public DateTime CurrentUserTime
        {
            get
            {
                HttpContext context = HttpContext.Current;
            	//Obtain PortalSettings from Current Context
                PortalSettings objSettings = PortalController.Instance.GetCurrentPortalSettings();
                if (!context.Request.IsAuthenticated)
                {
                    return TimeZoneInfo.ConvertTime(DateUtils.GetDatabaseTime(), TimeZoneInfo.Utc, objSettings.TimeZone);
                }
                else
                {
                    UserInfo objUserInfo = UserController.Instance.GetCurrentUserInfo();
                    return TimeZoneInfo.ConvertTime(DateUtils.GetDatabaseTime(), TimeZoneInfo.Utc, objUserInfo.Profile.PreferredTimeZone);
                }
            }
        }

        [Obsolete("Deprecated in DNN 6.0.")]
        public double ClientToServerTimeZoneFactor
        {
            get
            {
                PortalSettings portalSettings = PortalController.Instance.GetCurrentPortalSettings();
                UserInfo objUserInfo = UserController.Instance.GetCurrentUserInfo();
                return FromClientToServerFactor(objUserInfo.Profile.PreferredTimeZone.BaseUtcOffset.TotalMinutes, portalSettings.TimeZone.BaseUtcOffset.TotalMinutes);
            }
        }

        [Obsolete("Deprecated in DNN 6.0.")]
        public double ServerToClientTimeZoneFactor
        {
            get
            {
                UserInfo objUserInfo = UserController.Instance.GetCurrentUserInfo();
                PortalSettings portalSettings = PortalController.Instance.GetCurrentPortalSettings();
                return FromServerToClientFactor(objUserInfo.Profile.PreferredTimeZone.BaseUtcOffset.TotalMinutes, portalSettings.TimeZone.BaseUtcOffset.TotalMinutes);
            }
        }

        [Obsolete("Deprecated in DNN 6.0.")]
        public DateTime ConvertToUserTime(DateTime dt, double clientTimeZone)
        {
            PortalSettings portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            return dt.AddMinutes(FromClientToServerFactor(clientTimeZone, portalSettings.TimeZone.BaseUtcOffset.TotalMinutes));            
        }

        [Obsolete("Deprecated in DNN 6.0.")]
        public DateTime ConvertToServerTime(DateTime dt, double clientTimeZone)
        {
        	//Obtain PortalSettings from Current Context
            PortalSettings portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            return dt.AddMinutes(FromServerToClientFactor(clientTimeZone, portalSettings.TimeZone.BaseUtcOffset.TotalMinutes));
        }

        [Obsolete("Deprecated in DNN 6.0.1. Replaced by UserInfo.LocalTime")]
        public static DateTime CurrentTimeForUser(UserInfo userInfo)
        {
            if (userInfo == null || userInfo.UserID == -1)
            {
				//Obtain PortalSettings from Current Context             
                PortalSettings objSettings = PortalController.Instance.GetCurrentPortalSettings();
                return TimeZoneInfo.ConvertTime(DateUtils.GetDatabaseTime(), TimeZoneInfo.Utc, objSettings.TimeZone);
            }
            else
            {
                return TimeZoneInfo.ConvertTime(DateUtils.GetDatabaseTime(), TimeZoneInfo.Utc, userInfo.Profile.PreferredTimeZone);
            }
        }

        private double FromClientToServerFactor(double Client, double Server)
        {
            return Client - Server;
        }

        private double FromServerToClientFactor(double Client, double Server)
        {
            return Server - Client;
        }
    }
}