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
    [ConsoleCommand("copy-module", "Copies the module specified", new[] { "id", "pageid", "topageid", "pane" })]
    public class CopyModule : ConsoleCommandBase
    {
        protected override string LocalResourceFile => Constants.LocalResourcesFile;

        private const string FlagId = "id";
        private const string FlagPageId = "pageid";
        private const string FlagToPageId = "topageid";
        private const string FlagPane = "pane";
        private const string FlagIncludesettings = "includesettings";

        private int ModuleId { get; set; }
        private int PageId { get; set; }
        private int TargetPageId { get; set; }
        private string Pane { get; set; }
        private bool? IncludeSettings { get; set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            ModuleId = GetFlagValue(FlagId, "Module Id", -1, true, true, true);
            PageId = GetFlagValue(FlagPageId, "Page Id", -1, true, false, true);
            TargetPageId = GetFlagValue(FlagToPageId, "To Page Id", -1, true, false, true);
            Pane = GetFlagValue(FlagPane, "Pane", "ContentPane");
            IncludeSettings = GetFlagValue<bool?>(FlagIncludesettings, "Include settings", true);
            if (PageId == TargetPageId)
            {
                AddMessage(LocalizeString("Prompt_SourceAndTargetPagesAreSame"));
            }
        }

        public override ConsoleResultModel Run()
        {
            var lst = new List<ModuleInfoModel>();
            KeyValuePair<HttpStatusCode, string> message;
            var copiedModule = ModulesController.Instance.CopyModule(PortalSettings, ModuleId, PageId, TargetPageId, Pane, IncludeSettings ?? true, out message);
            if (copiedModule == null && !string.IsNullOrEmpty(message.Value))
                return new ConsoleErrorResultModel(message.Value);
            lst.Add(ModuleInfoModel.FromDnnModuleInfo(copiedModule));
            return new ConsoleResultModel(LocalizeString("Prompt_ModuleCopied")) { Data = lst, Records = lst.Count };
        }
    }
}