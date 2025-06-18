// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.Membership
{
    using System;
    using System.Linq;
    using System.Security.Principal;
    using System.Threading;
    using System.Web;
    using System.Web.Security;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.HttpModules.Services;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.UserRequest;
    using DotNetNuke.UI.Skins;
    using DotNetNuke.UI.Skins.Controls;
    using DotNetNuke.UI.Skins.EventListeners;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Information about membership.</summary>
    public class MembershipModule : IHttpModule
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(MembershipModule));
        private readonly IHostSettingsService hostSettingsService;
        private readonly IPortalController portalController;
        private readonly IUserRequestIPAddressController ipAddressController;
        private readonly IRoleController roleController;
        private readonly IUserController userController;

        /// <summary>Initializes a new instance of the <see cref="MembershipModule"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IHostSettingsService. Scheduled removal in v12.0.0.")]
        public MembershipModule()
            : this(null, null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="MembershipModule"/> class.</summary>
        /// <param name="hostSettingsService">The host settings service.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="ipAddressController">The IP address controller.</param>
        /// <param name="roleController">The role controller.</param>
        /// <param name="userController">The user controller.</param>
        public MembershipModule(IHostSettingsService hostSettingsService, IPortalController portalController, IUserRequestIPAddressController ipAddressController, IRoleController roleController, IUserController userController)
        {
            this.hostSettingsService = hostSettingsService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettingsService>();
            this.portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
            this.ipAddressController = ipAddressController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IUserRequestIPAddressController>();
            this.roleController = roleController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IRoleController>();
            this.userController = userController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IUserController>();
        }

        /// <summary>Gets the name of the module.</summary>
        public string ModuleName => "DNNMembershipModule";

        /// <summary>Called when unverified user skin initialize.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SkinEventArgs"/> instance containing the event data.</param>
        public static void OnUnverifiedUserSkinInit(object sender, SkinEventArgs e)
        {
            var strMessage = Localization.GetString("UnverifiedUser", Localization.SharedResourceFile, Thread.CurrentThread.CurrentCulture.Name);
            UI.Skins.Skin.AddPageMessage(e.Skin, string.Empty, strMessage, ModuleMessage.ModuleMessageType.YellowWarning);
        }

        /// <summary>Authenticates the request.</summary>
        /// <param name="context">The context.</param>
        /// <param name="allowUnknownExtensions">if set to <see langword="true"/> to allow unknown extensions.</param>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IHostSettingsService. Scheduled removal in v12.0.0.")]
        public static void AuthenticateRequest(HttpContextBase context, bool allowUnknownExtensions)
            => AuthenticateRequest(
                Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettingsService>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IUserRequestIPAddressController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IRoleController>(),
                context,
                allowUnknownExtensions);

        /// <summary>Authenticates the request.</summary>
        /// <param name="hostSettingsService">The host settings service.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="ipAddressController">The user request IP address controller.</param>
        /// <param name="roleController">The role controller.</param>
        /// <param name="context">The context.</param>
        /// <param name="allowUnknownExtensions">if set to <c>true</c> to allow unknown extensions.</param>
        public static void AuthenticateRequest(IHostSettingsService hostSettingsService, IPortalController portalController, IUserRequestIPAddressController ipAddressController, IRoleController roleController, HttpContextBase context, bool allowUnknownExtensions)
        {
            HttpRequestBase request = context.Request;
            HttpResponseBase response = context.Response;

            // First check if we are upgrading/installing
            if (!Initialize.ProcessHttpModule(context.ApplicationInstance.Request, allowUnknownExtensions, false))
            {
                return;
            }

            // Obtain PortalSettings from Current Context
            var portalSettings = portalController.GetCurrentSettings();

            if (request.IsAuthenticated && !IsActiveDirectoryAuthHeaderPresent() && portalSettings != null)
            {
                var user = UserController.GetCachedUser(portalSettings.PortalId, context.User.Identity.Name);

                // if current login is from windows authentication, the ignore the process
                if (user == null && context.User is WindowsPrincipal)
                {
                    return;
                }

                // authenticate user and set last login ( this is necessary for users who have a permanent Auth cookie set )
                if (RequireLogout(hostSettingsService, context, user))
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

                // if users LastActivityDate is outside the UsersOnlineTimeWindow then record user activity
    #pragma warning disable CS0618 // Type or member is obsolete
                var usersOnlineTimeWindow = Host.UsersOnlineTimeWindow;
    #pragma warning restore CS0618 // Type or member is obsolete
                if (user != null && DateTime.Compare(user.Membership.LastActivityDate.AddMinutes(usersOnlineTimeWindow), DateTime.Now) < 0)
                {
                    // update LastActivityDate and IP Address for user
                    user.Membership.LastActivityDate = DateTime.Now;
                    user.LastIPAddress = ipAddressController.GetUserRequestIPAddress(request);
                    UserController.UpdateUser(portalSettings.PortalId, user, false, false);
                }

                // check for RSVP code
                if (user != null && request.QueryString["rsvp"] != null && !string.IsNullOrEmpty(request.QueryString["rsvp"]))
                {
                    foreach (var role in roleController.GetRoles(portalSettings.PortalId, r => (r.SecurityMode != SecurityMode.SocialGroup || r.IsPublic) && r.Status == RoleStatus.Approved))
                    {
                        if (role.RSVPCode == request.QueryString["rsvp"])
                        {
                            roleController.UpdateUserRole(portalSettings.PortalId, user.UserID, role.RoleID, RoleStatus.Approved, false, false);
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
                if (user != null && request.RawUrl != null && !ServicesModule.ServiceApi.IsMatch(request.RawUrl))
                {
                    Localization.SetLanguage(user.Profile.PreferredLocale);
                }
            }

            if (context.Items["UserInfo"] == null)
            {
                context.Items.Add("UserInfo", new UserInfo());
            }
        }

        /// <summary>Initializes the specified application.</summary>
        /// <param name="application">The application.</param>
        public void Init(HttpApplication application)
        {
            application.AuthenticateRequest += this.OnAuthenticateRequest;
            application.PreRequestHandlerExecute += this.Context_PreRequestHandlerExecute;
            application.PreSendRequestHeaders += this.OnPreSendRequestHeaders;
        }

        /// <summary>Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule" />.</summary>
        public void Dispose()
        {
        }

        private static bool IsActiveDirectoryAuthHeaderPresent()
        {
            var auth = HttpContext.Current.Request.Headers.Get("Authorization");
            if (!string.IsNullOrEmpty(auth))
            {
                if (auth.StartsWith("Negotiate"))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool RequireLogout(IHostSettingsService hostSettingsService, HttpContextBase context, UserInfo user)
        {
            try
            {
                if (user == null || user.IsDeleted || user.Membership.LockedOut
                    || (!user.Membership.Approved && !user.IsInRole("Unverified Users"))
                    || !user.Username.Equals(context.User.Identity.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }

                var forceLogout = hostSettingsService.GetBoolean("ForceLogoutAfterPasswordChanged");
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
            AuthenticateRequest(this.hostSettingsService, this.portalController, this.ipAddressController, this.roleController, new HttpContextWrapper(application.Context), false);
        }

        // DNN-6973: if the authentication cookie set by cookie slide in membership,
        // then use SignIn method instead if current portal is in portal group.
        private void OnPreSendRequestHeaders(object sender, EventArgs e)
        {
            var application = (HttpApplication)sender;

            var portalSettings = this.portalController.GetCurrentSettings();
            var hasAuthCookie = application.Response.Headers["Set-Cookie"] != null
                                    && application.Response.Headers["Set-Cookie"].Contains(FormsAuthentication.FormsCookieName);
            if (portalSettings != null && hasAuthCookie && !application.Context.Items.Contains("DNN_UserSignIn"))
            {
                var isInPortalGroup = PortalController.IsMemberOfPortalGroup(this.portalController, portalSettings.PortalId);
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

        private void Context_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            var portalSettings = this.portalController.GetCurrentSettings();
            var request = HttpContext.Current.Request;
            var user = this.userController.GetCurrentUserInfo();
            if (!request.IsAuthenticated || IsActiveDirectoryAuthHeaderPresent() || portalSettings == null)
            {
                return;
            }

            var contextItems = HttpContext.Current.Items;
            if (!user.IsSuperUser && user.IsInRole("Unverified Users") && !contextItems.Contains(Skin.OnInitMessage))
            {
                contextItems.Add(Skin.OnInitMessage, Localization.GetString("UnverifiedUser", Localization.SharedResourceFile, Thread.CurrentThread.CurrentCulture.Name));
            }

            if (!user.IsSuperUser && request.QueryString.AllKeys.Contains("VerificationSuccess") && !contextItems.Contains(Skin.OnInitMessage))
            {
                contextItems.Add(Skin.OnInitMessage, Localization.GetString("VerificationSuccess", Localization.SharedResourceFile, Thread.CurrentThread.CurrentCulture.Name));
                contextItems.Add(Skin.OnInitMessageType, ModuleMessage.ModuleMessageType.GreenSuccess);
            }
        }
    }
}
