using Dnn.PersonaBar.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Users.Membership;
using System.Collections.Generic;
using System.Text;
using System;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;

namespace Dnn.PersonaBar.Prompt.Commands.User
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
                sbErrors.AppendFormat("No valid User ID found. Please pass the ID as the first argument after the command or explicitly using the --{0} flag; ", FlagId);
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
                    if (UserController.ValidatePassword(Flag(FlagPassword)))
                    {
                        Password = Flag(FlagPassword);
                    }
                    else
                    {
                        sbErrors.Append("Supplied password is invalid. Please supply a password that meets the minimum requirements of ths site; ");
                    }
                }
                if (HasFlag(FlagApproved))
                {
                    // there is no DNN API for setting Approval status to false. 
                    // So we do not allow setting approved to false;
                    Approved = true;
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
                    sbErrors.Append("Nothing to update. Please pass-in one or more flags with values to update on the user or type 'help set-user' for more help; ");
                }
            }

            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {
            var sbResults = new StringBuilder();

            // get the current user
            var userToUpdate = UserController.Instance.GetUserById(PortalId, (int)UserId);

            // Does User Exist?
            if (userToUpdate == null)
                return new ConsoleErrorResultModel($"No user found with the ID '{(int)UserId}'");
            // Only allow a SuperUser to update another SuperUser
            if (userToUpdate.IsSuperUser && !User.IsSuperUser)
                return new ConsoleErrorResultModel("You do not have permission to update this user.");

            // Update the User
            // process the password first. If invalid, we can abort other changes to the user
            if (!string.IsNullOrEmpty(Password))
            {
                // This call is only valid for those in administrator roles
                // If password reset is not enabled in membership config, this will throw ex.
                // UserController.ResetAndChangePassword(userToUpdate, Password);

                // verify password isn't a recently used password
                var m = new MembershipPasswordController();
                if (m.IsPasswordInHistory(userToUpdate.UserID, PortalId, Password))
                {
                    return new ConsoleErrorResultModel("The supplied password has been used recently and cannot be used again. No changes to the user have been made.");
                }
                UserController.ResetPasswordToken(userToUpdate, 1);
                userToUpdate = UserController.Instance.GetUserById(PortalId, userToUpdate.UserID);
                var newToken = userToUpdate.PasswordResetToken.ToString();
                try
                {
                    if (UserController.ChangePasswordByToken(PortalId, userToUpdate.Username, Password, newToken))
                    {
                        // success
                        sbResults.Append("The password was successfully changed. ");
                    }
                    else
                    {
                        // typically we shouldn't get here b/c password has been validated and checked against the password history.
                        return new ConsoleResultModel("Unable to change user password. No changes have been made to the user.");
                    }
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                    return new ConsoleErrorResultModel("An unexpected error occurred while trying to update the password. See the DNN Event Viewer for details. No changes to the user have been made.");
                }
            }

            // Update Username
            if (!string.IsNullOrEmpty(Username))
            {
                try
                {
                    UserController.ChangeUsername((int)UserId, Username);
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                    var msg = "An error occurred while changing the user's Username. See the DNN Event Viewer. ";
                    if (sbResults.Length > 0)
                    {
                        msg += sbResults.ToString() + "No other changes have been made. ";
                    }
                    else
                    {
                        msg += "No changes have been made to the user. ";
                    }
                    return new ConsoleErrorResultModel(msg);
                }
                // retrieve updated user info.
                userToUpdate = UserController.Instance.GetUserById(PortalId, userToUpdate.UserID);
                sbResults.Append("The Username has been changed. ");
            }

            // Update other properties
            if (!string.IsNullOrEmpty(DisplayName))
                userToUpdate.DisplayName = DisplayName;
            if (!string.IsNullOrEmpty(FirstName))
                userToUpdate.FirstName = FirstName;
            if (!string.IsNullOrEmpty(LastName))
                userToUpdate.LastName = LastName;
            if (!string.IsNullOrEmpty(Email))
                userToUpdate.Email = Email;
            if (Approved.HasValue)
            {
                userToUpdate.Membership.Approved = true;
            }

            try
            {
                UserController.UpdateUser(PortalId, userToUpdate);
                if (Approved.HasValue)
                {
                    // If user was approved, remove from unverified user, auto-assign roles
                    UserController.ApproveUser(userToUpdate);
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return new ConsoleErrorResultModel("An error occurred while updating the user. See the DNN Event Viewer. " + sbResults.ToString());
            }

            // retrieve the updated user
            var updatedUser = UserController.GetUserById(PortalId, userToUpdate.UserID);

            var lst = new List<UserModel>();
            lst.Add(new UserModel(updatedUser));

            if (lst.Count > 0)
            {
                return new ConsoleResultModel(string.Empty)
                {
                    Data = lst,
                    FieldOrder = UserModel.FieldOrder
                };
            }
            return new ConsoleResultModel("No user found");
        }


    }
}