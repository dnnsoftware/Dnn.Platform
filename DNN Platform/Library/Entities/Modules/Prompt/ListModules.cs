// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules.Prompt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Abstractions.Prompt;
    using DotNetNuke.Abstractions.Users;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Prompt;
    using DotNetNuke.Security.Permissions;

    /// <summary>This is a (Prompt) Console Command. You should not reference this class directly. It is to be used solely through Prompt.</summary>
    [ConsoleCommand("list-modules", Constants.CommandCategoryKeys.Modules, "Prompt_ListModules_Description")]
    public class ListModules : ConsoleCommand
    {
        /// <inheritdoc/>
        public override string LocalResourceFile => Constants.DefaultPromptResourceFile;

        [ConsoleCommandParameter("pageid", "Prompt_ListModules_FlagPageId")]
        public int? PageId { get; set; } = -1;

        [ConsoleCommandParameter("page", "Prompt_ListModules_FlagPage", "1")]
        public int Page { get; set; } = 1;

        [ConsoleCommandParameter("max", "Prompt_ListModules_FlagMax", "10")]
        public int Max { get; set; } = 10;

        [ConsoleCommandParameter("name", "Prompt_ListModules_FlagModuleName")]
        public string ModuleName { get; set; }

        [ConsoleCommandParameter("title", "Prompt_ListModules_FlagModuleTitle")]
        public string ModuleTitle { get; set; }

        [ConsoleCommandParameter("deleted", "Prompt_ListModules_FlagDeleted")]
        public bool? Deleted { get; set; }

        /// <inheritdoc/>
        public override void Initialize(string[] args, IPortalSettings portalSettings, IUserInfo userInfo, int activeTabId)
        {
            base.Initialize(args, portalSettings, userInfo, activeTabId);
            this.ParseParameters(this);
        }

        /// <inheritdoc/>
        public override IConsoleResultModel Run()
        {
            var max = this.Max <= 0 ? 10 : (this.Max > 500 ? 500 : this.Max);

            int total;
            var portalDesktopModules = DesktopModuleController.GetPortalDesktopModules(this.PortalId);

            var pageIndex = this.Page > 0 ? this.Page - 1 : 0;
            pageIndex = pageIndex < 0 ? 0 : pageIndex;
            var pageSize = this.Max;
            pageSize = pageSize > 0 && pageSize <= 100 ? pageSize : 10;
            this.ModuleName = this.ModuleName?.Replace("*", string.Empty);
            this.ModuleTitle = this.ModuleTitle?.Replace("*", string.Empty);
            var modules = ModuleController.Instance.GetModules(this.PortalSettings.PortalId)
                    .Cast<ModuleInfo>().Where(ModulePermissionController.CanViewModule);
            if (!string.IsNullOrEmpty(this.ModuleName))
            {
                modules = modules.Where(module => module.DesktopModule.ModuleName.IndexOf(this.ModuleName, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (!string.IsNullOrEmpty(this.ModuleTitle))
            {
                modules = modules.Where(module => module.ModuleTitle.IndexOf(this.ModuleTitle, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            // Return only deleted modules with matching criteria.
            if (this.PageId.HasValue && this.PageId.Value > 0)
            {
                modules = modules.Where(x => x.TabID == this.PageId.Value);
            }

            if (this.Deleted.HasValue)
            {
                modules = modules.Where(module => module.IsDeleted == this.Deleted);
            }

            // Get distincts.
            modules = modules.GroupBy(x => x.ModuleID).Select(group => group.First()).OrderBy(x => x.ModuleID);
            var moduleInfos = modules as IList<ModuleInfo> ?? modules.ToList();
            total = moduleInfos.Count;
            modules = moduleInfos.Skip(pageIndex * pageSize).Take(pageSize)
                .Where(m =>
                {
                    var moduleDefinition = ModuleDefinitionController.GetModuleDefinitionByID(m.ModuleDefID);
                    return portalDesktopModules.Any(kvp =>
                        kvp.Value.DesktopModuleID == moduleDefinition?.DesktopModuleID);
                });

            var results = modules.Select(x => PromptModuleInfo.FromDnnModuleInfo(x, this.Deleted));

            var totalPages = (total / max) + (total % max == 0 ? 0 : 1);
            var pageNo = this.Page > 0 ? this.Page : 1;
            return new ConsoleResultModel
            {
                Data = results,
                PagingInfo = new PagingInfo
                {
                    PageNo = pageNo,
                    TotalPages = totalPages,
                    TotalRecords = total,
                    PageSize = max,
                },
                Records = results.Count(),
                Output = results.Count() == 0 ? this.LocalizeString("Prompt_NoModules") : string.Empty,
            };
        }
    }
}
