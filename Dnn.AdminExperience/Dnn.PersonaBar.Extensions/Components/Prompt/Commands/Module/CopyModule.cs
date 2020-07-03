// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Prompt.Components.Commands.Module
{
    using System.Collections.Generic;
    using System.Net;

    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.Prompt.Components.Models;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

    using ModulesControllerLibrary = Dnn.PersonaBar.Library.Controllers.ModulesController;

    [ConsoleCommand("copy-module", Constants.ModulesCategory, "Prompt_CopyModule_Description")]
    public class CopyModule : ConsoleCommandBase
    {
        [FlagParameter("id", "Prompt_CopyModule_FlagId", "Integer", true)]
        private const string FlagId = "id";

        [FlagParameter("pageid", "Prompt_CopyModule_FlagPageId", "Integer", true)]
        private const string FlagPageId = "pageid";

        [FlagParameter("topageid", "Prompt_CopyModule_FlagToPageId", "Integer", true)]
        private const string FlagToPageId = "topageid";

        [FlagParameter("pane", "Prompt_CopyModule_FlagPane", "String", "ContentPane")]
        private const string FlagPane = "pane";

        [FlagParameter("includesettings", "Prompt_CopyModule_FlagIncludesettings", "Boolean", "true")]
        private const string FlagIncludesettings = "includesettings";

        public override string LocalResourceFile => Constants.LocalResourcesFile;
        private int ModuleId { get; set; }
        private int PageId { get; set; }
        private int TargetPageId { get; set; }
        private string Pane { get; set; }
        private bool? IncludeSettings { get; set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {

            this.ModuleId = this.GetFlagValue(FlagId, "Module Id", -1, true, true, true);
            this.PageId = this.GetFlagValue(FlagPageId, "Page Id", -1, true, false, true);
            this.TargetPageId = this.GetFlagValue(FlagToPageId, "To Page Id", -1, true, false, true);
            this.Pane = this.GetFlagValue(FlagPane, "Pane", "ContentPane");
            this.IncludeSettings = this.GetFlagValue<bool?>(FlagIncludesettings, "Include settings", null);
            if (this.PageId == this.TargetPageId)
            {
                this.AddMessage(this.LocalizeString("Prompt_SourceAndTargetPagesAreSame"));
            }
        }

        public override ConsoleResultModel Run()
        {
            var lst = new List<ModuleInfoModel>();
            KeyValuePair<HttpStatusCode, string> message;
            var copiedModule = ModulesControllerLibrary.Instance.CopyModule(
                this.PortalSettings,
                this.ModuleId,
                this.PageId,
                this.TargetPageId,
                this.Pane,
                this.IncludeSettings ?? true,
                out message
                );
            if (copiedModule == null && !string.IsNullOrEmpty(message.Value))
                return new ConsoleErrorResultModel(message.Value);
            lst.Add(ModuleInfoModel.FromDnnModuleInfo(copiedModule));
            return new ConsoleResultModel(this.LocalizeString("Prompt_ModuleCopied")) { Data = lst, Records = lst.Count };
        }
    }
}
