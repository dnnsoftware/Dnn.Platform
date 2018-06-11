using System.Collections.Generic;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Users.Components.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Users.Components.Prompt.Commands
{
    [ConsoleCommand("delete-user", Constants.UsersCategory, "Prompt_DeleteUser_Description")]
    public class DeleteUser : ConsoleCommandBase
    {
        public override string LocalResourceFile => Constants.LocalResourcesFile;

        [FlagParameter("id", "Prompt_DeleteUser_FlagId", "Integer", true)]
        private const string FlagId = "id";
        [FlagParameter("notify", "Prompt_DeleteUser_FlagNotify", "Boolean", "false")]
        private const string FlagNotify = "notify";

        private IUserValidator _userValidator;
        private IUserControllerWrapper _userControllerWrapper;

        private int UserId { get; set; }
        private bool Notify { get; set; }

        public DeleteUser() : this(new UserValidator(), new UserControllerWrapper())
        {
        }

        public DeleteUser(IUserValidator userValidator, IUserControllerWrapper userControllerWrapper)
        {
            this._userValidator = userValidator;
            this._userControllerWrapper = userControllerWrapper;
        }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            
            UserId = GetFlagValue(FlagId, "User Id", -1, true, true, true);
            Notify = GetFlagValue(FlagNotify, "Notify", false);
        }

        public override ConsoleResultModel Run()
        {
            ConsoleErrorResultModel errorResultModel;
            UserInfo userInfo;

            if ((errorResultModel = _userValidator.ValidateUser(UserId, PortalSettings, User, out userInfo)) != null)
            {
                return errorResultModel;
            }

            var userModels = new List<UserModel> { new UserModel(userInfo) };            

            if (userInfo.IsDeleted)
            {
                return new ConsoleErrorResultModel(LocalizeString("Prompt_UserAlreadyDeleted"));
            }

            var validPortalId = userInfo.PortalID;

            if (!_userControllerWrapper.DeleteUserAndClearCache(ref userInfo, Notify, false))
            {
                return new ConsoleErrorResultModel(LocalizeString("Prompt_UserDeletionFailed"))
                {
                    Data = userModels
                };
            }

            // attempt to retrieve the user from the dB 
            userInfo = _userControllerWrapper.GetUserById(validPortalId, userInfo.UserID);
            userModels = new List<UserModel> { new UserModel(userInfo) };
            return new ConsoleResultModel(LocalizeString("UserDeleted")) { Data = userModels, Records = userModels.Count };            
        }
    }
}