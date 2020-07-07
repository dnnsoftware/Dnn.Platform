// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.UserProfile
{
    using System;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;

    public class UserProfilePageHandler : IHttpHandler
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UserProfilePageHandler));

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   This handler handles requests for LinkClick.aspx, but only those specifc
        ///   to file serving.
        /// </summary>
        /// <param name = "context">System.Web.HttpContext).</param>
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
                // try UserId
                if (!string.IsNullOrEmpty(context.Request.QueryString["UserId"]))
                {
                    UserId = int.Parse(context.Request.QueryString["UserId"]);
                    if (UserController.GetUserById(PortalId, UserId) == null)
                    {
                        // The user cannot be found (potential DOS)
                        Exceptions.Exceptions.ProcessHttpException(context.Request);
                    }
                }

                if (UserId == Null.NullInteger)
                {
                    // try userName
                    if (!string.IsNullOrEmpty(context.Request.QueryString["UserName"]))
                    {
                        UserId = GetUserId(context.Request.QueryString["UserName"], PortalId);
                    }
                }

                if (UserId == Null.NullInteger)
                {
                    // try user
                    string user = context.Request.QueryString["User"];
                    if (!string.IsNullOrEmpty(user))
                    {
                        if (!int.TryParse(user, out UserId))
                        {
                            // User is not an integer, so try it as a name
                            UserId = GetUserId(user, PortalId);
                        }
                        else
                        {
                            if (UserController.GetUserById(PortalId, UserId) == null)
                            {
                                // The user cannot be found (potential DOS)
                                Exceptions.Exceptions.ProcessHttpException(context.Request);
                            }
                        }
                    }
                }

                if (UserId == Null.NullInteger)
                {
                    // The user cannot be found (potential DOS)
                    Exceptions.Exceptions.ProcessHttpException(context.Request);
                }
            }
            catch (Exception exc)
            {
                Logger.Debug(exc);

                // The user cannot be found (potential DOS)
                Exceptions.Exceptions.ProcessHttpException(context.Request);
            }

            // Redirect to Userprofile Page
            context.Response.Redirect(Globals.UserProfileURL(UserId), true);
        }

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
                // The user cannot be found (potential DOS)
                Exceptions.Exceptions.ProcessHttpException();
            }

            return _UserId;
        }
    }
}
