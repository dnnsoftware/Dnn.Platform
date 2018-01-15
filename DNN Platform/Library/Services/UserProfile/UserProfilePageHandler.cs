#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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