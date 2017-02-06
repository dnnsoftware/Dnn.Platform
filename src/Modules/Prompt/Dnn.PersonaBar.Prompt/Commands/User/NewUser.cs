using Dnn.PersonaBar.Prompt.Attributes;
using Dnn.PersonaBar.Prompt.Common;
using Dnn.PersonaBar.Prompt.Interfaces;
using Dnn.PersonaBar.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users;
using System.Collections.Generic;
using System.Text;
using static DotNetNuke.Common.Globals;

namespace Dnn.PersonaBar.Prompt.Commands.User
{
    [ConsoleCommand("new-user", "Creates a new user record", new string[]{
        "email",
        "username",
        "displayname",
        "firstname",
        "lastname",
        "password",
        "approved",
        "notify"
    })]
    public class NewUser : ConsoleCommandBase, IConsoleCommand
    {
        private const string FLAG_EMAIL = "email";
        private const string FLAG_USERNAME = "username";
        private const string FLAG_DISPLAYNAME = "displayname";
        private const string FLAG_FIRSTNAME = "firstname";
        private const string FLAG_LASTNAME = "lastname";
        private const string FLAG_PASSWORD = "password";
        private const string FLAG_APPROVED = "approved";
        private const string FLAG_NOTIFY = "notify";

        public string ValidationMessage { get; private set; }
        public string Email { get; private set; }
        public string Username { get; private set; }
        public string DisplayName { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Password { get; private set; }
        public bool Approved { get; private set; }
        public bool? Notify { get; private set; } // if not specified, it will use the site settings

        public void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Initialize(args, portalSettings, userInfo, activeTabId);

            StringBuilder sbErrors = new StringBuilder();

            if (HasFlag(FLAG_EMAIL))
                Email = Flag(FLAG_EMAIL);
            if (HasFlag(FLAG_USERNAME))
                Username = Flag(FLAG_USERNAME);
            if (HasFlag(FLAG_DISPLAYNAME))
                DisplayName = Flag(FLAG_DISPLAYNAME);
            if (HasFlag(FLAG_FIRSTNAME))
                FirstName = Flag(FLAG_FIRSTNAME);
            if (HasFlag(FLAG_LASTNAME))
                LastName = Flag(FLAG_LASTNAME);
            if (HasFlag(FLAG_PASSWORD))
                Password = Flag(FLAG_PASSWORD);
            if (HasFlag(FLAG_APPROVED))
            {
                bool tmpApproved = false;
                if (bool.TryParse(Flag(FLAG_APPROVED), out tmpApproved))
                {
                    Approved = tmpApproved;
                }
                else
                {
                    sbErrors.AppendFormat("If specified, --{0} must be True or False; ", FLAG_APPROVED);
                }
            }
            if (HasFlag(FLAG_NOTIFY))
            {
                bool tempNotify = false;
                if (bool.TryParse(Flag(FLAG_NOTIFY), out tempNotify))
                {
                    Notify = tempNotify;
                }
                else
                {
                    sbErrors.AppendFormat("If specified, --{0} must be True or False; ", FLAG_NOTIFY);
                }
            }

            // required fields
            if (string.IsNullOrEmpty(Flag(FLAG_EMAIL)))
                sbErrors.Append("email is required; ");
            if (string.IsNullOrEmpty(Flag(FLAG_USERNAME)))
                sbErrors.Append("username is required; ");
            if (string.IsNullOrEmpty(Flag(FLAG_FIRSTNAME)) || string.IsNullOrEmpty(Flag(FLAG_LASTNAME)))
            {
                sbErrors.Append("firstname and lastname are required; ");
            }

            if (sbErrors.Length == 0)
            {
                // validate email 
                var emailVal = new EmailValidator();
                if (!emailVal.IsValid(Flag(FLAG_EMAIL)))
                {
                    sbErrors.AppendFormat("Supplied email '{0}' is invalid; ", Flag(FLAG_EMAIL));
                }

                // There will be a problem if the caller is generating a password but the site is 
                // 1) Set for private/none registration
                // 2) Or the command is set to not send a notification.
                if (string.IsNullOrEmpty(Flag(FLAG_PASSWORD)))
                {
                    var newPassword = UserController.GeneratePassword();
                    if (!Notify.HasValue || Notify == false)
                    {
                        // check the site settings for portal registration type.
                        switch (portalSettings.UserRegistration)
                        {
                            case (int)PortalRegistrationType.NoRegistration:
                                break;
                            case (int)PortalRegistrationType.PrivateRegistration:
                                break;
                            case (int)PortalRegistrationType.PublicRegistration:
                                break;
                            case (int)PortalRegistrationType.VerifiedRegistration:
                                break;
                        }
                    }
                }
                Password = (string.IsNullOrEmpty(Flag(FLAG_PASSWORD)) ? UserController.GeneratePassword() : Flag(FLAG_PASSWORD));
                bool bApproved = false;

                // approved is True by default since admin/host would be using Prompt
                if (HasFlag(FLAG_APPROVED))
                {
                    // something specified for approved, go with what we're given
                    if (bool.TryParse(Flag(FLAG_APPROVED), out bApproved))
                    {
                        Approved = bApproved;
                    }
                    else
                    {
                        sbErrors.AppendFormat("--{0} must either be True or False, you specified '{1}'; ", FLAG_APPROVED, Flag(FLAG_APPROVED));
                    }
                }

                if (Approved && (!Notify.HasValue || Notify == true) && PortalSettings.UserRegistration == (int)PortalRegistrationType.VerifiedRegistration)
                {
                    // If Notify is true, a user will be sent an verification email but they are already verified

                    sbErrors.AppendFormat("The user is set to Approved{0}, but Site Registration is 'Verified'. Executing this command would send a redundant and confusing email to the user; " + "Either set --{1} to false, --{2} to false, or set the site's registration to something other than 'Verified'; ", (HasFlag(FLAG_APPROVED) ? string.Empty : " (the default value)"), FLAG_APPROVED, FLAG_NOTIFY);
                }
            }
            ValidationMessage = sbErrors.ToString();
        }

        public bool IsValid()
        {
            return string.IsNullOrEmpty(ValidationMessage);
        }

        public ConsoleResultModel Run()
        {
            StringBuilder sbError = new StringBuilder();
            List<UserModel> lstResult = new List<UserModel>();
            
            UserInfo ui = new UserInfo(); 
            ui.FirstName = FirstName;
            ui.LastName = LastName;
            ui.DisplayName = (string.IsNullOrEmpty(DisplayName) ? string.Format("{0} {1}", ui.FirstName.Trim(), ui.LastName.Trim()) : DisplayName);
            ui.Email = Email;
            ui.Username = Username;
            ui.PortalID = PortalId;

            ui.Membership.Password = Password;
            ui.Membership.Approved = Approved;


            var statusCreate = UserController.CreateUser(ref ui);
            if (statusCreate == DotNetNuke.Security.Membership.UserCreateStatus.Success)
            {
                ui.Profile.InitialiseProfile(PortalId);
                ui.Profile.SetProfileProperty("FirstName", FirstName);
                ui.Profile.SetProfileProperty("LastName", LastName);
                var props = ui.Profile.ProfileProperties;
                ProfileController.UpdateUserProfile(ui, props);
                lstResult.Add(new UserModel(ui));
                // If nothing has been set for Notify, then use the site settings.
                // If Notify is true, send notification
                if (!Notify.HasValue || (bool)Notify)
                {
                    switch (PortalSettings.UserRegistration)
                    {
                        case (int)PortalRegistrationType.PrivateRegistration:
                            Prompt.Utilities.SendSystemEmail(ui, DotNetNuke.Services.Mail.MessageType.UserRegistrationPrivate, PortalSettings);
                            break;
                        case (int)PortalRegistrationType.PublicRegistration:
                            Prompt.Utilities.SendSystemEmail(ui, DotNetNuke.Services.Mail.MessageType.UserRegistrationPublic, PortalSettings);
                            break;
                        case (int)PortalRegistrationType.VerifiedRegistration:
                            Prompt.Utilities.SendSystemEmail(ui, DotNetNuke.Services.Mail.MessageType.UserRegistrationVerified, PortalSettings);
                            break;
                    }
                }
                return new ConsoleResultModel("User successfully created") { data = lstResult };
            }
            else
            {
                switch (statusCreate)
                {
                    case DotNetNuke.Security.Membership.UserCreateStatus.DuplicateEmail:
                        sbError.AppendFormat("There is already a user with the email '{0}'; ", ui.Email);
                        break;
                    case DotNetNuke.Security.Membership.UserCreateStatus.InvalidEmail:
                        sbError.AppendFormat("Email address '{0}' is invalid; ", ui.Email);
                        break;
                    case DotNetNuke.Security.Membership.UserCreateStatus.InvalidPassword:
                        sbError.Append("The supplied password is invalid; ");
                        break;
                    case DotNetNuke.Security.Membership.UserCreateStatus.InvalidUserName:
                        sbError.AppendFormat("The supplied username '{0}' is invalid; ", ui.Username);
                        break;
                    case DotNetNuke.Security.Membership.UserCreateStatus.UserAlreadyRegistered:
                        sbError.Append("The user is already registered; ");
                        break;
                    case DotNetNuke.Security.Membership.UserCreateStatus.UsernameAlreadyExists:
                        sbError.AppendFormat("The username '{0}' already exists; ", ui.Username);
                        break;
                    //Case DotNetNuke.Security.Membership.UserCreateStatus.AddUser
                    //Case DotNetNuke.Security.Membership.UserCreateStatus.AddUserToPortal
                    //    ' User exists but not in this portal. The user is added to this portal.
                    //Case DotNetNuke.Security.Membership.UserCreateStatus.DuplicateProviderUserKey
                    case DotNetNuke.Security.Membership.UserCreateStatus.DuplicateUserName:
                        sbError.AppendFormat("The '{0}' username already exists; ", ui.Username);
                        break;
                    //Case DotNetNuke.Security.Membership.UserCreateStatus.InvalidAnswer
                    //Case DotNetNuke.Security.Membership.UserCreateStatus.InvalidProviderUserKey
                    //Case DotNetNuke.Security.Membership.UserCreateStatus.InvalidQuestion
                    //Case DotNetNuke.Security.Membership.UserCreateStatus.PasswordMismatch
                    //Case DotNetNuke.Security.Membership.UserCreateStatus.ProviderError
                    //Case DotNetNuke.Security.Membership.UserCreateStatus.UnexpectedError
                    //Case DotNetNuke.Security.Membership.UserCreateStatus.UserRejected
                    default:
                        sbError.Append(statusCreate.ToString());
                        break;
                }
            }
            return new ConsoleErrorResultModel(sbError.ToString());
        }

    }
}