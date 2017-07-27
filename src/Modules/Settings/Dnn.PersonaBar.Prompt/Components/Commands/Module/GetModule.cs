using System.Collections.Generic;
using System.Net;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Prompt.Components.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Prompt.Components.Commands.Module
{
    [ConsoleCommand("get-module", "Gets module information for module specified", new[] { "id" })]
    public class GetModule : ConsoleCommandBase
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
            var lst = new List<ModuleInfoModel>();
            KeyValuePair<HttpStatusCode, string> message;
            var moduleInfo = ModulesController.Instance.GetModule(ModuleId, PageId, out message);
            if (moduleInfo == null && !string.IsNullOrEmpty(message.Value))
            {
                return new ConsoleErrorResultModel(message.Value);
            }
            lst.Add(ModuleInfoModel.FromDnnModuleInfo(moduleInfo));
            return new ConsoleResultModel { Data = lst, Records = lst.Count };
        }
    }
}