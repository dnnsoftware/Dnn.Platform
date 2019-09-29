using System.Collections.Generic;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using System.Net;
using Dnn.PersonaBar.Library.Controllers;

namespace Dnn.PersonaBar.Recyclebin.Components.Prompt.Commands
{
    [ConsoleCommand("purge-module", Constants.RecylcleBinCategory, "Prompt_PurgeModule_Description")]
    public class PurgeModule : ConsoleCommandBase
    {
        public override string LocalResourceFile => Constants.LocalResourcesFile;

        [FlagParameter("id", "Prompt_PurgeModule_FlagId", "Integer", true)]
        private const string FlagId = "id";

        [FlagParameter("pageid", "Prompt_PurgeModule_FlagPageId", "Integer", true)]
        private const string FlagPageId = "pageid";

        private int ModuleId { get; set; }
        private int PageId { get; set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            
            ModuleId = GetFlagValue(FlagId, "Module Id", -1, true, true, true);
            PageId = GetFlagValue(FlagPageId, "Page Id", -1, true, false, true);
        }

        public override ConsoleResultModel Run()
        {
            KeyValuePair<HttpStatusCode, string> message;
            var module = ModulesController.Instance.GetModule(PortalSettings, ModuleId, PageId, out message);
            if (module == null)
                return new ConsoleErrorResultModel(string.Format(LocalizeString("ModuleNotFound"), ModuleId, PageId));
            var modulesToPurge = new List<ModuleInfo> { module };
            var errors = new StringBuilder();
            RecyclebinController.Instance.DeleteModules(modulesToPurge, errors);
            return errors.Length > 0
                ? new ConsoleErrorResultModel(string.Format(LocalizeString("Service_RemoveTabModuleError"), errors))
                : new ConsoleResultModel(string.Format(LocalizeString("Prompt_ModulePurgedSuccessfully"), ModuleId)) { Records = 1 };
        }
    }
}