using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Users.Components;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Recyclebin.Components.Prompt.Commands
{
    [ConsoleCommand("restore-user", Constants.RecylcleBinCategory, "Prompt_RestoreUser_Description")]
    public class RestoreUser : ConsoleCommandBase
    {
        public override string LocalResourceFile => Constants.LocalResourcesFile;

        [FlagParameter("id", "Prompt_RestoreUser_FlagId", "Integer", true)]
        private const string FlagId = "id";
        private IUserValidator _userValidator;
        private IRecyclebinController _recyclebinController;

        private int UserId { get; set; }

        public RestoreUser(): this (new UserValidator(), RecyclebinController.Instance)
        {
        }

        public RestoreUser(IUserValidator userValidator, IRecyclebinController instance)
        {
            this._userValidator = userValidator;
            this._recyclebinController = instance;
        }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            
            UserId = GetFlagValue(FlagId, "User Id", -1, true, true, true);
        }

        public override ConsoleResultModel Run()
        {
            UserInfo userInfo;
            _userValidator.ValidateUser(UserId, PortalSettings, User, out userInfo);
                
            if (userInfo == null)
                return new ConsoleErrorResultModel(string.Format(LocalizeString("UserNotFound"), UserId));

            if (!userInfo.IsDeleted)
                return new ConsoleErrorResultModel(LocalizeString("Prompt_RestoreNotRequired"));

            string message;
            var restoredUser = _recyclebinController.RestoreUser(userInfo, out message);
            return restoredUser
                ? new ConsoleResultModel(LocalizeString("UserRestored")) { Records = 1 }
                : new ConsoleErrorResultModel(message);
        }
    }
}