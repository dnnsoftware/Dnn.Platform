using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Portals;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Users.Components.Dto;
using Dnn.PersonaBar.Users.Components.Prompt.Models;

namespace Dnn.PersonaBar.Users.Components.Prompt.Commands
{
    [ConsoleCommand("set-user", Constants.UsersCategory, "Prompt_SetUser_Description")]
    public class SetUser : ConsoleCommandBase
    {
        public override string LocalResourceFile => Constants.LocalResourcesFile;

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

        private int? UserId { get; set; }
        private string Email { get; set; }
        private string Username { get; set; }
        private string DisplayName { get; set; }
        private string FirstName { get; set; }
        private string LastName { get; set; }
        private bool? Approved { get; set; }
        private string Password { get; set; }

        public SetUser() : this(new UserValidator(), UsersController.Instance, new UserControllerWrapper())
        {
        }

        public SetUser(IUserValidator userValidator, IUsersController usersController, IUserControllerWrapper userControllerWrapper)
        {
            this._userValidator = userValidator;
            this._usersController = usersController;
            this._userControllerWrapper = userControllerWrapper;
        }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            
            UserId = GetFlagValue(FlagId, "User Id", -1, true, true, true);
            Email = GetFlagValue(FlagEmail, "Email", string.Empty);
            Username = GetFlagValue(FlagUsername, "Username", string.Empty);
            DisplayName = GetFlagValue(FlagDisplayname, "DisplayName", string.Empty);
            FirstName = GetFlagValue(FlagFirstname, "FirstName", string.Empty);
            LastName = GetFlagValue(FlagLastname, "LastName", string.Empty);
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

            if (
                (errorResultModel = _userValidator.ValidateUser(
                    UserId,
                    PortalSettings,
                    User,
                    out userInfo)
                ) != null
               )
            {
                return errorResultModel;
            }                        
          
            // Update the User
            // process the password first. If invalid, we can abort other changes to the user
            if (!string.IsNullOrEmpty(Password))
            {
                try
                {                    
                    _usersController.ChangePassword(userInfo.PortalID, userInfo.UserID, Password);
                    sbResults.Append(LocalizeString("ChangeSuccessful"));
                }
                catch (Exception ex)
                {
                    return new ConsoleErrorResultModel(ex.Message);
                }
            }

            if (Approved.HasValue && userInfo.Membership.Approved != Approved.Value)
            {
                _usersController.UpdateAuthorizeStatus(userInfo, userInfo.PortalID, Approved.Value);
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
                    _usersController.UpdateUserBasicInfo(userBasicDto, userInfo.PortalID);
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
            var updatedUser = _userControllerWrapper.GetUserById(userInfo.PortalID, userInfo.UserID);

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