using System;
using System.Collections.Generic;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Common;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Users.Components.Contracts;
using Dnn.PersonaBar.Users.Components.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;

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
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(NewUser));
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
        public bool Approved { get; private set; } = true;
        public bool Notify { get; private set; } // if not specified, it will use the site settings

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
                    sbErrors.AppendFormat(Localization.GetString("Prompt_IfSpecifiedMustHaveValue", Constants.LocalResourcesFile), FlagApproved);
                }
            }
            if (HasFlag(FlagNotify))
            {
                bool tempNotify;
                if (bool.TryParse(Flag(FlagNotify), out tempNotify))
                {
                    Notify = tempNotify;
                }
                else
                {
                    sbErrors.AppendFormat(Localization.GetString("Prompt_IfSpecifiedMustHaveValue", Constants.LocalResourcesFile), FlagNotify);
                }
            }

            // required fields
            if (string.IsNullOrEmpty(Flag(FlagEmail)))
                sbErrors.Append(Localization.GetString("Email.Required", Constants.LocalResourcesFile));
            if (string.IsNullOrEmpty(Flag(FlagUsername)))
                sbErrors.Append(Localization.GetString("Username.Required", Constants.LocalResourcesFile));
            if (string.IsNullOrEmpty(Flag(FlagFirstname)))
            {
                sbErrors.Append(Localization.GetString("FirstName.Required", Constants.LocalResourcesFile));
            }
            if (string.IsNullOrEmpty(Flag(FlagLastname)))
            {
                sbErrors.Append(Localization.GetString("LastName.Required", Constants.LocalResourcesFile));
            }

            if (sbErrors.Length == 0)
            {
                // validate email 
                var emailVal = new EmailValidator();
                if (!emailVal.IsValid(Flag(FlagEmail)))
                {
                    sbErrors.AppendFormat(Localization.GetString("Email.RegExError", Constants.LocalResourcesFile));
                }
            }
            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {
            var settings = new RegisterationDetails
            {
                PortalSettings = PortalSettings,
                Email = Email,
                FirstName = FirstName,
                LastName = LastName,
                UserName = Username,
                Password = Password,
                Notify = Notify,
                Authorize = Approved,
                RandomPassword = string.IsNullOrEmpty(Password),
                IgnoreRegistrationMode = true
            };
            try
            {
                var userInfo = RegisterController.Instance.Register(settings);
                var lstResult = new List<UserModel>
                {
                    new UserModel(UserController.Instance.GetUser(PortalId, userInfo.UserId))
                };
                return new ConsoleResultModel(Localization.GetString("UserCreated", Constants.LocalResourcesFile)) { Data = lstResult, Records = lstResult.Count };
            }
            catch (Exception ex)
            {
                return new ConsoleErrorResultModel(ex.Message);
            }
        }

    }
}