using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Recyclebin.Components.Prompt.Commands
{
    [ConsoleCommand("restore-module", "Restores a module from the DNN recycle bin", new[]
    {
        "id",
        "pageid"
    })]
    public class RestoreModule : ConsoleCommandBase
    {
        protected override string LocalResourceFile => Constants.LocalResourcesFile;

        private const string FlagId = "id";
        private const string FlagPageId = "pageid";

        private int ModuleId { get; set; }
        private int PageId { get; set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            ModuleId = GetFlagValue(FlagId, "Module Id", -1, true, true, true);
            PageId = GetFlagValue(FlagPageId, "Page Id", -1, true, false, true);
        }

        public override ConsoleResultModel Run()
        {
            string message;
            var restored = RecyclebinController.Instance.RestoreModule(ModuleId, PageId, out message);
            return !restored
                ? new ConsoleErrorResultModel(message)
                : new ConsoleResultModel(string.Format(LocalizeString("Prompt_ModuleRestoredSuccessfully"), ModuleId)) { Records = 1 };
        }
    }
}