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
    [ConsoleCommand("new-user", Constants.UsersCategory, "Prompt_NewUser_Description")]
    public class NewUser : ConsoleCommandBase
    {
        public override string LocalResourceFile => Constants.LocalResourcesFile;

        [FlagParameter("email", "Prompt_NewUser_FlagEmail", "String", true)]
        private const string FlagEmail = "email";
        [FlagParameter("username", "Prompt_NewUser_FlagUsername", "String", true)]
        private const string FlagUsername = "username";
        [FlagParameter("firstname", "Prompt_NewUser_FlagFirstname", "String", true)]
        private const string FlagFirstname = "firstname";
        [FlagParameter("lastname", "Prompt_NewUser_FlagLastname", "String", true)]
        private const string FlagLastname = "lastname";
        [FlagParameter("password", "Prompt_NewUser_FlagPassword", "String", "auto-generated")]
        private const string FlagPassword = "password";
        [FlagParameter("approved", "Prompt_NewUser_FlagApproved", "Boolean", "true")]
        private const string FlagApproved = "approved";
        [FlagParameter("notify", "Prompt_NewUser_FlagNotify", "Boolean", "false")]
        private const string FlagNotify = "notify";

        private string Email { get; set; }
        private string Username { get; set; }
        private string FirstName { get; set; }
        private string LastName { get; set; }
        private string Password { get; set; }
        private bool Approved { get; set; }
        private bool Notify { get; set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            
            Email = GetFlagValue(FlagEmail, "Email", string.Empty, true);
            Username = GetFlagValue(FlagUsername, "Username", string.Empty, true);
            FirstName = GetFlagValue(FlagFirstname, "FirstName", string.Empty, true);
            LastName = GetFlagValue(FlagLastname, "LastName", string.Empty, true);
            Password = GetFlagValue(FlagPassword, "Password", string.Empty);
            Approved = GetFlagValue(FlagApproved, "Approved", true);
            Notify = GetFlagValue(FlagNotify, "Notify", false);
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