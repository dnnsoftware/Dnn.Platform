using System.Collections.Generic;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Users.Components.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;

namespace Dnn.PersonaBar.Users.Components.Prompt.Commands
{
    [ConsoleCommand("delete-user", "Deletes the specifed user from the portal.", new[]{
        "id",
        "notify"
    })]
    public class DeleteUser : ConsoleCommandBase
    {
        private const string FlagId = "id";
        private const string FlagNotify = "notify";


        public int? UserId { get; private set; }
        public bool Notify { get; private set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            var sbErrors = new StringBuilder();

            if (HasFlag(FlagId))
            {
                int tmpId;
                if (int.TryParse(Flag(FlagId), out tmpId))
                    UserId = tmpId;
            }
            else
            {
                int tmpId;
                if (int.TryParse(args[1], out tmpId))
                    UserId = tmpId;
            }

            if (!UserId.HasValue)
            {
                sbErrors.Append(Localization.GetString("Prompt_UserIdIsRequired", Constants.LocalResourcesFile));
            }

            if (HasFlag(FlagNotify))
            {
                bool tmpNotify;
                if (bool.TryParse(Flag(FlagNotify), out tmpNotify))
                {
                    Notify = tmpNotify;
                }
                else
                {
                    sbErrors.Append(string.Format(Localization.GetString("Prompt_IfSpecifiedMustHaveValue", Constants.LocalResourcesFile), FlagNotify) + " ");
                }
            }

            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {
            ConsoleErrorResultModel errorResultModel;
            UserInfo userInfo;
            if ((errorResultModel = Utilities.ValidateUser(UserId, PortalSettings, User, out userInfo)) != null) return errorResultModel;
            var userModels = new List<UserModel> { new UserModel(userInfo) };
            if (userInfo.IsDeleted)
                return new ConsoleErrorResultModel(Localization.GetString("Prompt_UserAlreadyDeleted", Constants.LocalResourcesFile));

            if (!UserController.DeleteUser(ref userInfo, Notify, false))
            {
                return new ConsoleErrorResultModel(Localization.GetString("Prompt_UserDeletionFailed", Constants.LocalResourcesFile))
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
            return new ConsoleResultModel(Localization.GetString("UserDeleted", Constants.LocalResourcesFile)) { Data = userModels, Records = userModels.Count };
        }
    }
}