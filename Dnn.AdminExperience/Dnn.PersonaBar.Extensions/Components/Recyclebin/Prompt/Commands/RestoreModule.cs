// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Recyclebin.Components.Prompt.Commands
{
    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

    [ConsoleCommand("restore-module", Constants.RecylcleBinCategory, "Prompt_RestoreModule_Description")]
    public class RestoreModule : ConsoleCommandBase
    {
        [FlagParameter("id", "Prompt_RestoreModule_FlagId", "Integer", true)]
        private const string FlagId = "id";

        [FlagParameter("pageid", "Prompt_RestoreModule_FlagPageId", "Integer", true)]
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
            string message;
            var restored = RecyclebinController.Instance.RestoreModule(this.ModuleId, this.PageId, out message);
            return !restored
                ? new ConsoleErrorResultModel(message)
                : new ConsoleResultModel(string.Format(this.LocalizeString("Prompt_ModuleRestoredSuccessfully"), this.ModuleId)) { Records = 1 };
        }
    }
}
