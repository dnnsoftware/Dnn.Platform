// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.ViewProfile
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Users.Social;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Social.Notifications;
    using DotNetNuke.Services.Tokens;
    using DotNetNuke.UI.Modules;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    ///   The ViewProfile ProfileModuleUserControlBase is used to view a Users Profile.
    /// </summary>
    public partial class ViewProfile : ProfileModuleUserControlBase
    {
        private readonly INavigationManager _navigationManager;

        public ViewProfile()
        {
            this._navigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        public override bool DisplayModule
        {
            get
            {
                return true;
            }
        }

        public bool IncludeButton
        {
            get
            {
                var includeButton = true;
                if (this.ModuleContext.Settings.ContainsKey("IncludeButton"))
                {
                    includeButton = Convert.ToBoolean(this.ModuleContext.Settings["IncludeButton"]);
                }

                return includeButton;
            }
        }

        public string ProfileProperties { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // throw 404 so that deleted profile is not reindexed
            if (this.ProfileUser == null || this.ProfileUser.IsDeleted)
            {
                UrlUtils.Handle404Exception(this.Response, PortalSettings.Current);
            }

            this.ProcessQuerystring();

            JavaScript.RequestRegistration(CommonJs.jQuery);
            JavaScript.RequestRegistration(CommonJs.jQueryMigrate);
            JavaScript.RequestRegistration(CommonJs.Knockout);
        }

        /// <summary>
        ///   Page_Load runs when the control is loaded.
        /// </summary>
        /// <remarks>
        /// </remarks>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                if (Null.IsNull(this.ProfileUserId))
                {
                    this.Visible = false;
                    return;
                }

                var template = Convert.ToString(this.ModuleContext.Settings["ProfileTemplate"]);
                if (string.IsNullOrEmpty(template))
                {
                    template = Localization.GetString("DefaultTemplate", this.LocalResourceFile);
                }

                var editUrl = this._navigationManager.NavigateURL(this.ModuleContext.PortalSettings.ActiveTab.TabID, "Profile", "userId=" + this.ProfileUserId, "pageno=1");
                var profileUrl = this._navigationManager.NavigateURL(this.ModuleContext.PortalSettings.ActiveTab.TabID, "Profile", "userId=" + this.ProfileUserId, "pageno=2");

                if (template.Contains("[BUTTON:EDITPROFILE]"))
                {
                    if (this.IncludeButton && this.IsUser)
                    {
                        string editHyperLink = string.Format("<a href=\"{0}\" class=\"dnnPrimaryAction\">{1}</a>", profileUrl, this.LocalizeString("Edit"));
                        template = template.Replace("[BUTTON:EDITPROFILE]", editHyperLink);
                    }

                    this.buttonPanel.Visible = false;
                }
                else
                {
                    this.buttonPanel.Visible = this.IncludeButton;
                    this.editLink.NavigateUrl = editUrl;
                }

                if (template.Contains("[HYPERLINK:EDITPROFILE]"))
                {
                    if (this.IsUser)
                    {
                        string editHyperLink = string.Format("<a href=\"{0}\" class=\"dnnSecondaryAction\">{1}</a>", profileUrl, this.LocalizeString("Edit"));
                        template = template.Replace("[HYPERLINK:EDITPROFILE]", editHyperLink);
                    }
                }

                if (template.Contains("[HYPERLINK:MYACCOUNT]"))
                {
                    if (this.IsUser)
                    {
                        string editHyperLink = string.Format("<a href=\"{0}\" class=\"dnnSecondaryAction\">{1}</a>", editUrl, this.LocalizeString("MyAccount"));
                        template = template.Replace("[HYPERLINK:MYACCOUNT]", editHyperLink);
                    }

                    this.buttonPanel.Visible = false;
                }

                if (!this.IsUser && this.buttonPanel.Visible)
                {
                    this.buttonPanel.Visible = false;
                }

                if (this.ProfileUser.Profile.ProfileProperties.Cast<ProfilePropertyDefinition>().Count(profProperty => profProperty.Visible) == 0)
                {
                    this.noPropertiesLabel.Visible = true;
                    this.profileOutput.Visible = false;
                    this.pnlScripts.Visible = false;
                }
                else
                {
                    if (template.IndexOf("[PROFILE:PHOTO]") > -1)
                    {
                        var profileImageHandlerBasedURL =
                            UserController.Instance?.GetUserProfilePictureUrl(this.ProfileUserId, 120, 120);
                        template = template.Replace("[PROFILE:PHOTO]", profileImageHandlerBasedURL);
                    }

                    var token = new TokenReplace { User = this.ProfileUser, AccessingUser = this.ModuleContext.PortalSettings.UserInfo };
                    this.profileOutput.InnerHtml = token.ReplaceEnvironmentTokens(template);
                    this.noPropertiesLabel.Visible = false;
                    this.profileOutput.Visible = true;
                }

                var propertyAccess = new ProfilePropertyAccess(this.ProfileUser);
                StringBuilder sb = new StringBuilder();
                bool propertyNotFound = false;

                foreach (ProfilePropertyDefinition property in this.ProfileUser.Profile.ProfileProperties)
                {
                    var displayDataType = ProfilePropertyAccess.DisplayDataType(property).ToLowerInvariant();
                    string value = propertyAccess.GetProperty(
                        property.PropertyName,
                        string.Empty,
                        Thread.CurrentThread.CurrentUICulture,
                        this.ModuleContext.PortalSettings.UserInfo,
                        Scope.DefaultSettings,
                        ref propertyNotFound);

                    var clientName = Localization.GetSafeJSString(property.PropertyName);
                    sb.Append("self['" + clientName + "'] = ko.observable(");
                    sb.Append("\"");
                    if (!string.IsNullOrEmpty(value))
                    {
                        value = Localization.GetSafeJSString(displayDataType == "richtext" ? value : this.Server.HtmlDecode(value));
                        value = value
                            .Replace("\r", string.Empty)
                            .Replace("\n", " ")
                            .Replace(";", string.Empty)
                            .Replace("://", ":||") // protect http protocols won't be replaced in next step
                            .Replace("//", string.Empty)
                            .Replace(":||", "://"); // restore http protocols
                    }

                    sb.Append(value + "\"" + ");");
                    sb.Append('\n');
                    sb.Append("self['" + clientName + "Text'] = '");
                    sb.Append(clientName + "';");
                    sb.Append('\n');
                }

                string email = (this.ProfileUserId == this.ModuleContext.PortalSettings.UserId
                                || this.ModuleContext.PortalSettings.UserInfo.IsInRole(this.ModuleContext.PortalSettings.AdministratorRoleName))
                                   ? this.ProfileUser.Email
                                   : string.Empty;

                sb.Append("self.Email = ko.observable('");
                email = Localization.GetSafeJSString(this.Server.HtmlDecode(email));
                email = email.Replace(";", string.Empty).Replace("//", string.Empty);
                sb.Append(email + "');");
                sb.Append('\n');
                sb.Append("self.EmailText = '");
                sb.Append(this.LocalizeString("Email") + "';");
                sb.Append('\n');

                this.ProfileProperties = sb.ToString();
            }
            catch (Exception exc)
            {
                // Module failed to load
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private string GetRedirectUrl()
        {
            // redirect user to default page if not specific the home tab, do this action to prevent loop redirect.
            var homeTabId = this.ModuleContext.PortalSettings.HomeTabId;
            string redirectUrl;

            if (homeTabId > Null.NullInteger)
            {
                redirectUrl = this._navigationManager.NavigateURL(homeTabId);
            }
            else
            {
                redirectUrl = Globals.GetPortalDomainName(PortalSettings.Current.PortalAlias.HTTPAlias, this.Request, true) +
                              "/" + Globals.glbDefaultPage;
            }

            return redirectUrl;
        }

        private void ProcessQuerystring()
        {
            // in case someone is being redirected to here from an e-mail link action we need to process that here
            var action = this.Request.QueryString["action"];

            if (!this.Request.IsAuthenticated && !string.IsNullOrEmpty(action)) // action requested but not logged in.
            {
                string loginUrl = Common.Globals.LoginURL(this.Request.RawUrl, false);
                this.Response.Redirect(loginUrl);
            }

            if (this.Request.IsAuthenticated && !string.IsNullOrEmpty(action)) // only process this for authenticated requests
            {
                // current user, i.e. the one that the request was for
                var currentUser = UserController.Instance.GetCurrentUserInfo();

                // the initiating user,i.e. the one who wanted to be friend
                // note that in this case here currentUser is visiting the profile of initiatingUser, most likely from a link in the notification e-mail
                var initiatingUser = UserController.Instance.GetUserById(PortalSettings.Current.PortalId, Convert.ToInt32(this.Request.QueryString["UserID"]));

                if (initiatingUser.UserID == currentUser.UserID)
                {
                    return; // do not further process for users who are on their own profile page
                }

                var friendRelationship = RelationshipController.Instance.GetFriendRelationship(currentUser, initiatingUser);

                if (friendRelationship != null)
                {
                    if (action.ToLowerInvariant() == "acceptfriend")
                    {
                        var friend = UserController.GetUserById(PortalSettings.Current.PortalId, friendRelationship.UserId);
                        FriendsController.Instance.AcceptFriend(friend);
                    }

                    if (action.ToLowerInvariant() == "followback")
                    {
                        var follower = UserController.GetUserById(PortalSettings.Current.PortalId, friendRelationship.UserId);
                        try
                        {
                            FollowersController.Instance.FollowUser(follower);
                            var notifications = NotificationsController.Instance.GetNotificationByContext(3, initiatingUser.UserID.ToString());
                            if (notifications.Count > 0)
                            {
                                NotificationsController.Instance.DeleteNotificationRecipient(notifications[0].NotificationID, currentUser.UserID);
                            }
                        }
                        catch
                        {
                            // ignore
                        }
                    }
                }

                this.Response.Redirect(Common.Globals.UserProfileURL(initiatingUser.UserID));
            }
        }
    }
}
