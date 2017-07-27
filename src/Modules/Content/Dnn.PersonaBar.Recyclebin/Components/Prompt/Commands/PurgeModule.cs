using System.Collections.Generic;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;

namespace Dnn.PersonaBar.Recyclebin.Components.Prompt.Commands
{
    [ConsoleCommand("purge-module", "Permanently deletes a module instance that has previously been sent to the DNN Recycle Bin", new[]{
        "id",
        "pageid"
    })]
    public class PurgeModule : ConsoleCommandBase
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
            var module = ModuleController.Instance.GetModule(ModuleId, PageId, true);
            if (module == null)
                return new ConsoleErrorResultModel(string.Format(LocalizeString("ModuleNotFound"), ModuleId));
            var modulesToPurge = new List<ModuleInfo> { module };
            var errors = new StringBuilder();
            RecyclebinController.Instance.DeleteModules(modulesToPurge, errors);
            return errors.Length > 0
                ? new ConsoleErrorResultModel(string.Format(LocalizeString("Service_RemoveTabModuleError"), errors))
                : new ConsoleResultModel(string.Format(LocalizeString("Prompt_ModulePurgedSuccessfully"), ModuleId)) { Records = 1 };
        }
    }
}