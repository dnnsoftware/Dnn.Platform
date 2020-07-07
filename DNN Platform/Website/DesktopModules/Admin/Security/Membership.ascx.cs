// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Users
{
    using System;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Mail;
    using DotNetNuke.UI.Skins.Controls;
    using Microsoft.Extensions.DependencyInjection;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Modules.Admin.Users
    /// Class:      Membership
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The Membership UserModuleBase is used to manage the membership aspects of a
    /// User.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class Membership : UserModuleBase
    {
        private readonly INavigationManager _navigationManager;

        public Membership()
        {
            this._navigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Raises the MembershipAuthorized Event
        /// </summary>
        /// -----------------------------------------------------------------------------
        public event EventHandler MembershipAuthorized;

        public event EventHandler MembershipPasswordUpdateChanged;

        public event EventHandler MembershipUnAuthorized;

        public event EventHandler MembershipUnLocked;

        public event EventHandler MembershipPromoteToSuperuser;

        public event EventHandler MembershipDemoteFromSuperuser;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the UserMembership associated with this control.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public UserMembership UserMembership
        {
            get
            {
                UserMembership membership = null;
                if (this.User != null)
                {
                    membership = this.User.Membership;
                }

                return membership;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Raises the MembershipPromoteToSuperuser Event.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void OnMembershipPromoteToSuperuser(EventArgs e)
        {
            if (this.IsUserOrAdmin == false)
            {
                return;
            }

            if (this.MembershipPromoteToSuperuser != null)
            {
                this.MembershipPromoteToSuperuser(this, e);
                this.Response.Redirect(this._navigationManager.NavigateURL(), true);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Raises the MembershipPromoteToSuperuser Event.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void OnMembershipDemoteFromSuperuser(EventArgs e)
        {
            if (this.IsUserOrAdmin == false)
            {
                return;
            }

            if (this.MembershipDemoteFromSuperuser != null)
            {
                this.MembershipDemoteFromSuperuser(this, e);
                this.Response.Redirect(this._navigationManager.NavigateURL(), true);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Raises the MembershipAuthorized Event.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void OnMembershipAuthorized(EventArgs e)
        {
            if (this.IsUserOrAdmin == false)
            {
                return;
            }

            if (this.MembershipAuthorized != null)
            {
                this.MembershipAuthorized(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Raises the MembershipPasswordUpdateChanged Event.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void OnMembershipPasswordUpdateChanged(EventArgs e)
        {
            if (this.IsUserOrAdmin == false)
            {
                return;
            }

            if (this.MembershipPasswordUpdateChanged != null)
            {
                this.MembershipPasswordUpdateChanged(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Raises the MembershipUnAuthorized Event.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void OnMembershipUnAuthorized(EventArgs e)
        {
            if (this.IsUserOrAdmin == false)
            {
                return;
            }

            if (this.MembershipUnAuthorized != null)
            {
                this.MembershipUnAuthorized(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Raises the MembershipUnLocked Event.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void OnMembershipUnLocked(EventArgs e)
        {
            if (this.IsUserOrAdmin == false)
            {
                return;
            }

            if (this.MembershipUnLocked != null)
            {
                this.MembershipUnLocked(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DataBind binds the data to the controls.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void DataBind()
        {
            // disable/enable buttons
            if (this.UserInfo.UserID == this.User.UserID)
            {
                this.cmdAuthorize.Visible = false;
                this.cmdUnAuthorize.Visible = false;
                this.cmdUnLock.Visible = false;
                this.cmdPassword.Visible = false;
            }
            else
            {
                this.cmdUnLock.Visible = this.UserMembership.LockedOut;
                this.cmdUnAuthorize.Visible = this.UserMembership.Approved && !this.User.IsInRole("Unverified Users");
                this.cmdAuthorize.Visible = !this.UserMembership.Approved || this.User.IsInRole("Unverified Users");
                this.cmdPassword.Visible = !this.UserMembership.UpdatePassword;
            }

            if (UserController.Instance.GetCurrentUserInfo().IsSuperUser && UserController.Instance.GetCurrentUserInfo().UserID != this.User.UserID)
            {
                this.cmdToggleSuperuser.Visible = true;

                if (this.User.IsSuperUser)
                {
                    this.cmdToggleSuperuser.Text = Localization.GetString("DemoteFromSuperUser", this.LocalResourceFile);
                }
                else
                {
                    this.cmdToggleSuperuser.Text = Localization.GetString("PromoteToSuperUser", this.LocalResourceFile);
                }

                if (PortalController.GetPortalsByUser(this.User.UserID).Count == 0)
                {
                    this.cmdToggleSuperuser.Visible = false;
                }
            }

            this.lastLockoutDate.Value = this.UserMembership.LastLockoutDate.Year > 2000
                                        ? (object)this.UserMembership.LastLockoutDate
                                        : this.LocalizeString("Never");

            // ReSharper disable SpecifyACultureInStringConversionExplicitly
            this.isOnLine.Value = this.LocalizeString(this.UserMembership.IsOnLine.ToString());
            this.lockedOut.Value = this.LocalizeString(this.UserMembership.LockedOut.ToString());
            this.approved.Value = this.LocalizeString(this.UserMembership.Approved.ToString());
            this.updatePassword.Value = this.LocalizeString(this.UserMembership.UpdatePassword.ToString());
            this.isDeleted.Value = this.LocalizeString(this.UserMembership.IsDeleted.ToString());

            // show the user folder path without default parent folder, and only visible to admin.
            this.userFolder.Visible = this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName);
            if (this.userFolder.Visible)
            {
                this.userFolder.Value = FolderManager.Instance.GetUserFolder(this.User).FolderPath.Substring(6);
            }

            // ReSharper restore SpecifyACultureInStringConversionExplicitly
            this.membershipForm.DataSource = this.UserMembership;
            this.membershipForm.DataBind();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Load runs when the control is loaded.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.cmdAuthorize.Click += this.cmdAuthorize_Click;
            this.cmdPassword.Click += this.cmdPassword_Click;
            this.cmdUnAuthorize.Click += this.cmdUnAuthorize_Click;
            this.cmdUnLock.Click += this.cmdUnLock_Click;
            this.cmdToggleSuperuser.Click += this.cmdToggleSuperuser_Click;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdAuthorize_Click runs when the Authorize User Button is clicked.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void cmdAuthorize_Click(object sender, EventArgs e)
        {
            if (this.IsUserOrAdmin == false)
            {
                return;
            }

            if (this.Request.IsAuthenticated != true)
            {
                return;
            }

            // Get the Membership Information from the property editors
            this.User.Membership = (UserMembership)this.membershipForm.DataSource;

            this.User.Membership.Approved = true;

            // Update User
            UserController.UpdateUser(this.PortalId, this.User);

            // Update User Roles if needed
            if (!this.User.IsSuperUser && this.User.IsInRole("Unverified Users") && this.PortalSettings.UserRegistration == (int)Common.Globals.PortalRegistrationType.VerifiedRegistration)
            {
                UserController.ApproveUser(this.User);
            }

            Mail.SendMail(this.User, MessageType.UserAuthorized, this.PortalSettings);

            this.OnMembershipAuthorized(EventArgs.Empty);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdPassword_Click runs when the ChangePassword Button is clicked.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void cmdPassword_Click(object sender, EventArgs e)
        {
            if (this.IsUserOrAdmin == false)
            {
                return;
            }

            if (this.Request.IsAuthenticated != true)
            {
                return;
            }

            bool canSend = Mail.SendMail(this.User, MessageType.PasswordReminder, this.PortalSettings) == string.Empty;
            var message = string.Empty;
            if (canSend)
            {
                // Get the Membership Information from the property editors
                this.User.Membership = (UserMembership)this.membershipForm.DataSource;

                this.User.Membership.UpdatePassword = true;

                // Update User
                UserController.UpdateUser(this.PortalId, this.User);

                this.OnMembershipPasswordUpdateChanged(EventArgs.Empty);
            }
            else
            {
                message = Localization.GetString("OptionUnavailable", this.LocalResourceFile);
                UI.Skins.Skin.AddModuleMessage(this, message, ModuleMessage.ModuleMessageType.YellowWarning);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdUnAuthorize_Click runs when the UnAuthorize User Button is clicked.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void cmdUnAuthorize_Click(object sender, EventArgs e)
        {
            if (this.IsUserOrAdmin == false)
            {
                return;
            }

            if (this.Request.IsAuthenticated != true)
            {
                return;
            }

            // Get the Membership Information from the property editors
            this.User.Membership = (UserMembership)this.membershipForm.DataSource;

            this.User.Membership.Approved = false;

            // Update User
            UserController.UpdateUser(this.PortalId, this.User);

            this.OnMembershipUnAuthorized(EventArgs.Empty);
        }

        /// <summary>
        /// cmdToggleSuperuser_Click runs when the toggle superuser button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdToggleSuperuser_Click(object sender, EventArgs e)
        {
            if (this.IsUserOrAdmin == false)
            {
                return;
            }

            if (this.Request.IsAuthenticated != true)
            {
                return;
            }
            ////ensure only superusers can change user superuser state
            if (UserController.Instance.GetCurrentUserInfo().IsSuperUser != true)
            {
                return;
            }

            var currentSuperUserState = this.User.IsSuperUser;
            this.User.IsSuperUser = !currentSuperUserState;

            // Update User
            UserController.UpdateUser(this.PortalId, this.User);
            DataCache.ClearCache();

            if (currentSuperUserState)
            {
                this.OnMembershipDemoteFromSuperuser(EventArgs.Empty);
            }
            else
            {
                this.OnMembershipPromoteToSuperuser(EventArgs.Empty);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdUnlock_Click runs when the Unlock Account Button is clicked.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void cmdUnLock_Click(object sender, EventArgs e)
        {
            if (this.IsUserOrAdmin == false)
            {
                return;
            }

            if (this.Request.IsAuthenticated != true)
            {
                return;
            }

            // update the user record in the database
            bool isUnLocked = UserController.UnLockUser(this.User);

            if (isUnLocked)
            {
                this.User.Membership.LockedOut = false;

                this.OnMembershipUnLocked(EventArgs.Empty);
            }
        }
    }
}
