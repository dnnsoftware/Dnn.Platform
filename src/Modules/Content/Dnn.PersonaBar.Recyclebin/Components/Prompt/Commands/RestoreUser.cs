using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Recyclebin.Components.Prompt.Commands
{
    [ConsoleCommand("restore-user", "Recovers a user that has previously been deleted or 'unregistered'", new[] { "id" })]
    public class RestoreUser : ConsoleCommandBase
    {
        protected override string LocalResourceFile => Constants.LocalResourcesFile;

        private const string FlagId = "id";
        public int UserId { get; private set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            UserId = GetFlagValue(FlagId, "User Id", -1, true, true, true);
        }

        public override ConsoleResultModel Run()
        {
            var userInfo = UserController.Instance.GetUser(PortalId, UserId);
            if (userInfo == null)
                return new ConsoleErrorResultModel(string.Format(LocalizeString("UserNotFound"), UserId));

            if (!userInfo.IsDeleted)
                return new ConsoleErrorResultModel(LocalizeString("Prompt_RestoreNotRequired"));

            if (!UserController.RestoreUser(ref userInfo))
                return new ConsoleErrorResultModel(LocalizeString("UserRestoreError"));

            string message;
            var restoredUser = RecyclebinController.Instance.RestoreUser(userInfo, out message);
            return restoredUser
                ? new ConsoleResultModel(LocalizeString("UserRestored")) { Records = 1 }
                : new ConsoleErrorResultModel(message);
        }
    }
}