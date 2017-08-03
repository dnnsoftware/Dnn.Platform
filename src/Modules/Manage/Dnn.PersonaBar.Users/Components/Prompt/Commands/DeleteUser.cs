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


        private int UserId { get; set; }
        private bool Notify { get; set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            UserId = GetFlagValue(FlagId, "User Id", -1, true, true, true);
            Notify = GetFlagValue(FlagNotify, "Notify", false);
        }

        public override ConsoleResultModel Run()
        {
            ConsoleErrorResultModel errorResultModel;
            UserInfo userInfo;
            if ((errorResultModel = Utilities.ValidateUser(UserId, PortalSettings, User, out userInfo)) != null) return errorResultModel;
            var userModels = new List<UserModel> { new UserModel(userInfo) };
            if (userInfo.IsDeleted)
                return new ConsoleErrorResultModel(LocalizeString("Prompt_UserAlreadyDeleted"));

            if (!UserController.DeleteUser(ref userInfo, Notify, false))
            {
                return new ConsoleErrorResultModel(LocalizeString("Prompt_UserDeletionFailed"))
                {
                    Data = userModels
                };
            }
            // We must clear User cache or else, when the user is 'removed' (so it can't be restored), you 
            // will not be able to create a new user with the same username -- even though no user with that username
            // exists.
            DotNetNuke.Common.Utilities.DataCache.ClearUserCache(PortalId, userInfo.Username);
            // attempt to retrieve the user from the dB
            userInfo = UserController.GetUserById(PortalId, userInfo.UserID);
            userModels = new List<UserModel> { new UserModel(userInfo) };
            return new ConsoleResultModel(LocalizeString("UserDeleted")) { Data = userModels, Records = userModels.Count };
        }
    }
}