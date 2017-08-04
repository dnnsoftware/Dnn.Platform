using System.Linq;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Prompt.Components.Models;

namespace Dnn.PersonaBar.Prompt.Components.Commands.Module
{
    [ConsoleCommand("list-modules", Constants.ModulesCategory, "Prompt_ListModules_Description")]
    public class ListModules : ConsoleCommandBase
    {
        public override string LocalResourceFile => Constants.LocalResourcesFile;

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

        private int? PageId { get; set; }
        private int Page { get; set; }
        private int Max { get; set; } = 10;
        private string ModuleName { get; set; }
        private string ModuleTitle { get; set; }
        private bool? Deleted { get; set; }
        //public string PageName { get; }


        public override void Init(string[] args, DotNetNuke.Entities.Portals.PortalSettings portalSettings, DotNetNuke.Entities.Users.UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            PageId = GetFlagValue(FlagPageId, "Page Id", -1);
            ModuleName = GetFlagValue(FlagModuleName, "Module Name", string.Empty);
            ModuleTitle = GetFlagValue(FlagModuleTitle, "Module Title", string.Empty);
            Deleted = GetFlagValue<bool?>(FlagDeleted, "Deleted", null);
            Page = GetFlagValue(FlagPage, "Page No", 1);
            Max = GetFlagValue(FlagMax, "Page Size", 10);
        }

        public override ConsoleResultModel Run()
        {
            var max = Max <= 0 ? 10 : (Max > 500 ? 500 : Max);

            int total;
            var modules =
                ModulesController.Instance.GetModules(PortalSettings, Deleted, out total, ModuleName, ModuleTitle,
                    PageId, (Page > 0 ? Page - 1 : 0), max).Select(x => ModuleInfoModel.FromDnnModuleInfo(x, Deleted)).ToList();
            var totalPages = total / max + (total % max == 0 ? 0 : 1);
            var pageNo = Page > 0 ? Page : 1;
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
                Output = modules.Count == 0 ? LocalizeString("Prompt_NoModules") : ""
            };
        }
    }
}