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

    [ConsoleCommand("get-module", Constants.ModulesCategory, "Prompt_GetModule_Description")]
    public class GetModule : ConsoleCommandBase
    {
        [FlagParameter("id", "Prompt_GetModule_FlagId", "Integer", true)]
        private const string FlagId = "id";

        [FlagParameter("pageid", "Prompt_GetModule_FlagPageId", "Integer", true)]
        private const string FlagPageId = "pageid";

        public override string LocalResourceFile => Constants.LocalResourcesFile;
        private int ModuleId { get; set; }
        private int PageId { get; set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {

            this.ModuleId = this.GetFlagValue(FlagId, "Module Id", -1, true, true, true);
            this.PageId = this.GetFlagValue(FlagPageId, "Page Id", -1, true, false, true);
        }

        public override ConsoleResultModel Run()
        {
            var lst = new List<ModuleInfoModel>();
            KeyValuePair<HttpStatusCode, string> message;
            var moduleInfo = ModulesControllerLibrary.Instance.GetModule(
                this.PortalSettings,
                this.ModuleId,
                this.PageId,
                out message
                );
            if (moduleInfo == null && !string.IsNullOrEmpty(message.Value))
            {
                return new ConsoleErrorResultModel(message.Value);
            }
            lst.Add(ModuleInfoModel.FromDnnModuleInfo(moduleInfo));
            return new ConsoleResultModel { Data = lst, Records = lst.Count, Output = string.Format(this.LocalizeString("Prompt_GetModule_Result"), this.ModuleId, this.PageId) };
        }
    }
}
