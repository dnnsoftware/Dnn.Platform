using System.Collections.Generic;
using System.Net;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using ModulesControllerLibrary = Dnn.PersonaBar.Library.Controllers.ModulesController;

namespace Dnn.PersonaBar.Prompt.Components.Commands.Module
{
    [ConsoleCommand("delete-module", Constants.ModulesCategory, "Prompt_DeleteModule_Description")]
    public class DeleteModule : ConsoleCommandBase
    {
        public override string LocalResourceFile => Constants.LocalResourcesFile;

        [FlagParameter("id", "Prompt_DeleteModule_FlagId", "Integer", true)]
        private const string FlagId = "id";

        [FlagParameter("pageid", "Prompt_DeleteModule_FlagPageId", "Integer", true)]
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
            ModulesControllerLibrary.Instance.DeleteModule(PortalSettings, ModuleId, PageId, out message);
            return string.IsNullOrEmpty(message.Value)
                ? new ConsoleResultModel(LocalizeString("Prompt_ModuleDeleted"))
                : new ConsoleErrorResultModel(message.Value) { Records = 1 };
        }
    }
}