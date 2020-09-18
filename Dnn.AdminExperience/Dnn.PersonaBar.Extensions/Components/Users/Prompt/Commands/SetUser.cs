// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Components.Prompt.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Text;

    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.Users.Components.Dto;
    using Dnn.PersonaBar.Users.Components.Prompt.Models;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

    [ConsoleCommand("set-user", Constants.UsersCategory, "Prompt_SetUser_Description")]
    public class SetUser : ConsoleCommandBase
    {
        [FlagParameter("id", "Prompt_SetUser_FlagId", "Integer", true)]
        private const string FlagId = "id";

        [FlagParameter("email", "Prompt_SetUser_FlagEmail", "String")]
        private const string FlagEmail = "email";

        [FlagParameter("username", "Prompt_SetUser_FlagUsername", "String")]
        private const string FlagUsername = "username";

        [FlagParameter("displayname", "Prompt_SetUser_FlagDisplayname", "String")]
        private const string FlagDisplayname = "displayname";

        [FlagParameter("firstname", "Prompt_SetUser_FlagFirstname", "String")]
        private const string FlagFirstname = "firstname";

        [FlagParameter("lastname", "Prompt_SetUser_FlagLastname", "String")]
        private const string FlagLastname = "lastname";

        [FlagParameter("approved", "Prompt_SetUser_FlagApproved", "Boolean")]
        private const string FlagApproved = "approved";

        [FlagParameter("password", "Prompt_SetUser_FlagPassword", "String")]
        private const string FlagPassword = "password";

        private readonly IUserValidator _userValidator;
        private readonly IUsersController _usersController;
        private readonly IUserControllerWrapper _userControllerWrapper;

        public SetUser() : this(new UserValidator(), UsersController.Instance, new UserControllerWrapper())
        {
        }

        public SetUser(IUserValidator userValidator, IUsersController usersController, IUserControllerWrapper userControllerWrapper)
        {
            this._userValidator = userValidator;
            this._usersController = usersController;
            this._userControllerWrapper = userControllerWrapper;
        }

        public override string LocalResourceFile => Constants.LocalResourcesFile;

        private int? UserId { get; set; }
        private string Email { get; set; }
        private string Username { get; set; }
        private string DisplayName { get; set; }
        private string FirstName { get; set; }
        private string LastName { get; set; }
        private bool? Approved { get; set; }
        private string Password { get; set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {

            this.UserId = this.GetFlagValue(FlagId, "User Id", -1, true, true, true);
            this.Email = this.GetFlagValue(FlagEmail, "Email", string.Empty);
            this.Username = this.GetFlagValue(FlagUsername, "Username", string.Empty);
            this.DisplayName = this.GetFlagValue(FlagDisplayname, "DisplayName", string.Empty);
            this.FirstName = this.GetFlagValue(FlagFirstname, "FirstName", string.Empty);
            this.LastName = this.GetFlagValue(FlagLastname, "LastName", string.Empty);
            this.Password = this.GetFlagValue(FlagPassword, "Password", string.Empty);
            this.Approved = this.GetFlagValue<bool?>(FlagApproved, "Approved", null);

            // ensure there's something to update
            if (string.IsNullOrEmpty(this.Email) &&
                string.IsNullOrEmpty(this.Username) &&
                string.IsNullOrEmpty(this.DisplayName) &&
                string.IsNullOrEmpty(this.FirstName) &&
                string.IsNullOrEmpty(this.LastName) &&
                string.IsNullOrEmpty(this.Password) &&
                !this.Approved.HasValue)
            {
                this.AddMessage(this.LocalizeString("Prompt_NothingToSetUser"));
            }
        }

        public override ConsoleResultModel Run()
        {
            var sbResults = new StringBuilder();

            ConsoleErrorResultModel errorResultModel;
            UserInfo userInfo;

            if (
                (errorResultModel = this._userValidator.ValidateUser(
                    this.UserId,
                    this.PortalSettings,
                    this.User,
                    out userInfo)
                ) != null
               )
            {
                return errorResultModel;
            }

            // Update the User
            // process the password first. If invalid, we can abort other changes to the user
            if (!string.IsNullOrEmpty(this.Password))
            {
                try
                {
                    this._usersController.ChangePassword(userInfo.PortalID, userInfo.UserID, this.Password);
                    sbResults.Append(this.LocalizeString("ChangeSuccessful"));
                }
                catch (Exception ex)
                {
                    return new ConsoleErrorResultModel(ex.Message);
                }
            }

            if (this.Approved.HasValue && userInfo.Membership.Approved != this.Approved.Value)
            {
                this._usersController.UpdateAuthorizeStatus(userInfo, userInfo.PortalID, this.Approved.Value);
                sbResults.Append(this.LocalizeString(this.Approved.Value ? "UserAuthorized" : "UserUnAuthorized"));
            }

            var basicUpdated = !string.IsNullOrEmpty(this.Username) || !string.IsNullOrEmpty(this.DisplayName) || !string.IsNullOrEmpty(this.FirstName) || !string.IsNullOrEmpty(this.LastName) || !string.IsNullOrEmpty(this.Email);
            var userBasicDto = new UserBasicDto
            {
                Displayname = userInfo.DisplayName,
                UserId = userInfo.UserID,
                Email = userInfo.Email,
                IsDeleted = userInfo.IsDeleted,
                Username = userInfo.Username,
                Firstname = userInfo.FirstName,
                Lastname = userInfo.LastName
            };
            // Update Username
            if (!string.IsNullOrEmpty(this.Username))
                userBasicDto.Username = this.Username;
            // Update other properties
            if (!string.IsNullOrEmpty(this.DisplayName))
                userBasicDto.Displayname = this.DisplayName;
            if (!string.IsNullOrEmpty(this.FirstName))
                userBasicDto.Firstname = this.FirstName;
            if (!string.IsNullOrEmpty(this.LastName))
                userBasicDto.Lastname = this.LastName;
            if (!string.IsNullOrEmpty(this.Email))
                userBasicDto.Email = this.Email;
            if (basicUpdated)
            {
                try
                {
                    this._usersController.UpdateUserBasicInfo(userBasicDto, userInfo.PortalID);
                }
                catch (SqlException)
                {
                    return new ConsoleErrorResultModel(this.LocalizeString("UsernameNotUnique") + "\n" + sbResults);
                }
                catch (Exception ex)
                {
                    return new ConsoleErrorResultModel(ex.Message + sbResults);
                }
            }
            // retrieve the updated user
            var updatedUser = this._userControllerWrapper.GetUserById(userInfo.PortalID, userInfo.UserID);

            var lst = new List<UserModel> { new UserModel(updatedUser) };

            return new ConsoleResultModel(string.Empty)
            {
                Data = lst,
                Records = lst.Count,
                FieldOrder = UserModel.FieldOrder,
                Output = this.LocalizeString("UserUpdated")
            };
        }
    }
}
