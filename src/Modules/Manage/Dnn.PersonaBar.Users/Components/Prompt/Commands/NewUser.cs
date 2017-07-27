using System;
using System.Collections.Generic;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Common;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Users.Components.Contracts;
using Dnn.PersonaBar.Users.Components.Prompt.Models;
using DotNetNuke.Entities.Portals;
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
        protected override string LocalResourceFile => Constants.LocalResourcesFile;

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
        public bool Notify { get; private set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            Email = GetFlagValue(FlagEmail, "Email", string.Empty, true);
            Username = GetFlagValue(FlagUsername, "Username", string.Empty, true);
            DisplayName = GetFlagValue(FlagDisplayname, "DisplayName", string.Empty);
            FirstName = GetFlagValue(FlagFirstname, "FirstName", string.Empty, true);
            LastName = GetFlagValue(FlagLastname, "LastName", string.Empty, true);
            Password = GetFlagValue(FlagPassword, "Password", string.Empty);
            Approved = GetFlagValue(FlagApproved, "Approved", true);
            Notify = GetFlagValue(FlagNotify, "Notify", PortalSettings.EnableRegisterNotification);
            if (string.IsNullOrEmpty(Email)) return;
            var emailVal = new EmailValidator();
            if (!emailVal.IsValid(Email))
            {
                AddMessage(LocalizeString("Email.RegExError"));
            }
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
                return new ConsoleResultModel(LocalizeString("UserCreated")) { Data = lstResult, Records = lstResult.Count };
            }
            catch (Exception ex)
            {
                return new ConsoleErrorResultModel(ex.Message);
            }
        }

    }
}