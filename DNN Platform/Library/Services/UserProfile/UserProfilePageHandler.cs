// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.UserProfile
{
    using System;
    using System.Globalization;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;

    public class UserProfilePageHandler : IHttpHandler
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UserProfilePageHandler));

        /// <inheritdoc/>
        public bool IsReusable
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        ///   This handler handles requests for LinkClick.aspx, but only those specific
        ///   to file serving.
        /// </summary>
        /// <param name="context">System.Web.HttpContext).</param>
        public void ProcessRequest(HttpContext context)
        {
            PortalSettings portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            int userId = Null.NullInteger;
            int portalId = portalSettings.PortalId;

            try
            {
                // try UserId
                if (!string.IsNullOrEmpty(context.Request.QueryString["UserId"]))
                {
                    userId = int.Parse(context.Request.QueryString["UserId"], CultureInfo.InvariantCulture);
                    if (UserController.GetUserById(portalId, userId) == null)
                    {
                        // The user cannot be found (potential DOS)
                        Exceptions.Exceptions.ProcessHttpException(context.Request);
                    }
                }

                if (userId == Null.NullInteger)
                {
                    // try userName
                    if (!string.IsNullOrEmpty(context.Request.QueryString["UserName"]))
                    {
                        userId = GetUserId(context.Request.QueryString["UserName"], portalId);
                    }
                }

                if (userId == Null.NullInteger)
                {
                    // try user
                    string user = context.Request.QueryString["User"];
                    if (!string.IsNullOrEmpty(user))
                    {
                        if (!int.TryParse(user, out userId))
                        {
                            // User is not an integer, so try it as a name
                            userId = GetUserId(user, portalId);
                        }
                        else
                        {
                            if (UserController.GetUserById(portalId, userId) == null)
                            {
                                // The user cannot be found (potential DOS)
                                Exceptions.Exceptions.ProcessHttpException(context.Request);
                            }
                        }
                    }
                }

                if (userId == Null.NullInteger)
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
            context.Response.Redirect(Globals.UserProfileURL(userId), true);
        }

        private static int GetUserId(string username, int portalId)
        {
            int userId = Null.NullInteger;
            UserInfo userInfo = UserController.GetUserByName(portalId, username);
            if (userInfo != null)
            {
                userId = userInfo.UserID;
            }
            else
            {
                // The user cannot be found (potential DOS)
                Exceptions.Exceptions.ProcessHttpException();
            }

            return userId;
        }
    }
}
