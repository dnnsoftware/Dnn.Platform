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
using DotNetNuke.Services.Localization;

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
        private const string FlagId = "id";
        private const string FlagEmail = "email";
        private const string FlagUsername = "username";
        private const string FlagDisplayname = "displayname";
        private const string FlagFirstname = "firstname";
        private const string FlagLastname = "lastname";
        private const string FlagApproved = "approved";
        private const string FlagPassword = "password";


        public int? UserId { get; private set; }
        public string Email { get; private set; }
        public string Username { get; private set; }
        public string DisplayName { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public bool? Approved { get; private set; }
        public string Password { get; private set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            var sbErrors = new StringBuilder();


            if (HasFlag(FlagId))
            {
                var tmpId = 0;
                if (int.TryParse(Flag(FlagId), out tmpId))
                    UserId = tmpId;
            }
            else
            {
                // if ID not explicitly passed, it must be the first argument
                if (args.Length > 1 && !IsFlag(args[1]))
                {
                    var tmpId = 0;
                    if (int.TryParse(args[1], out tmpId))
                    {
                        UserId = tmpId;
                    }
                }
            }
            if (!UserId.HasValue)
            {
                // error. no valid user ID passed
                sbErrors.Append(Localization.GetString("Prompt_UserIdIsRequired", Constants.LocalResourcesFile));
            }
            else
            {
                // Only continue if there's a valid UserID
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
                {
                    Password = Flag(FlagPassword);
                }
                Approved = null;
                if (HasFlag(FlagApproved))
                {
                    bool approved;
                    if (bool.TryParse(Flag(FlagApproved), out approved))
                        Approved = approved;
                }

                // ensure there's something to update
                if (string.IsNullOrEmpty(Email) &&
                    string.IsNullOrEmpty(Username) &&
                    string.IsNullOrEmpty(DisplayName) &&
                    string.IsNullOrEmpty(FirstName) &&
                    string.IsNullOrEmpty(LastName) &&
                    string.IsNullOrEmpty(Password) &&
                    !Approved.HasValue)
                {
                    sbErrors.Append(Localization.GetString("Prompt_NothingToSetUser", Constants.LocalResourcesFile));
                }
            }

            ValidationMessage = sbErrors.ToString();
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
                    sbResults.Append(Localization.GetString("ChangeSuccessful", Constants.LocalResourcesFile));
                }
                catch (Exception ex)
                {
                    return new ConsoleErrorResultModel(ex.Message);
                }
            }
            if (Approved.HasValue && userInfo.Membership.Approved != Approved.Value)
            {
                UsersController.Instance.UpdateAuthorizeStatus(userInfo, PortalId, Approved.Value);
                sbResults.Append(Localization.GetString(Approved.Value ? "UserAuthorized" : "UserUnAuthorized", Constants.LocalResourcesFile));

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
                    return new ConsoleErrorResultModel(Localization.GetString("UsernameNotUnique", Constants.LocalResourcesFile) + "\n" + sbResults);
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
                FieldOrder = UserModel.FieldOrder,
                Output = Localization.GetString("UserUpdated", Constants.LocalResourcesFile)
            };
        }


    }
}