// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Users
{
    using System;
    using System.Collections;
    using System.Linq;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Utilities;
    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using DotNetNuke.Web.UI.WebControls;

    using DataCache = DotNetNuke.Common.Utilities.DataCache;
    using Globals = DotNetNuke.Common.Globals;
    using Host = DotNetNuke.Entities.Host.Host;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The User UserModuleBase is used to manage the base parts of a User.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public partial class User : UserUserControlBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(User));

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether gets whether the User is valid.
        /// </summary>
        public bool IsValid
        {
            get
            {
                return this.Validate();
            }
        }

        public UserCreateStatus CreateStatus { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets whether the Password section is displayed.
        /// </summary>
        public bool ShowPassword
        {
            get
            {
                return this.Password.Visible;
            }

            set
            {
                this.Password.Visible = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets whether the Update button.
        /// </summary>
        public bool ShowUpdate
        {
            get
            {
                return this.actionsRow.Visible;
            }

            set
            {
                this.actionsRow.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets user Form's css class.
        /// </summary>
        public string CssClass
        {
            get
            {
                return this.pnlAddUser.CssClass;
            }

            set
            {
                this.userForm.CssClass = string.IsNullOrEmpty(this.userForm.CssClass) ? value : string.Format("{0} {1}", this.userForm.CssClass, value);
                this.pnlAddUser.CssClass = string.IsNullOrEmpty(this.pnlAddUser.CssClass) ? value : string.Format("{0} {1}", this.pnlAddUser.CssClass, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// CreateUser creates a new user in the Database.
        /// </summary>
        public void CreateUser()
        {
            // Update DisplayName to conform to Format
            this.UpdateDisplayName();

            if (this.IsRegister)
            {
                this.User.Membership.Approved = this.PortalSettings.UserRegistration == (int)Globals.PortalRegistrationType.PublicRegistration;
            }
            else
            {
                // Set the Approved status from the value in the Authorized checkbox
                this.User.Membership.Approved = this.chkAuthorize.Checked;
            }

            var user = this.User;

            // make sure username is set in UseEmailAsUserName" mode
            if (PortalController.GetPortalSettingAsBoolean("Registration_UseEmailAsUserName", this.PortalId, false))
            {
                user.Username = this.User.Email;
                this.User.Username = this.User.Email;
            }

            var createStatus = UserController.CreateUser(ref user);

            var args = (createStatus == UserCreateStatus.Success)
                                            ? new UserCreatedEventArgs(this.User) { Notify = this.chkNotify.Checked }
                                            : new UserCreatedEventArgs(null);
            args.CreateStatus = createStatus;
            this.OnUserCreated(args);
            this.OnUserCreateCompleted(args);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DataBind binds the data to the controls.
        /// </summary>
        public override void DataBind()
        {
            if (this.Page.IsPostBack == false)
            {
                string confirmString = Localization.GetString("DeleteItem");
                if (this.IsUser)
                {
                    confirmString = Localization.GetString("ConfirmUnRegister", this.LocalResourceFile);
                }

                ClientAPI.AddButtonConfirm(this.cmdDelete, confirmString);
                this.chkRandom.Checked = false;
            }

            this.cmdDelete.Visible = false;
            this.cmdRemove.Visible = false;
            this.cmdRestore.Visible = false;
            if (!this.AddUser)
            {
                var deletePermitted = (this.User.UserID != this.PortalSettings.AdministratorId) && !(this.IsUser && this.User.IsSuperUser);
                if (deletePermitted)
                {
                    if (this.User.IsDeleted)
                    {
                        this.cmdRemove.Visible = true;
                        this.cmdRestore.Visible = true;
                    }
                    else
                    {
                        this.cmdDelete.Visible = true;
                    }
                }
            }

            this.cmdUpdate.Text = Localization.GetString(this.IsUser ? "Register" : "CreateUser", this.LocalResourceFile);
            this.cmdDelete.Text = Localization.GetString(this.IsUser ? "UnRegister" : "Delete", this.LocalResourceFile);
            if (this.AddUser)
            {
                this.pnlAddUser.Visible = true;
                if (this.IsRegister)
                {
                    this.AuthorizeNotify.Visible = false;
                    this.randomRow.Visible = false;
                    if (this.ShowPassword)
                    {
                        this.questionRow.Visible = MembershipProviderConfig.RequiresQuestionAndAnswer;
                        this.answerRow.Visible = MembershipProviderConfig.RequiresQuestionAndAnswer;
                        this.lblPasswordHelp.Text = Localization.GetString("PasswordHelpUser", this.LocalResourceFile);
                    }
                }
                else
                {
                    this.lblPasswordHelp.Text = Localization.GetString("PasswordHelpAdmin", this.LocalResourceFile);
                }

                this.txtConfirm.Attributes.Add("value", this.txtConfirm.Text);
                this.txtPassword.Attributes.Add("value", this.txtPassword.Text);
            }

            bool disableUsername = PortalController.GetPortalSettingAsBoolean("Registration_UseEmailAsUserName", this.PortalId, false);

            // only show username row once UseEmailAsUserName is disabled in site settings
            if (disableUsername)
            {
                this.userNameReadOnly.Visible = false;
                this.userName.Visible = false;
            }
            else
            {
                this.userNameReadOnly.Visible = !this.AddUser;
                this.userName.Visible = this.AddUser;
            }

            if (this.CanUpdateUsername() && !disableUsername)
            {
                this.renameUserName.Visible = true;

                this.userName.Visible = false;
                this.userNameReadOnly.Visible = false;

                ArrayList portals = PortalController.GetPortalsByUser(this.User.UserID);
                if (portals.Count > 1)
                {
                    this.numSites.Text = string.Format(Localization.GetString("UpdateUserName", this.LocalResourceFile), portals.Count.ToString());
                    this.cboSites.Visible = true;
                    this.cboSites.DataSource = portals;
                    this.cboSites.DataTextField = "PortalName";
                    this.cboSites.DataBind();

                    this.renameUserPortals.Visible = true;
                }
            }

            if (!string.IsNullOrEmpty(this.PortalSettings.Registration.UserNameValidator))
            {
                this.userName.ValidationExpression = this.PortalSettings.Registration.UserNameValidator;
            }

            if (!string.IsNullOrEmpty(this.PortalSettings.Registration.EmailValidator))
            {
                this.email.ValidationExpression = this.PortalSettings.Registration.EmailValidator;
            }

            if (!string.IsNullOrEmpty(this.PortalSettings.Registration.DisplayNameFormat))
            {
                if (this.AddUser)
                {
                    this.displayNameReadOnly.Visible = false;
                    this.displayName.Visible = false;
                }
                else
                {
                    this.displayNameReadOnly.Visible = true;
                    this.displayName.Visible = false;
                }

                this.firstName.Visible = true;
                this.lastName.Visible = true;
            }
            else
            {
                this.displayNameReadOnly.Visible = false;
                this.displayName.Visible = true;
                this.firstName.Visible = false;
                this.lastName.Visible = false;
            }

            this.userForm.DataSource = this.User;
            if (!this.Page.IsPostBack)
            {
                this.userForm.DataBind();
                this.renameUserName.Value = this.User.Username;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Load runs when the control is loaded.
        /// </summary>
        /// <remarks>
        /// </remarks>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.cmdDelete.Click += this.cmdDelete_Click;
            this.cmdUpdate.Click += this.cmdUpdate_Click;
            this.cmdRemove.Click += this.cmdRemove_Click;
            this.cmdRestore.Click += this.cmdRestore_Click;
        }

        protected override void OnPreRender(EventArgs e)
        {
            ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/scripts/dnn.jquery.extensions.js");
            ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/scripts/dnn.jquery.tooltip.js");
            ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/scripts/dnn.PasswordStrength.js");
            ClientResourceManager.RegisterScript(this.Page, "~/DesktopModules/Admin/Security/Scripts/dnn.PasswordComparer.js");

            ClientResourceManager.RegisterStyleSheet(this.Page, "~/Resources/Shared/stylesheets/dnn.PasswordStrength.css", FileOrder.Css.ResourceCss);

            JavaScript.RequestRegistration(CommonJs.DnnPlugins);

            base.OnPreRender(e);

            if (Host.EnableStrengthMeter)
            {
                this.passwordContainer.CssClass = "password-strength-container";
                this.txtPassword.CssClass = "password-strength";
                this.txtConfirm.CssClass = string.Format("{0} checkStength", this.txtConfirm.CssClass);

                var options = new DnnPaswordStrengthOptions();
                var optionsAsJsonString = Json.Serialize(options);
                var passwordScript = string.Format(
                    "dnn.initializePasswordStrength('.{0}', {1});{2}",
                    "password-strength", optionsAsJsonString, Environment.NewLine);

                if (ScriptManager.GetCurrent(this.Page) != null)
                {
                    // respect MS AJAX
                    ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "PasswordStrength", passwordScript, true);
                }
                else
                {
                    this.Page.ClientScript.RegisterStartupScript(this.GetType(), "PasswordStrength", passwordScript, true);
                }
            }

            var confirmPasswordOptions = new DnnConfirmPasswordOptions()
            {
                FirstElementSelector = "#" + this.passwordContainer.ClientID + " input[type=password]",
                SecondElementSelector = ".password-confirm",
                ContainerSelector = ".dnnFormPassword",
                UnmatchedCssClass = "unmatched",
                MatchedCssClass = "matched",
            };

            var confirmOptionsAsJsonString = Json.Serialize(confirmPasswordOptions);
            var confirmScript = string.Format("dnn.initializePasswordComparer({0});{1}", confirmOptionsAsJsonString, Environment.NewLine);

            if (ScriptManager.GetCurrent(this.Page) != null)
            {
                // respect MS AJAX
                ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "ConfirmPassword", confirmScript, true);
            }
            else
            {
                this.Page.ClientScript.RegisterStartupScript(this.GetType(), "ConfirmPassword", confirmScript, true);
            }
        }

        /// <summary>
        /// method checks to see if its allowed to change the username
        /// valid if a host, or an admin where the username is in only 1 portal.
        /// </summary>
        /// <returns></returns>
        private bool CanUpdateUsername()
        {
            // do not allow for non-logged in users
            if (this.Request.IsAuthenticated == false || this.AddUser)
            {
                return false;
            }

            // can only update username if a host/admin and account being managed is not a superuser
            if (UserController.Instance.GetCurrentUserInfo().IsSuperUser)
            {
                // only allow updates for non-superuser accounts
                if (this.User.IsSuperUser == false)
                {
                    return true;
                }
            }

            // if an admin, check if the user is only within this portal
            if (UserController.Instance.GetCurrentUserInfo().IsInRole(this.PortalSettings.AdministratorRoleName))
            {
                // only allow updates for non-superuser accounts
                if (this.User.IsSuperUser)
                {
                    return false;
                }

                if (PortalController.GetPortalsByUser(this.User.UserID).Count == 1)
                {
                    return true;
                }
            }

            return false;
        }

        private void UpdateDisplayName()
        {
            // Update DisplayName to conform to Format
            if (!string.IsNullOrEmpty(this.PortalSettings.Registration.DisplayNameFormat))
            {
                this.User.UpdateDisplayName(this.PortalSettings.Registration.DisplayNameFormat);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Validate validates the User.
        /// </summary>
        private bool Validate()
        {
            // Check User Editor
            bool _IsValid = this.userForm.IsValid;

            // Check Password is valid
            if (this.AddUser && this.ShowPassword)
            {
                this.CreateStatus = UserCreateStatus.AddUser;
                if (!this.chkRandom.Checked)
                {
                    // 1. Check Password is Valid
                    if (this.CreateStatus == UserCreateStatus.AddUser && !UserController.ValidatePassword(this.txtPassword.Text))
                    {
                        this.CreateStatus = UserCreateStatus.InvalidPassword;
                    }

                    if (this.CreateStatus == UserCreateStatus.AddUser)
                    {
                        this.User.Membership.Password = this.txtPassword.Text;
                    }
                }
                else
                {
                    // Generate a random password for the user
                    this.User.Membership.Password = UserController.GeneratePassword();
                }

                // Check Question/Answer
                if (this.CreateStatus == UserCreateStatus.AddUser && MembershipProviderConfig.RequiresQuestionAndAnswer)
                {
                    if (string.IsNullOrEmpty(this.txtQuestion.Text))
                    {
                        // Invalid Question
                        this.CreateStatus = UserCreateStatus.InvalidQuestion;
                    }
                    else
                    {
                        this.User.Membership.PasswordQuestion = this.txtQuestion.Text;
                    }

                    if (this.CreateStatus == UserCreateStatus.AddUser)
                    {
                        if (string.IsNullOrEmpty(this.txtAnswer.Text))
                        {
                            // Invalid Question
                            this.CreateStatus = UserCreateStatus.InvalidAnswer;
                        }
                        else
                        {
                            this.User.Membership.PasswordAnswer = this.txtAnswer.Text;
                        }
                    }
                }

                if (this.CreateStatus != UserCreateStatus.AddUser)
                {
                    _IsValid = false;
                }
            }

            return _IsValid;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdDelete_Click runs when the delete Button is clicked.
        /// </summary>
        private void cmdDelete_Click(object sender, EventArgs e)
        {
            if (this.IsUserOrAdmin == false)
            {
                return;
            }

            string name = this.User.Username;
            int id = this.UserId;
            UserInfo user = this.User;
            if (UserController.DeleteUser(ref user, true, false))
            {
                this.OnUserDeleted(new UserDeletedEventArgs(id, name));
            }
            else
            {
                this.OnUserDeleteError(new UserUpdateErrorArgs(id, name, "UserDeleteError"));
            }
        }

        private void cmdRestore_Click(object sender, EventArgs e)
        {
            if (this.IsUserOrAdmin == false)
            {
                return;
            }

            var name = this.User.Username;
            var id = this.UserId;

            var userInfo = this.User;
            if (UserController.RestoreUser(ref userInfo))
            {
                this.OnUserRestored(new UserRestoredEventArgs(id, name));
            }
            else
            {
                this.OnUserRestoreError(new UserUpdateErrorArgs(id, name, "UserRestoreError"));
            }
        }

        private void cmdRemove_Click(object sender, EventArgs e)
        {
            if (this.IsUserOrAdmin == false)
            {
                return;
            }

            var name = this.User.Username;
            var id = this.UserId;

            if (UserController.RemoveUser(this.User))
            {
                this.OnUserRemoved(new UserRemovedEventArgs(id, name));
            }
            else
            {
                this.OnUserRemoveError(new UserUpdateErrorArgs(id, name, "UserRemoveError"));
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdUpdate_Click runs when the Update Button is clicked.
        /// </summary>
        private void cmdUpdate_Click(object sender, EventArgs e)
        {
            if (this.IsUserOrAdmin == false)
            {
                return;
            }

            if (this.AddUser)
            {
                if (this.IsValid)
                {
                    this.CreateUser();
                    DataCache.ClearPortalUserCountCache(this.PortalId);
                }
            }
            else
            {
                if (this.userForm.IsValid && (this.User != null))
                {
                    if (this.User.UserID == this.PortalSettings.AdministratorId)
                    {
                        // Clear the Portal Cache
                        DataCache.ClearPortalUserCountCache(this.UserPortalID);
                    }

                    try
                    {
                        // Update DisplayName to conform to Format
                        this.UpdateDisplayName();

                        // either update the username or update the user details
                        if (this.CanUpdateUsername() && !this.PortalSettings.Registration.UseEmailAsUserName)
                        {
                            UserController.ChangeUsername(this.User.UserID, this.renameUserName.Value.ToString());
                        }

                        // DNN-5874 Check if unique display name is required
                        if (this.PortalSettings.Registration.RequireUniqueDisplayName)
                        {
                            var usersWithSameDisplayName = (System.Collections.Generic.List<UserInfo>)MembershipProvider.Instance().GetUsersBasicSearch(this.PortalId, 0, 2, "DisplayName", true, "DisplayName", this.User.DisplayName);
                            if (usersWithSameDisplayName.Any(user => user.UserID != this.User.UserID))
                            {
                                UI.Skins.Skin.AddModuleMessage(this, this.LocalizeString("DisplayNameNotUnique"), UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                                return;
                            }
                        }

                        UserController.UpdateUser(this.UserPortalID, this.User);

                        if (this.PortalSettings.Registration.UseEmailAsUserName && (this.User.Username.ToLower() != this.User.Email.ToLower()))
                        {
                            UserController.ChangeUsername(this.User.UserID, this.User.Email);
                        }

                        this.OnUserUpdated(EventArgs.Empty);
                        this.OnUserUpdateCompleted(EventArgs.Empty);
                    }
                    catch (Exception exc)
                    {
                        Logger.Error(exc);

                        var args = new UserUpdateErrorArgs(this.User.UserID, this.User.Username, "EmailError");
                        this.OnUserUpdateError(args);
                    }
                }
            }
        }
    }
}
