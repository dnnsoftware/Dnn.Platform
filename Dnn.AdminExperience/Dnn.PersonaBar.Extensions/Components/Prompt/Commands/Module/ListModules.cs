// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Prompt.Components.Commands.Module
{
    using System.Linq;

    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.Prompt.Components.Models;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;

    using ModulesControllerLibrary = Dnn.PersonaBar.Library.Controllers.ModulesController;

    [ConsoleCommand("list-modules", Constants.ModulesCategory, "Prompt_ListModules_Description")]
    public class ListModules : ConsoleCommandBase
    {
        [FlagParameter("name", "Prompt_ListModules_FlagModuleName", "String")]
        private const string FlagModuleName = "name";

        [FlagParameter("title", "Prompt_ListModules_FlagModuleTitle", "String")]
        private const string FlagModuleTitle = "title";

        [FlagParameter("pageid", "Prompt_ListModules_FlagPageId", "Integer")]
        private const string FlagPageId = "pageid";

        [FlagParameter("deleted", "Prompt_ListModules_FlagDeleted", "Boolean")]
        private const string FlagDeleted = "deleted";

        [FlagParameter("page", "Prompt_ListModules_FlagPage", "Integer", "1")]
        private const string FlagPage = "page";

        [FlagParameter("max", "Prompt_ListModules_FlagMax", "Integer", "10")]
        private const string FlagMax = "max";

        public override string LocalResourceFile => Constants.LocalResourcesFile;

        private int? PageId { get; set; }
        private int Page { get; set; }
        private int Max { get; set; } = 10;
        private string ModuleName { get; set; }
        private string ModuleTitle { get; set; }

        private bool? Deleted { get; set; }
        //public string PageName { get; }

        public override void Init(string[] args, DotNetNuke.Entities.Portals.PortalSettings portalSettings, DotNetNuke.Entities.Users.UserInfo userInfo, int activeTabId)
        {

            this.PageId = this.GetFlagValue(FlagPageId, "Page Id", -1);
            this.ModuleName = this.GetFlagValue(FlagModuleName, "Module Name", string.Empty);
            this.ModuleTitle = this.GetFlagValue(FlagModuleTitle, "Module Title", string.Empty);
            this.Deleted = this.GetFlagValue<bool?>(FlagDeleted, "Deleted", null);
            this.Page = this.GetFlagValue(FlagPage, "Page No", 1);
            this.Max = this.GetFlagValue(FlagMax, "Page Size", 10);
        }

        public override ConsoleResultModel Run()
        {
            var max = this.Max <= 0 ? 10 : (this.Max > 500 ? 500 : this.Max);

            int total;
            var portalDesktopModules = DesktopModuleController.GetPortalDesktopModules(this.PortalId);
            var modules = ModulesControllerLibrary.Instance
                .GetModules(
                    this.PortalSettings,
                    this.Deleted,
                    out total, this.ModuleName,
                    this.ModuleTitle,
                    this.PageId, (this.Page > 0 ? this.Page - 1 : 0),
                    max)
                .Select(x => ModuleInfoModel.FromDnnModuleInfo(x, this.Deleted))
                .Where(m =>
                    {
                        var moduleDefinition = ModuleDefinitionController.GetModuleDefinitionByID(m.ModuleDefId);
                        return portalDesktopModules.Any(kvp =>
                            kvp.Value.DesktopModuleID == moduleDefinition?.DesktopModuleID);
                    })
                .ToList();
            var totalPages = total / max + (total % max == 0 ? 0 : 1);
            var pageNo = this.Page > 0 ? this.Page : 1;
            return new ConsoleResultModel
            {
                Data = modules,
                PagingInfo = new PagingInfo
                {
                    PageNo = pageNo,
                    TotalPages = totalPages,
                    PageSize = max
                },
                Records = modules.Count,
                Output = modules.Count == 0 ? this.LocalizeString("Prompt_NoModules") : ""
            };
        }
    }
}
