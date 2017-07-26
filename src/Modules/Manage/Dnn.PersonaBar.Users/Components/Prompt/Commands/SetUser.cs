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

namespace Dnn.PersonaBar.Users.Components.Prompt.Commands
{
    [ConsoleCommand("set-user", "Updates values on the specified user", new[]{
        "id",
        "email",
        "username",
        "displayname",
        "firstname",
        "lastname",
        "approved",
        "password"

    })]
    public class SetUser : ConsoleCommandBase
    {
        protected override string LocalResourceFile => Constants.LocalResourcesFile;

        private const string FlagId = "id";
        private const string FlagEmail = "email";
        private const string FlagUsername = "username";
        private const string FlagDisplayname = "displayname";
        private const string FlagFirstname = "firstname";
        private const string FlagLastname = "lastname";
        private const string FlagApproved = "approved";
        private const string FlagPassword = "password";


        public int? UserId { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool? Approved { get; set; }
        public string Password { get; set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            UserId = GetFlagValue(FlagId, "User Id", -1, true, true, true);
            Email = GetFlagValue(FlagEmail, "Email", string.Empty);
            Username = GetFlagValue(FlagUsername, "Username", string.Empty);
            DisplayName = GetFlagValue(FlagDisplayname, "DisplayName", string.Empty);
            FirstName = GetFlagValue(FlagFirstname, "FirstName", string.Empty, true);
            LastName = GetFlagValue(FlagLastname, "LastName", string.Empty, true);
            Password = GetFlagValue(FlagPassword, "Password", string.Empty);
            Approved = GetFlagValue<bool?>(FlagApproved, "Approved", null);

            // ensure there's something to update
            if (string.IsNullOrEmpty(Email) &&
                string.IsNullOrEmpty(Username) &&
                string.IsNullOrEmpty(DisplayName) &&
                string.IsNullOrEmpty(FirstName) &&
                string.IsNullOrEmpty(LastName) &&
                string.IsNullOrEmpty(Password) &&
                !Approved.HasValue)
            {
                AddMessage(LocalizeString("Prompt_NothingToSetUser"));
            }
        }

        public override ConsoleResultModel Run()
        {
            var sbResults = new StringBuilder();

            ConsoleErrorResultModel errorResultModel;
            UserInfo userInfo;
            if ((errorResultModel = Utilities.ValidateUser(UserId, PortalSettings, User, out userInfo)) != null) return errorResultModel;

            // Update the User
            // process the password first. If invalid, we can abort other changes to the user
            if (!string.IsNullOrEmpty(Password))
            {
                try
                {
                    UsersController.Instance.ChangePassword(PortalId, userInfo.UserID, Password);
                    sbResults.Append(LocalizeString("ChangeSuccessful"));
                }
                catch (Exception ex)
                {
                    return new ConsoleErrorResultModel(ex.Message);
                }
            }
            if (Approved.HasValue && userInfo.Membership.Approved != Approved.Value)
            {
                UsersController.Instance.UpdateAuthorizeStatus(userInfo, PortalId, Approved.Value);
                sbResults.Append(LocalizeString(Approved.Value ? "UserAuthorized" : "UserUnAuthorized"));

            }
            var basicUpdated = !string.IsNullOrEmpty(Username) || !string.IsNullOrEmpty(DisplayName) || !string.IsNullOrEmpty(FirstName) || !string.IsNullOrEmpty(LastName) || !string.IsNullOrEmpty(Email);
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
            if (!string.IsNullOrEmpty(Username))
                userBasicDto.Username = Username;
            // Update other properties
            if (!string.IsNullOrEmpty(DisplayName))
                userBasicDto.Displayname = DisplayName;
            if (!string.IsNullOrEmpty(FirstName))
                userBasicDto.Firstname = FirstName;
            if (!string.IsNullOrEmpty(LastName))
                userBasicDto.Lastname = LastName;
            if (!string.IsNullOrEmpty(Email))
                userBasicDto.Email = Email;
            if (basicUpdated)
            {
                try
                {
                    UsersController.Instance.UpdateUserBasicInfo(userBasicDto);
                }
                catch (SqlException)
                {
                    return new ConsoleErrorResultModel(LocalizeString("UsernameNotUnique") + "\n" + sbResults);
                }
                catch (Exception ex)
                {
                    return new ConsoleErrorResultModel(ex.Message + sbResults);
                }
            }
            // retrieve the updated user
            var updatedUser = UserController.GetUserById(PortalId, userInfo.UserID);

            var lst = new List<UserModel> { new UserModel(updatedUser) };

            return new ConsoleResultModel(string.Empty)
            {
                Data = lst,
                Records = lst.Count,
                FieldOrder = UserModel.FieldOrder,
                Output = LocalizeString("UserUpdated")
            };
        }


    }
}