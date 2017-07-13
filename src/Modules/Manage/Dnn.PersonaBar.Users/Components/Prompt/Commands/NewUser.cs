using System.Collections.Generic;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Common;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Users.Components.Prompt.Models;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Users.Components.Prompt.Commands
{
    [ConsoleCommand("new-user", "Creates a new user record", new[]{
        "email",
        "username",
        "displayname",
        "firstname",
        "lastname",
        "password",
        "approved",
        "notify"
    })]
    public class NewUser : ConsoleCommandBase
    {
        private const string FlagEmail = "email";
        private const string FlagUsername = "username";
        private const string FlagDisplayname = "displayname";
        private const string FlagFirstname = "firstname";
        private const string FlagLastname = "lastname";
        private const string FlagPassword = "password";
        private const string FlagApproved = "approved";
        private const string FlagNotify = "notify";


        public string Email { get; private set; }
        public string Username { get; private set; }
        public string DisplayName { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Password { get; private set; }
        public bool Approved { get; private set; }
        public bool? Notify { get; private set; } // if not specified, it will use the site settings

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);

            var sbErrors = new StringBuilder();

            if (HasFlag(FlagEmail))
                Email = Flag(FlagEmail);
            if (HasFlag(FlagUsername))
                Username = Flag(FlagUsername);
            if (HasFlag(FlagDisplayname))
                DisplayName = Flag(FlagDisplayname);
            if (HasFlag(FlagFirstname))
                FirstName = Flag(FlagFirstname);
            if (HasFlag(FlagLastname))
                LastName = Flag(FlagLastname);
            if (HasFlag(FlagPassword))
                Password = Flag(FlagPassword);
            if (HasFlag(FlagApproved))
            {
                var tmpApproved = false;
                if (bool.TryParse(Flag(FlagApproved), out tmpApproved))
                {
                    Approved = tmpApproved;
                }
                else
                {
                    sbErrors.AppendFormat("If specified, --{0} must be True or False; ", FlagApproved);
                }
            }
            if (HasFlag(FlagNotify))
            {
                var tempNotify = false;
                if (bool.TryParse(Flag(FlagNotify), out tempNotify))
                {
                    Notify = tempNotify;
                }
                else
                {
                    sbErrors.AppendFormat("If specified, --{0} must be True or False; ", FlagNotify);
                }
            }

            // required fields
            if (string.IsNullOrEmpty(Flag(FlagEmail)))
                sbErrors.Append("email is required; ");
            if (string.IsNullOrEmpty(Flag(FlagUsername)))
                sbErrors.Append("username is required; ");
            if (string.IsNullOrEmpty(Flag(FlagFirstname)) || string.IsNullOrEmpty(Flag(FlagLastname)))
            {
                sbErrors.Append("firstname and lastname are required; ");
            }

            if (sbErrors.Length == 0)
            {
                // validate email 
                var emailVal = new EmailValidator();
                if (!emailVal.IsValid(Flag(FlagEmail)))
                {
                    sbErrors.AppendFormat("Supplied email '{0}' is invalid; ", Flag(FlagEmail));
                }

                // There will be a problem if the caller is generating a password but the site is 
                // 1) Set for private/none registration
                // 2) Or the command is set to not send a notification.
                if (string.IsNullOrEmpty(Flag(FlagPassword)))
                {
                    var newPassword = UserController.GeneratePassword();
                    if (!Notify.HasValue || Notify == false)
                    {
                        // check the site settings for portal registration type.
                        switch (portalSettings.UserRegistration)
                        {
                            case (int)Globals.PortalRegistrationType.NoRegistration:
                                break;
                            case (int)Globals.PortalRegistrationType.PrivateRegistration:
                                break;
                            case (int)Globals.PortalRegistrationType.PublicRegistration:
                                break;
                            case (int)Globals.PortalRegistrationType.VerifiedRegistration:
                                break;
                        }
                    }
                }
                Password = string.IsNullOrEmpty(Flag(FlagPassword)) ? UserController.GeneratePassword() : Flag(FlagPassword);
                var bApproved = true;

                // approved is True by default since admin/host would be using Prompt
                if (HasFlag(FlagApproved))
                {
                    // something specified for approved, go with what we're given
                    if (bool.TryParse(Flag(FlagApproved), out bApproved))
                    {
                        Approved = bApproved;
                    }
                    else
                    {
                        sbErrors.AppendFormat("--{0} must either be True or False, you specified '{1}'; ", FlagApproved, Flag(FlagApproved));
                    }
                }
                else
                {
                    Approved = bApproved;
                }

                if (Approved && (!Notify.HasValue || Notify == true) && PortalSettings.UserRegistration == (int)Globals.PortalRegistrationType.VerifiedRegistration)
                {
                    // If Notify is true, a user will be sent an verification email but they are already verified

                    sbErrors.AppendFormat("The user is set to Approved{0}, but Site Registration is 'Verified'. Executing this command would send a redundant and confusing email to the user; " + "Either set --{1} to false, --{2} to false, or set the site's registration to something other than 'Verified'; ", HasFlag(FlagApproved) ? string.Empty : " (the default value)", FlagApproved, FlagNotify);
                }
            }
            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {
            var sbError = new StringBuilder();
            var lstResult = new List<UserModel>();

            var ui = new UserInfo();
            ui.FirstName = FirstName;
            ui.LastName = LastName;
            ui.DisplayName = string.IsNullOrEmpty(DisplayName) ? $"{ui.FirstName.Trim()} {ui.LastName.Trim()}"
                : DisplayName;
            ui.Email = Email;
            ui.Username = Username;
            ui.PortalID = PortalId;

            ui.Membership.Password = Password;
            ui.Membership.Approved = Approved;


            var statusCreate = UserController.CreateUser(ref ui);
            if (statusCreate == DotNetNuke.Security.Membership.UserCreateStatus.Success)
            {
                if (PortalSettings.UserRegistration == (int)Globals.PortalRegistrationType.VerifiedRegistration)
                {
                    // ensure user placed in appropriate roles
                    UserController.ApproveUser(ui);
                }
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
                        case (int)Globals.PortalRegistrationType.PrivateRegistration:
                            Utilities.SendSystemEmail(ui, DotNetNuke.Services.Mail.MessageType.UserRegistrationPrivate, PortalSettings);
                            break;
                        case (int)Globals.PortalRegistrationType.PublicRegistration:
                            Utilities.SendSystemEmail(ui, DotNetNuke.Services.Mail.MessageType.UserRegistrationPublic, PortalSettings);
                            break;
                        case (int)Globals.PortalRegistrationType.VerifiedRegistration:
                            Utilities.SendSystemEmail(ui, DotNetNuke.Services.Mail.MessageType.UserRegistrationVerified, PortalSettings);
                            break;
                    }
                }
                return new ConsoleResultModel("User successfully created") { Data = lstResult };
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