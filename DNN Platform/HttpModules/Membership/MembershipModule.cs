// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.Membership
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Security.Principal;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Security;

    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.HttpModules.Services;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Security.Roles.Internal;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.Services.UserRequest;
    using DotNetNuke.UI.Skins.Controls;
    using DotNetNuke.UI.Skins.EventListeners;

    /// <summary>
    /// Information about membership.
    /// </summary>
    public class MembershipModule : IHttpModule
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(MembershipModule));

        private static readonly Regex NameRegex = new Regex(@"\w+[\\]+(?=)", RegexOptions.Compiled);

        private static string _cultureCode;

        /// <summary>
        /// Gets the name of the module.
        /// </summary>
        /// <value>
        /// The name of the module: "DNNMembershipModule".
        /// </value>
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

        /// <summary>
        /// Called when unverified user skin initialize.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SkinEventArgs"/> instance containing the event data.</param>
        public static void OnUnverifiedUserSkinInit(object sender, SkinEventArgs e)
        {
            var strMessage = Localization.GetString("UnverifiedUser", Localization.SharedResourceFile, CurrentCulture);
            UI.Skins.Skin.AddPageMessage(e.Skin, string.Empty, strMessage, ModuleMessage.ModuleMessageType.YellowWarning);
        }

        /// <summary>
        /// Authenticates the request.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="allowUnknownExtensions">if set to <c>true</c> to allow unknown extensinons.</param>
        public static void AuthenticateRequest(HttpContextBase context, bool allowUnknownExtensions)
        {
            HttpRequestBase request = context.Request;
            HttpResponseBase response = context.Response;

            // First check if we are upgrading/installing
            if (!Initialize.ProcessHttpModule(context.ApplicationInstance.Request, allowUnknownExtensions, false))
            {
                return;
            }

            // Obtain PortalSettings from Current Context
            PortalSettings portalSettings = PortalController.Instance.GetCurrentPortalSettings();

            bool isActiveDirectoryAuthHeaderPresent = false;
            var auth = request.Headers.Get("Authorization");
            if (!string.IsNullOrEmpty(auth))
            {
                if (auth.StartsWith("Negotiate"))
                {
                    isActiveDirectoryAuthHeaderPresent = true;
                }
            }

            if (request.IsAuthenticated && !isActiveDirectoryAuthHeaderPresent && portalSettings != null)
            {
                var user = UserController.GetCachedUser(portalSettings.PortalId, context.User.Identity.Name);

                // if current login is from windows authentication, the ignore the process
                if (user == null && context.User is WindowsPrincipal)
                {
                    return;
                }

                // authenticate user and set last login ( this is necessary for users who have a permanent Auth cookie set )
                if (RequireLogout(context, user))
                {
                    var portalSecurity = PortalSecurity.Instance;
                    portalSecurity.SignOut();

                    // Remove user from cache
                    if (user != null)
                    {
                        DataCache.ClearUserCache(portalSettings.PortalId, context.User.Identity.Name);
                    }

                    // Redirect browser back to home page
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

                // if users LastActivityDate is outside of the UsersOnlineTimeWindow then record user activity
                if (DateTime.Compare(user.Membership.LastActivityDate.AddMinutes(Host.UsersOnlineTimeWindow), DateTime.Now) < 0)
                {
                    // update LastActivityDate and IP Address for user
                    user.Membership.LastActivityDate = DateTime.Now;
                    user.LastIPAddress = UserRequestIPAddressController.Instance.GetUserRequestIPAddress(request);
                    UserController.UpdateUser(portalSettings.PortalId, user, false, false);
                }

                // check for RSVP code
                if (request.QueryString["rsvp"] != null && !string.IsNullOrEmpty(request.QueryString["rsvp"]))
                {
                    foreach (var role in RoleController.Instance.GetRoles(portalSettings.PortalId, r => (r.SecurityMode != SecurityMode.SocialGroup || r.IsPublic) && r.Status == RoleStatus.Approved))
                    {
                        if (role.RSVPCode == request.QueryString["rsvp"])
                        {
                            RoleController.Instance.UpdateUserRole(portalSettings.PortalId, user.UserID, role.RoleID, RoleStatus.Approved, false, false);
                        }
                    }
                }

                // save userinfo object in context
                if (context.Items["UserInfo"] != null)
                {
                    context.Items["UserInfo"] = user;
                }
                else
                {
                    context.Items.Add("UserInfo", user);
                }

                // Localization.SetLanguage also updates the user profile, so this needs to go after the profile is loaded
                if (request.RawUrl != null && !ServicesModule.ServiceApi.IsMatch(request.RawUrl))
                {
                    Localization.SetLanguage(user.Profile.PreferredLocale);
                }
            }

            if (context.Items["UserInfo"] == null)
            {
                context.Items.Add("UserInfo", new UserInfo());
            }
        }

        /// <summary>
        /// Initializes the specified application.
        /// </summary>
        /// <param name="application">The application.</param>
        public void Init(HttpApplication application)
        {
            application.AuthenticateRequest += this.OnAuthenticateRequest;
            application.PreSendRequestHeaders += this.OnPreSendRequestHeaders;
        }

        /// <summary>
        /// Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule" />.
        /// </summary>
        public void Dispose()
        {
        }

        private static bool RequireLogout(HttpContextBase context, UserInfo user)
        {
            try
            {
                if (user == null || user.IsDeleted || user.Membership.LockedOut
                    || (!user.Membership.Approved && !user.IsInRole("Unverified Users"))
                    || !user.Username.Equals(context.User.Identity.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }

                var forceLogout = HostController.Instance.GetBoolean("ForceLogoutAfterPasswordChanged");
                if (!forceLogout)
                {
                    return false;
                }

                // if user's password changed after the user cookie created, then force user to login again.
                DateTime? issueDate = null;
                if (context.User.Identity is FormsIdentity formsIdentity)
                {
                    issueDate = formsIdentity.Ticket.IssueDate;
                }

                return !Null.IsNull(issueDate) && issueDate < user.Membership.LastPasswordChangeDate;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return true;
            }
        }

        private void OnAuthenticateRequest(object sender, EventArgs e)
        {
            var application = (HttpApplication)sender;
            AuthenticateRequest(new HttpContextWrapper(application.Context), false);
        }

        // DNN-6973: if the authentication cookie set by cookie slide in membership,
        // then use SignIn method instead if current portal is in portal group.
        private void OnPreSendRequestHeaders(object sender, EventArgs e)
        {
            var application = (HttpApplication)sender;

            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            var hasAuthCookie = application.Response.Headers["Set-Cookie"] != null
                                    && application.Response.Headers["Set-Cookie"].Contains(FormsAuthentication.FormsCookieName);
            if (portalSettings != null && hasAuthCookie && !application.Context.Items.Contains("DNN_UserSignIn"))
            {
                var isInPortalGroup = PortalController.IsMemberOfPortalGroup(portalSettings.PortalId);
                if (isInPortalGroup)
                {
                    var authCookie = application.Response.Cookies[FormsAuthentication.FormsCookieName];
                    if (authCookie != null && !string.IsNullOrEmpty(authCookie.Value) && string.IsNullOrEmpty(authCookie.Domain))
                    {
                        application.Response.Cookies.Remove(FormsAuthentication.FormsCookieName);
                        PortalSecurity.Instance.SignIn(UserController.Instance.GetCurrentUserInfo(), false);
                    }
                }
            }
        }
    }
}
