// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Recyclebin.Components.Prompt.Commands
{
    using System.Collections.Generic;
    using System.Net;
    using System.Text;

    using Dnn.PersonaBar.Library.Controllers;
    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

    [ConsoleCommand("purge-module", Constants.RecylcleBinCategory, "Prompt_PurgeModule_Description")]
    public class PurgeModule : ConsoleCommandBase
    {
        [FlagParameter("id", "Prompt_PurgeModule_FlagId", "Integer", true)]
        private const string FlagId = "id";

        [FlagParameter("pageid", "Prompt_PurgeModule_FlagPageId", "Integer", true)]
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
            KeyValuePair<HttpStatusCode, string> message;
            var module = ModulesController.Instance.GetModule(this.PortalSettings, this.ModuleId, this.PageId, out message);
            if (module == null)
                return new ConsoleErrorResultModel(string.Format(this.LocalizeString("ModuleNotFound"), this.ModuleId, this.PageId));
            var modulesToPurge = new List<ModuleInfo> { module };
            var errors = new StringBuilder();
            RecyclebinController.Instance.DeleteModules(modulesToPurge, errors);
            return errors.Length > 0
                ? new ConsoleErrorResultModel(string.Format(this.LocalizeString("Service_RemoveTabModuleError"), errors))
                : new ConsoleResultModel(string.Format(this.LocalizeString("Prompt_ModulePurgedSuccessfully"), this.ModuleId)) { Records = 1 };
        }
    }
}
