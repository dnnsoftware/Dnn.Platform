using System.Collections.Generic;
using System.Net;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Prompt.Components.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using ModulesControllerLibrary = Dnn.PersonaBar.Library.Controllers.ModulesController;

namespace Dnn.PersonaBar.Prompt.Components.Commands.Module
{
    [ConsoleCommand("move-module", Constants.ModulesCategory, "Prompt_MoveModule_Description")]
    public class MoveModule : ConsoleCommandBase
    {
        public override string LocalResourceFile => Constants.LocalResourcesFile;

        [FlagParameter("id", "Prompt_MoveModule_FlagId", "Integer", true)]
        private const string FlagId = "id";

        [FlagParameter("pageid", "Prompt_MoveModule_FlagPageId", "Integer", true)]
        private const string FlagPageId = "pageid";

        [FlagParameter("topageid", "Prompt_MoveModule_FlagToPageId", "Integer", true)]
        private const string FlagToPageId = "topageid";

        [FlagParameter("pane", "Prompt_MoveModule_FlagPane", "String", "ContentPane")]
        private const string FlagPane = "pane";

        private int ModuleId { get; set; }
        private int PageId { get; set; }
        private int TargetPageId { get; set; }
        private string Pane { get; set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            
            ModuleId = GetFlagValue(FlagId, "Module Id", -1, true, true, true);
            PageId = GetFlagValue(FlagPageId, "Page Id", -1, true, false, true);
            TargetPageId = GetFlagValue(FlagToPageId, "To Page Id", -1, true, false, true);
            Pane = GetFlagValue(FlagPane, "Pane", "ContentPane");

            if (PageId == TargetPageId)
            {
                AddMessage(LocalizeString("Prompt_SourceAndTargetPagesAreSame"));
            }
        }

        public override ConsoleResultModel Run()
        {
            var modules = new List<ModuleInfoModel>();
            KeyValuePair<HttpStatusCode, string> message;
            var movedModule = ModulesControllerLibrary.Instance.CopyModule(
                PortalSettings,
                ModuleId,
                PageId,
                TargetPageId,
                Pane,
                true,
                out message,
                true
                );
            if (movedModule == null && !string.IsNullOrEmpty(message.Value))
                return new ConsoleErrorResultModel(message.Value);
            modules.Add(ModuleInfoModel.FromDnnModuleInfo(movedModule));
            return new ConsoleResultModel(LocalizeString("Prompt_ModuleMoved")) { Data = modules, Records = modules.Count };
        }
    }
}