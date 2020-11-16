// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Components.Prompt.Commands
{
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

    [ConsoleCommand("new-user", Constants.UsersCategory, "Prompt_NewUser_Description")]
    public class NewUser : ConsoleCommandBase
    {
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

        public override string LocalResourceFile => Constants.LocalResourcesFile;

        private string Email { get; set; }
        private string Username { get; set; }
        private string FirstName { get; set; }
        private string LastName { get; set; }
        private string Password { get; set; }
        private bool Approved { get; set; }
        private bool Notify { get; set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {

            this.Email = this.GetFlagValue(FlagEmail, "Email", string.Empty, true);
            this.Username = this.GetFlagValue(FlagUsername, "Username", string.Empty, true);
            this.FirstName = this.GetFlagValue(FlagFirstname, "FirstName", string.Empty, true);
            this.LastName = this.GetFlagValue(FlagLastname, "LastName", string.Empty, true);
            this.Password = this.GetFlagValue(FlagPassword, "Password", string.Empty);
            this.Approved = this.GetFlagValue(FlagApproved, "Approved", true);
            this.Notify = this.GetFlagValue(FlagNotify, "Notify", false);
            if (string.IsNullOrEmpty(this.Email)) return;
            var emailVal = new EmailValidator();
            if (!emailVal.IsValid(this.Email))
            {
                this.AddMessage(this.LocalizeString("Email.RegExError"));
            }
        }

        public override ConsoleResultModel Run()
        {
            var settings = new RegisterationDetails
            {
                PortalSettings = this.PortalSettings,
                Email = this.Email,
                FirstName = this.FirstName,
                LastName = this.LastName,
                UserName = this.Username,
                Password = this.Password,
                Notify = this.Notify,
                Authorize = this.Approved,
                RandomPassword = string.IsNullOrEmpty(this.Password),
                IgnoreRegistrationMode = true
            };
            try
            {
                var userInfo = RegisterController.Instance.Register(settings);
                var lstResult = new List<UserModel>
                {
                    new UserModel(UserController.Instance.GetUser(this.PortalId, userInfo.UserId))
                };
                return new ConsoleResultModel(this.LocalizeString("UserCreated")) { Data = lstResult, Records = lstResult.Count };
            }
            catch (Exception ex)
            {
                return new ConsoleErrorResultModel(ex.Message);
            }
        }
    }
}
