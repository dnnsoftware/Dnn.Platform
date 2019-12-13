// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Web;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;

#endregion

namespace DotNetNuke.Services.UserProfile
{
    public class UserProfilePageHandler : IHttpHandler
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (UserProfilePageHandler));
        #region IHttpHandler Members

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   This handler handles requests for LinkClick.aspx, but only those specifc
        ///   to file serving
        /// </summary>
        /// <param name = "context">System.Web.HttpContext)</param>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public void ProcessRequest(HttpContext context)
        {
            PortalSettings _portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            int UserId = Null.NullInteger;
            int PortalId = _portalSettings.PortalId;

            try
            {
                //try UserId
                if (!string.IsNullOrEmpty(context.Request.QueryString["UserId"]))
                {
                    UserId = Int32.Parse(context.Request.QueryString["UserId"]);
                    if (UserController.GetUserById(PortalId, UserId) == null)
                    {
                        //The user cannot be found (potential DOS)
                        Exceptions.Exceptions.ProcessHttpException(context.Request);

                    }
                }

                if (UserId == Null.NullInteger)
                {
                    //try userName
                    if (!string.IsNullOrEmpty(context.Request.QueryString["UserName"]))
                    {
                        UserId = GetUserId(context.Request.QueryString["UserName"], PortalId);
                    }
                }

                if (UserId == Null.NullInteger)
                {
                    //try user
                    string user = context.Request.QueryString["User"];
                    if (!string.IsNullOrEmpty(user))
                    {
                        if (!Int32.TryParse(user, out UserId))
                        {
                            //User is not an integer, so try it as a name
                            UserId = GetUserId(user, PortalId);
                        }
                        else
                        {
                            if (UserController.GetUserById(PortalId, UserId) == null)
                            {
                                //The user cannot be found (potential DOS)
                                Exceptions.Exceptions.ProcessHttpException(context.Request);

                            }
                        }
                    }
                }

                if (UserId == Null.NullInteger)
                {
                    //The user cannot be found (potential DOS)
                    Exceptions.Exceptions.ProcessHttpException(context.Request);
                }
            }
            catch (Exception exc)
            {
                Logger.Debug(exc);
                //The user cannot be found (potential DOS)
                Exceptions.Exceptions.ProcessHttpException(context.Request);
            }

            //Redirect to Userprofile Page
            context.Response.Redirect(Globals.UserProfileURL(UserId), true);
        }

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }

        #endregion

        private static int GetUserId(string username, int PortalId)
        {
            int _UserId = Null.NullInteger;
            UserInfo userInfo = UserController.GetUserByName(PortalId, username);
            if (userInfo != null)
            {
                _UserId = userInfo.UserID;
            }
            else
            {
                //The user cannot be found (potential DOS)
                Exceptions.Exceptions.ProcessHttpException();
            }
            return _UserId;
        }
    }
}
