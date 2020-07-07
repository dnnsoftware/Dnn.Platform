// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Users
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Users.Membership;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins.Controls;
    using DotNetNuke.UI.WebControls;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using DotNetNuke.Web.UI.WebControls;
    using Microsoft.Extensions.DependencyInjection;

    using Host = DotNetNuke.Entities.Host.Host;

    public partial class Register : UserUserControlBase
    {
        protected const string PasswordStrengthTextBoxCssClass = "password-strength";
        protected const string ConfirmPasswordTextBoxCssClass = "password-confirm";

        private readonly List<AuthenticationLoginBase> _loginControls = new List<AuthenticationLoginBase>();
        private readonly INavigationManager _navigationManager;

        public Register()
        {
            this._navigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        protected string ExcludeTerms
        {
            get
            {
                string regex = string.Empty;
                if (!string.IsNullOrEmpty(this.PortalSettings.Registration.ExcludeTerms))
                {
                    regex = @"^(?:(?!" + this.PortalSettings.Registration.ExcludeTerms.Replace(" ", string.Empty).Replace(",", "|") + @").)*$\r?\n?";
                }

                return regex;
            }
        }

        protected bool IsValid
        {
            get
            {
                return this.Validate();
            }
        }

        protected override bool AddUser { get; } = true;

        protected string AuthenticationType
        {
            get
            {
                return this.ViewState.GetValue("AuthenticationType", Null.NullString);
            }

            set
            {
                this.ViewState.SetValue("AuthenticationType", value, Null.NullString);
            }
        }

        protected UserCreateStatus CreateStatus { get; set; }

        protected string UserToken
        {
            get
            {
                return this.ViewState.GetValue("UserToken", string.Empty);
            }

            set
            {
                this.ViewState.SetValue("UserToken", value, string.Empty);
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            JavaScript.RequestRegistration(CommonJs.DnnPlugins);

            ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/scripts/dnn.jquery.extensions.js");
            ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/scripts/dnn.jquery.tooltip.js");
            ClientResourceManager.RegisterScript(this.Page, "~/DesktopModules/Admin/Security/Scripts/dnn.PasswordComparer.js");

            if (this.PortalSettings.Registration.RegistrationFormType == 0)
            {
                // DisplayName
                if (string.IsNullOrEmpty(this.PortalSettings.Registration.DisplayNameFormat))
                {
                    this.AddField("DisplayName", string.Empty, true, string.Empty, TextBoxMode.SingleLine);
                }
                else
                {
                    this.AddField("FirstName", string.Empty, true, string.Empty, TextBoxMode.SingleLine);
                    this.AddField("LastName", string.Empty, true, string.Empty, TextBoxMode.SingleLine);
                }

                // Email
                this.AddField("Email", string.Empty, true, this.PortalSettings.Registration.EmailValidator, TextBoxMode.SingleLine);

                // UserName
                if (!this.PortalSettings.Registration.UseEmailAsUserName)
                {
                    this.AddField("Username", string.Empty, true,
                            string.IsNullOrEmpty(this.PortalSettings.Registration.UserNameValidator) ? this.ExcludeTerms : this.PortalSettings.Registration.UserNameValidator,
                            TextBoxMode.SingleLine);
                }

                // Password
                if (!this.PortalSettings.Registration.RandomPassword)
                {
                    this.AddPasswordStrengthField("Password", "Membership", true);

                    if (this.PortalSettings.Registration.RequirePasswordConfirm)
                    {
                        this.AddPasswordConfirmField("PasswordConfirm", "Membership", true);
                    }
                }

                // Password Q&A
                if (MembershipProviderConfig.RequiresQuestionAndAnswer)
                {
                    this.AddField("PasswordQuestion", "Membership", true, string.Empty, TextBoxMode.SingleLine);
                    this.AddField("PasswordAnswer", "Membership", true, string.Empty, TextBoxMode.SingleLine);
                }

                if (this.PortalSettings.Registration.RequireValidProfile)
                {
                    foreach (ProfilePropertyDefinition property in this.User.Profile.ProfileProperties)
                    {
                        if (property.Required)
                        {
                            this.AddProperty(property);
                        }
                    }
                }
            }
            else
            {
                var fields = this.PortalSettings.Registration.RegistrationFields.Split(',').ToList();

                // append question/answer field when RequiresQuestionAndAnswer is enabled in config.
                if (MembershipProviderConfig.RequiresQuestionAndAnswer)
                {
                    if (!fields.Contains("PasswordQuestion"))
                    {
                        fields.Add("PasswordQuestion");
                    }

                    if (!fields.Contains("PasswordAnswer"))
                    {
                        fields.Add("PasswordAnswer");
                    }
                }

                foreach (string field in fields)
                {
                    var trimmedField = field.Trim();
                    switch (trimmedField)
                    {
                        case "Username":
                            this.AddField("Username", string.Empty, true, string.IsNullOrEmpty(this.PortalSettings.Registration.UserNameValidator)
                                                                ? this.ExcludeTerms : this.PortalSettings.Registration.UserNameValidator,
                                                                        TextBoxMode.SingleLine);
                            break;
                        case "DisplayName":
                            this.AddField(trimmedField, string.Empty, true, this.ExcludeTerms, TextBoxMode.SingleLine);
                            break;
                        case "Email":
                            this.AddField("Email", string.Empty, true, this.PortalSettings.Registration.EmailValidator, TextBoxMode.SingleLine);
                            break;
                        case "Password":
                            this.AddPasswordStrengthField(trimmedField, "Membership", true);
                            break;
                        case "PasswordConfirm":
                            this.AddPasswordConfirmField(trimmedField, "Membership", true);
                            break;
                        case "PasswordQuestion":
                        case "PasswordAnswer":
                            this.AddField(trimmedField, "Membership", true, string.Empty, TextBoxMode.SingleLine);
                            break;
                        default:
                            ProfilePropertyDefinition property = this.User.Profile.GetProperty(trimmedField);
                            if (property != null)
                            {
                                this.AddProperty(property);
                            }

                            break;
                    }
                }
            }

            // Verify that the current user has access to this page
            if (this.PortalSettings.UserRegistration == (int)Globals.PortalRegistrationType.NoRegistration && this.Request.IsAuthenticated == false)
            {
                try
                {
                    this.Response.Redirect(this._navigationManager.NavigateURL("Access Denied"), true);
                }
                catch (ThreadAbortException)
                {
                    // do nothing here.
                }
            }

            this.cancelLink.NavigateUrl = this.closeLink.NavigateUrl = this.GetRedirectUrl(false);
            this.registerButton.Click += this.registerButton_Click;

            if (this.PortalSettings.Registration.UseAuthProviders)
            {
                List<AuthenticationInfo> authSystems = AuthenticationController.GetEnabledAuthenticationServices();
                foreach (AuthenticationInfo authSystem in authSystems)
                {
                    try
                    {
                        var authLoginControl = (AuthenticationLoginBase)this.LoadControl("~/" + authSystem.LoginControlSrc);
                        if (authSystem.AuthenticationType != "DNN")
                        {
                            this.BindLoginControl(authLoginControl, authSystem);

                            // Check if AuthSystem is Enabled
                            if (authLoginControl.Enabled && authLoginControl.SupportsRegistration)
                            {
                                authLoginControl.Mode = AuthMode.Register;

                                // Add Login Control to List
                                this._loginControls.Add(authLoginControl);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Exceptions.LogException(ex);
                    }
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (this.Request.IsAuthenticated)
            {
                // if a Login Page has not been specified for the portal
                if (Globals.IsAdminControl())
                {
                    // redirect to current page
                    this.Response.Redirect(this._navigationManager.NavigateURL(), true);
                }
                else // make module container invisible if user is not a page admin
                {
                    if (!TabPermissionController.CanAdminPage())
                    {
                        this.ContainerControl.Visible = false;
                    }
                }
            }

            if (this.PortalSettings.Registration.UseCaptcha)
            {
                this.captchaRow.Visible = true;
                this.ctlCaptcha.ErrorMessage = Localization.GetString("InvalidCaptcha", this.LocalResourceFile);
                this.ctlCaptcha.Text = Localization.GetString("CaptchaText", this.LocalResourceFile);
            }

            if (this.PortalSettings.Registration.UseAuthProviders && string.IsNullOrEmpty(this.AuthenticationType))
            {
                foreach (AuthenticationLoginBase authLoginControl in this._loginControls)
                {
                    this.socialLoginControls.Controls.Add(authLoginControl);
                }
            }

            // Display relevant message
            this.userHelpLabel.Text = Localization.GetSystemMessage(this.PortalSettings, "MESSAGE_REGISTRATION_INSTRUCTIONS");
            switch (this.PortalSettings.UserRegistration)
            {
                case (int)Globals.PortalRegistrationType.PrivateRegistration:
                    this.userHelpLabel.Text += Localization.GetString("PrivateMembership", Localization.SharedResourceFile);
                    break;
                case (int)Globals.PortalRegistrationType.PublicRegistration:
                    this.userHelpLabel.Text += Localization.GetString("PublicMembership", Localization.SharedResourceFile);
                    break;
                case (int)Globals.PortalRegistrationType.VerifiedRegistration:
                    this.userHelpLabel.Text += Localization.GetString("VerifiedMembership", Localization.SharedResourceFile);
                    break;
            }

            this.userHelpLabel.Text += Localization.GetString("Required", this.LocalResourceFile);
            this.userHelpLabel.Text += Localization.GetString("RegisterWarning", this.LocalResourceFile);

            this.userForm.DataSource = this.User;
            if (!this.Page.IsPostBack)
            {
                this.userForm.DataBind();
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            var confirmPasswordOptions = new DnnConfirmPasswordOptions()
            {
                FirstElementSelector = "." + PasswordStrengthTextBoxCssClass,
                SecondElementSelector = "." + ConfirmPasswordTextBoxCssClass,
                ContainerSelector = ".dnnRegistrationForm",
                UnmatchedCssClass = "unmatched",
                MatchedCssClass = "matched",
            };

            var optionsAsJsonString = Json.Serialize(confirmPasswordOptions);
            var script = string.Format("dnn.initializePasswordComparer({0});{1}", optionsAsJsonString, Environment.NewLine);

            if (ScriptManager.GetCurrent(this.Page) != null)
            {
                // respect MS AJAX
                ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "ConfirmPassword", script, true);
            }
            else
            {
                this.Page.ClientScript.RegisterStartupScript(this.GetType(), "ConfirmPassword", script, true);
            }
        }

        private void AddField(string dataField, string dataMember, bool required, string regexValidator, TextBoxMode textMode)
        {
            if (this.userForm.Items.Any(i => i.ID == dataField))
            {
                return;
            }

            var formItem = new DnnFormTextBoxItem
            {
                ID = dataField,
                DataField = dataField,
                DataMember = dataMember,
                Visible = true,
                Required = required,
                TextMode = textMode,
            };
            if (!string.IsNullOrEmpty(regexValidator))
            {
                formItem.ValidationExpression = regexValidator;
            }

            this.userForm.Items.Add(formItem);
        }

        private void AddPasswordStrengthField(string dataField, string dataMember, bool required)
        {
            DnnFormItemBase formItem;

            if (Host.EnableStrengthMeter)
            {
                formItem = new DnnFormPasswordItem
                {
                    TextBoxCssClass = PasswordStrengthTextBoxCssClass,
                    ContainerCssClass = "password-strength-container",
                };
            }
            else
            {
                formItem = new DnnFormTextBoxItem
                {
                    TextMode = TextBoxMode.Password,
                    TextBoxCssClass = PasswordStrengthTextBoxCssClass,
                };
            }

            formItem.ID = dataField;
            formItem.DataField = dataField;
            formItem.DataMember = dataMember;
            formItem.Visible = true;
            formItem.Required = required;

            this.userForm.Items.Add(formItem);
        }

        private void AddPasswordConfirmField(string dataField, string dataMember, bool required)
        {
            var formItem = new DnnFormTextBoxItem
            {
                ID = dataField,
                DataField = dataField,
                DataMember = dataMember,
                Visible = true,
                Required = required,
                TextMode = TextBoxMode.Password,
                TextBoxCssClass = ConfirmPasswordTextBoxCssClass,
                ClearContentInPasswordMode = true,
                MaxLength = 39,
            };
            this.userForm.Items.Add(formItem);
        }

        private void AddProperty(ProfilePropertyDefinition property)
        {
            if (this.userForm.Items.Any(i => i.ID == property.PropertyName))
            {
                return;
            }

            var controller = new ListController();
            ListEntryInfo imageType = controller.GetListEntryInfo("DataType", "Image");
            if (property.DataType != imageType.EntryID)
            {
                DnnFormEditControlItem formItem = new DnnFormEditControlItem
                {
                    ID = property.PropertyName,
                    ResourceKey = string.Format("ProfileProperties_{0}", property.PropertyName),
                    LocalResourceFile = "~/DesktopModules/Admin/Security/App_LocalResources/Profile.ascx.resx",
                    ValidationMessageSuffix = ".Validation",
                    ControlType = EditorInfo.GetEditor(property.DataType),
                    DataMember = "Profile",
                    DataField = property.PropertyName,
                    Visible = property.Visible,
                    Required = property.Required,
                };

                // To check if the property has a deafult value
                if (!string.IsNullOrEmpty(property.DefaultValue))
                {
                    formItem.Value = property.DefaultValue;
                }

                if (!string.IsNullOrEmpty(property.ValidationExpression))
                {
                    formItem.ValidationExpression = property.ValidationExpression;
                }

                this.userForm.Items.Add(formItem);
            }
        }

        private void BindLoginControl(AuthenticationLoginBase authLoginControl, AuthenticationInfo authSystem)
        {
            // set the control ID to the resource file name ( ie. controlname.ascx = controlname )
            // this is necessary for the Localization in PageBase
            authLoginControl.AuthenticationType = authSystem.AuthenticationType;
            authLoginControl.ID = Path.GetFileNameWithoutExtension(authSystem.LoginControlSrc) + "_" + authSystem.AuthenticationType;
            authLoginControl.LocalResourceFile = authLoginControl.TemplateSourceDirectory + "/" + Localization.LocalResourceDirectory + "/" +
                                                 Path.GetFileNameWithoutExtension(authSystem.LoginControlSrc);
            authLoginControl.RedirectURL = this.GetRedirectUrl();
            authLoginControl.ModuleConfiguration = this.ModuleConfiguration;

            authLoginControl.UserAuthenticated += this.UserAuthenticated;
        }

        private void CreateUser()
        {
            // Update DisplayName to conform to Format
            this.UpdateDisplayName();

            this.User.Membership.Approved = this.PortalSettings.UserRegistration == (int)Globals.PortalRegistrationType.PublicRegistration;
            var user = this.User;
            this.CreateStatus = UserController.CreateUser(ref user);

            DataCache.ClearPortalUserCountCache(this.PortalId);

            try
            {
                if (this.CreateStatus == UserCreateStatus.Success)
                {
                    // hide the succesful captcha
                    this.captchaRow.Visible = false;

                    // Assocate alternate Login with User and proceed with Login
                    if (!string.IsNullOrEmpty(this.AuthenticationType))
                    {
                        AuthenticationController.AddUserAuthentication(this.User.UserID, this.AuthenticationType, this.UserToken);
                    }

                    string strMessage = this.CompleteUserCreation(this.CreateStatus, user, true, this.IsRegister);

                    if (string.IsNullOrEmpty(strMessage))
                    {
                        this.Response.Redirect(this.GetRedirectUrl(), true);
                    }
                    else
                    {
                        this.RegistrationForm.Visible = false;
                        this.registerButton.Visible = false;
                        this.closeLink.Visible = true;
                    }
                }
                else
                {
                    this.AddLocalizedModuleMessage(UserController.GetUserCreateStatus(this.CreateStatus), ModuleMessage.ModuleMessageType.RedError, true);
                }
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void UpdateDisplayName()
        {
            // Update DisplayName to conform to Format
            if (!string.IsNullOrEmpty(this.PortalSettings.Registration.DisplayNameFormat))
            {
                this.User.UpdateDisplayName(this.PortalSettings.Registration.DisplayNameFormat);
            }
        }

        private bool Validate()
        {
            if (!string.IsNullOrEmpty(this.gotcha.Value))
            {
                return false;
            }

            this.CreateStatus = UserCreateStatus.AddUser;
            var portalSecurity = PortalSecurity.Instance;

            // Check User Editor
            bool _IsValid = this.userForm.IsValid;

            if (_IsValid)
            {
                var filterFlags = PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets | PortalSecurity.FilterFlag.NoMarkup;
                var name = this.User.Username ?? this.User.Email;
                var cleanUsername = PortalSecurity.Instance.InputFilter(name, filterFlags);
                if (!cleanUsername.Equals(name))
                {
                    this.CreateStatus = UserCreateStatus.InvalidUserName;
                }

                var valid = UserController.Instance.IsValidUserName(name);

                if (!valid)
                {
                    this.CreateStatus = UserCreateStatus.InvalidUserName;
                }

                var cleanEmail = PortalSecurity.Instance.InputFilter(this.User.Email, filterFlags);
                if (!cleanEmail.Equals(this.User.Email))
                {
                    this.CreateStatus = UserCreateStatus.InvalidEmail;
                }

                var cleanFirstName = PortalSecurity.Instance.InputFilter(this.User.FirstName, filterFlags);
                if (!cleanFirstName.Equals(this.User.FirstName))
                {
                    this.CreateStatus = UserCreateStatus.InvalidFirstName;
                }

                var cleanLastName = PortalSecurity.Instance.InputFilter(this.User.LastName, filterFlags);
                if (!cleanLastName.Equals(this.User.LastName))
                {
                    this.CreateStatus = UserCreateStatus.InvalidLastName;
                }

                if (string.IsNullOrEmpty(this.PortalSettings.Registration.DisplayNameFormat))
                {
                    var cleanDisplayName = PortalSecurity.Instance.InputFilter(this.User.DisplayName, filterFlags);
                    if (!cleanDisplayName.Equals(this.User.DisplayName))
                    {
                        this.CreateStatus = UserCreateStatus.InvalidDisplayName;
                    }
                }
            }

            if (this.PortalSettings.Registration.RegistrationFormType == 0)
            {
                // Update UserName
                if (this.PortalSettings.Registration.UseEmailAsUserName)
                {
                    this.User.Username = this.User.Email;
                    if (string.IsNullOrEmpty(this.User.DisplayName))
                    {
                        this.User.DisplayName = this.User.Email.Substring(0, this.User.Email.IndexOf("@", StringComparison.Ordinal));
                    }
                }

                // Check Password is valid
                if (!this.PortalSettings.Registration.RandomPassword)
                {
                    // Check Password is Valid
                    if (this.CreateStatus == UserCreateStatus.AddUser && !UserController.ValidatePassword(this.User.Membership.Password))
                    {
                        this.CreateStatus = UserCreateStatus.InvalidPassword;
                    }

                    if (this.PortalSettings.Registration.RequirePasswordConfirm && string.IsNullOrEmpty(this.AuthenticationType))
                    {
                        if (this.User.Membership.Password != this.User.Membership.PasswordConfirm)
                        {
                            this.CreateStatus = UserCreateStatus.PasswordMismatch;
                        }
                    }
                }
                else
                {
                    // Generate a random password for the user
                    this.User.Membership.Password = UserController.GeneratePassword();
                    this.User.Membership.PasswordConfirm = this.User.Membership.Password;
                }
            }
            else
            {
                // Set Username to Email
                if (string.IsNullOrEmpty(this.User.Username))
                {
                    this.User.Username = this.User.Email;
                }

                // Set DisplayName
                if (string.IsNullOrEmpty(this.User.DisplayName))
                {
                    this.User.DisplayName = string.IsNullOrEmpty(this.User.FirstName + " " + this.User.LastName)
                                           ? this.User.Email.Substring(0, this.User.Email.IndexOf("@", StringComparison.Ordinal))
                                           : this.User.FirstName + " " + this.User.LastName;
                }

                // Random Password
                if (string.IsNullOrEmpty(this.User.Membership.Password))
                {
                    // Generate a random password for the user
                    this.User.Membership.Password = UserController.GeneratePassword();
                }

                // Password Confirm
                if (!string.IsNullOrEmpty(this.User.Membership.PasswordConfirm))
                {
                    if (this.User.Membership.Password != this.User.Membership.PasswordConfirm)
                    {
                        this.CreateStatus = UserCreateStatus.PasswordMismatch;
                    }
                }
            }

            // Validate banned password
            var settings = new MembershipPasswordSettings(this.User.PortalID);

            if (settings.EnableBannedList)
            {
                var m = new MembershipPasswordController();
                if (m.FoundBannedPassword(this.User.Membership.Password) || this.User.Username == this.User.Membership.Password)
                {
                    this.CreateStatus = UserCreateStatus.BannedPasswordUsed;
                }
            }

            // Validate Profanity
            if (this.PortalSettings.Registration.UseProfanityFilter)
            {
                if (!portalSecurity.ValidateInput(this.User.Username, PortalSecurity.FilterFlag.NoProfanity))
                {
                    this.CreateStatus = UserCreateStatus.InvalidUserName;
                }

                if (!string.IsNullOrEmpty(this.User.DisplayName))
                {
                    if (!portalSecurity.ValidateInput(this.User.DisplayName, PortalSecurity.FilterFlag.NoProfanity))
                    {
                        this.CreateStatus = UserCreateStatus.InvalidDisplayName;
                    }
                }
            }

            // Validate Unique User Name
            UserInfo user = UserController.GetUserByName(this.PortalId, this.User.Username);
            if (user != null)
            {
                if (this.PortalSettings.Registration.UseEmailAsUserName)
                {
                    this.CreateStatus = UserCreateStatus.DuplicateEmail;
                }
                else
                {
                    this.CreateStatus = UserCreateStatus.DuplicateUserName;
                    int i = 1;
                    string userName = null;
                    while (user != null)
                    {
                        userName = this.User.Username + "0" + i.ToString(CultureInfo.InvariantCulture);
                        user = UserController.GetUserByName(this.PortalId, userName);
                        i++;
                    }

                    this.User.Username = userName;
                }
            }

            // Validate Unique Display Name
            if (this.CreateStatus == UserCreateStatus.AddUser && this.PortalSettings.Registration.RequireUniqueDisplayName)
            {
                user = UserController.Instance.GetUserByDisplayname(this.PortalId, this.User.DisplayName);
                if (user != null)
                {
                    this.CreateStatus = UserCreateStatus.DuplicateDisplayName;
                    int i = 1;
                    string displayName = null;
                    while (user != null)
                    {
                        displayName = this.User.DisplayName + " 0" + i.ToString(CultureInfo.InvariantCulture);
                        user = UserController.Instance.GetUserByDisplayname(this.PortalId, displayName);
                        i++;
                    }

                    this.User.DisplayName = displayName;
                }
            }

            // Check Question/Answer
            if (this.CreateStatus == UserCreateStatus.AddUser && MembershipProviderConfig.RequiresQuestionAndAnswer)
            {
                if (string.IsNullOrEmpty(this.User.Membership.PasswordQuestion))
                {
                    // Invalid Question
                    this.CreateStatus = UserCreateStatus.InvalidQuestion;
                }

                if (this.CreateStatus == UserCreateStatus.AddUser)
                {
                    if (string.IsNullOrEmpty(this.User.Membership.PasswordAnswer))
                    {
                        // Invalid Question
                        this.CreateStatus = UserCreateStatus.InvalidAnswer;
                    }
                }
            }

            if (this.CreateStatus != UserCreateStatus.AddUser)
            {
                _IsValid = false;
            }

            return _IsValid;
        }

        private string GetRedirectUrl(bool checkSetting = true)
        {
            var redirectUrl = string.Empty;
            var redirectAfterRegistration = this.PortalSettings.Registration.RedirectAfterRegistration;
            if (checkSetting && redirectAfterRegistration > 0) // redirect to after registration page
            {
                redirectUrl = this._navigationManager.NavigateURL(redirectAfterRegistration);
            }
            else
            {
                if (this.Request.QueryString["returnurl"] != null)
                {
                    // return to the url passed to register
                    redirectUrl = HttpUtility.UrlDecode(this.Request.QueryString["returnurl"]);

                    // clean the return url to avoid possible XSS attack.
                    redirectUrl = UrlUtils.ValidReturnUrl(redirectUrl);

                    if (redirectUrl.Contains("?returnurl"))
                    {
                        string baseURL = redirectUrl.Substring(
                            0,
                            redirectUrl.IndexOf("?returnurl", StringComparison.Ordinal));
                        string returnURL =
                            redirectUrl.Substring(redirectUrl.IndexOf("?returnurl", StringComparison.Ordinal) + 11);

                        redirectUrl = string.Concat(baseURL, "?returnurl", HttpUtility.UrlEncode(returnURL));
                    }
                }

                if (string.IsNullOrEmpty(redirectUrl))
                {
                    // redirect to current page
                    redirectUrl = this._navigationManager.NavigateURL();
                }
            }

            return redirectUrl;
        }

        private void registerButton_Click(object sender, EventArgs e)
        {
            if ((this.PortalSettings.Registration.UseCaptcha && this.ctlCaptcha.IsValid) || !this.PortalSettings.Registration.UseCaptcha)
            {
                if (this.IsValid)
                {
                    if (this.PortalSettings.UserRegistration != (int)Globals.PortalRegistrationType.NoRegistration)
                    {
                        this.CreateUser();
                    }
                }
                else
                {
                    if (this.CreateStatus != UserCreateStatus.AddUser)
                    {
                        this.AddLocalizedModuleMessage(UserController.GetUserCreateStatus(this.CreateStatus), ModuleMessage.ModuleMessageType.RedError, true);
                    }
                }
            }
        }

        private void UserAuthenticated(object sender, UserAuthenticatedEventArgs e)
        {
            NameValueCollection profileProperties = e.Profile;

            this.User.Username = e.UserToken;
            this.AuthenticationType = e.AuthenticationType;
            this.UserToken = e.UserToken;

            foreach (string key in profileProperties)
            {
                switch (key)
                {
                    case "FirstName":
                        this.User.FirstName = profileProperties[key];
                        break;
                    case "LastName":
                        this.User.LastName = profileProperties[key];
                        break;
                    case "Email":
                        this.User.Email = profileProperties[key];
                        break;
                    case "DisplayName":
                        this.User.DisplayName = profileProperties[key];
                        break;
                    default:
                        this.User.Profile.SetProfileProperty(key, profileProperties[key]);
                        break;
                }
            }

            // Generate a random password for the user
            this.User.Membership.Password = UserController.GeneratePassword();

            if (!string.IsNullOrEmpty(this.User.Email))
            {
                this.CreateUser();
            }
            else
            {
                this.AddLocalizedModuleMessage(this.LocalizeString("NoEmail"), ModuleMessage.ModuleMessageType.RedError, true);
                foreach (DnnFormItemBase formItem in this.userForm.Items)
                {
                    formItem.Visible = formItem.DataField == "Email";
                }

                this.userForm.DataBind();
            }
        }
    }
}
