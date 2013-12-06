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
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Web;

using DotNetNuke.Application;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Personalization;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.UI.Skins.EventListeners;
using DotNetNuke.Security.Roles.Internal;

#endregion

namespace DotNetNuke.HttpModules.Membership
{
    public class MembershipModule : IHttpModule
    {
	    private static string _cultureCode;
        public string ModuleName
        {
            get
            {
                return "DNNMembershipModule";
            }
        }

	    private static string CurrentCulture
	    {
		    get
		    {
			    if (string.IsNullOrEmpty(_cultureCode))
			    {
				    _cultureCode = Localization.GetPageLocale(PortalSettings.Current).Name; 
			    }

			    return _cultureCode;
		    }
	    }

        #region IHttpModule Members

        public void Init(HttpApplication application)
        {
            application.AuthenticateRequest += OnAuthenticateRequest;
        }

        public void Dispose()
        {
        }

        #endregion

        private void OnAuthenticateRequest(object sender, EventArgs e)
        {
            var application = (HttpApplication) sender;
            AuthenticateRequest(new HttpContextWrapper(application.Context), false);
        }

        public static void OnUnverifiedUserSkinInit(object sender, SkinEventArgs e)
        {
			var strMessage = Localization.GetString("UnverifiedUser", Localization.SharedResourceFile, CurrentCulture);
            UI.Skins.Skin.AddPageMessage(e.Skin, "", strMessage, ModuleMessage.ModuleMessageType.YellowWarning);
        }

        public static void AuthenticateRequest(HttpContextBase context, bool allowUnknownExtensinons)
        {
            HttpRequestBase request = context.Request;
            HttpResponseBase response = context.Response;

            //First check if we are upgrading/installing
            if (request == null || request.Url == null
                || request.Url.LocalPath.ToLower().EndsWith("install.aspx")
                || request.Url.LocalPath.ToLower().Contains("upgradewizard.aspx")
                || request.Url.LocalPath.ToLower().Contains("installwizard.aspx"))
            {
                return;
            }

            //exit if a request for a .net mapping that isn't a content page is made i.e. axd
            if (allowUnknownExtensinons == false
                && request.Url.LocalPath.ToLower().EndsWith(".aspx") == false
                && request.Url.LocalPath.ToLower().EndsWith(".asmx") == false
                && request.Url.LocalPath.ToLower().EndsWith(".ashx") == false)
            {
                return;
            }

            //Obtain PortalSettings from Current Context
            PortalSettings portalSettings = PortalController.GetCurrentPortalSettings();

            bool isActiveDirectoryAuthHeaderPresent = false;
            var auth = request.Headers.Get("Authorization");
            if(!string.IsNullOrEmpty(auth))
            {
                if(auth.StartsWith("Negotiate"))
                {
                    isActiveDirectoryAuthHeaderPresent = true;
                }
            }

            if (request.IsAuthenticated && !isActiveDirectoryAuthHeaderPresent && portalSettings != null)
            {
                var roleController = new RoleController();
                var user = UserController.GetCachedUser(portalSettings.PortalId, context.User.Identity.Name);
				//if current login is from windows authentication, the ignore the process
				if (user == null && context.User is WindowsPrincipal)
				{
					return;
				}

                //authenticate user and set last login ( this is necessary for users who have a permanent Auth cookie set ) 
                if (user == null || user.IsDeleted || user.Membership.LockedOut
                    || (!user.Membership.Approved && !user.IsInRole("Unverified Users"))
                    || user.Username.ToLower() != context.User.Identity.Name.ToLower())
                {
                    var portalSecurity = new PortalSecurity();
                    portalSecurity.SignOut();

                    //Remove user from cache
                    if (user != null)
                    {
                        DataCache.ClearUserCache(portalSettings.PortalId, context.User.Identity.Name);
                    }

                    //Redirect browser back to home page
                    response.Redirect(request.RawUrl, true);
                    return;
                }

                if (!user.IsSuperUser && user.IsInRole("Unverified Users") && !HttpContext.Current.Items.Contains(DotNetNuke.UI.Skins.Skin.OnInitMessage))
                {
					HttpContext.Current.Items.Add(DotNetNuke.UI.Skins.Skin.OnInitMessage, Localization.GetString("UnverifiedUser", Localization.SharedResourceFile, CurrentCulture));
                }

				if (!user.IsSuperUser && HttpContext.Current.Request.QueryString.AllKeys.Contains("VerificationSuccess") && !HttpContext.Current.Items.Contains(DotNetNuke.UI.Skins.Skin.OnInitMessage))
				{
					HttpContext.Current.Items.Add(DotNetNuke.UI.Skins.Skin.OnInitMessage, Localization.GetString("VerificationSuccess", Localization.SharedResourceFile, CurrentCulture));
					HttpContext.Current.Items.Add(DotNetNuke.UI.Skins.Skin.OnInitMessageType, ModuleMessage.ModuleMessageType.GreenSuccess);
				}

                //if users LastActivityDate is outside of the UsersOnlineTimeWindow then record user activity
                if (DateTime.Compare(user.Membership.LastActivityDate.AddMinutes(Host.UsersOnlineTimeWindow), DateTime.Now) < 0)
                {
                    //update LastActivityDate and IP Address for user
                    user.Membership.LastActivityDate = DateTime.Now;
                    user.LastIPAddress = request.UserHostAddress;
                    UserController.UpdateUser(portalSettings.PortalId, user, false, false);
                }

                //check for RSVP code
                if (request.QueryString["rsvp"] != null && !string.IsNullOrEmpty(request.QueryString["rsvp"]))
                {
                    foreach (var role in TestableRoleController.Instance.GetRoles(portalSettings.PortalId, r => (r.SecurityMode != SecurityMode.SocialGroup || r.IsPublic) && r.Status == RoleStatus.Approved))
                    {
                        if (role.RSVPCode == request.QueryString["rsvp"])
                        {
                            roleController.UpdateUserRole(portalSettings.PortalId, user.UserID, role.RoleID);
                        }
                    }
                }

                //save userinfo object in context
                context.Items.Add("UserInfo", user);

                //Localization.SetLanguage also updates the user profile, so this needs to go after the profile is loaded
                Localization.SetLanguage(user.Profile.PreferredLocale);
            }

            if (context.Items["UserInfo"] == null)
            {
                context.Items.Add("UserInfo", new UserInfo());
            }
        }
    }
}