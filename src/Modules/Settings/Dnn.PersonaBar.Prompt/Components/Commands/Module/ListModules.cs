using System.Linq;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Prompt.Components.Models;

namespace Dnn.PersonaBar.Prompt.Components.Commands.Module
{
    [ConsoleCommand("list-modules", "Lists modules on current page", new[]{
        "name",
        "title",
        "pageid",
        "page",
        "max"
    })]
    public class ListModules : ConsoleCommandBase
    {
        protected override string LocalResourceFile => Constants.LocalResourcesFile;

        private const string FlagModuleName = "name";
        private const string FlagModuleTitle = "title";
        private const string FlagPageId = "pageid";
        private const string FlagDeleted = "deleted";
        private const string FlagPage = "page";
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
            PageId = GetFlagValue(FlagPageId, "Page Id", -1, false, true);
            ModuleName = GetFlagValue(FlagModuleName, "Module Name", string.Empty);
            ModuleTitle = GetFlagValue(FlagModuleTitle, "Module Title", string.Empty);
            Deleted = GetFlagValue<bool?>(FlagDeleted, "Deleted", null);
            Page = GetFlagValue(FlagPage, "Page No", 1);
            Max = GetFlagValue(FlagMax, "Page Size", 10);
        }

        public override ConsoleResultModel Run()
        {
            int total;
            var modules =
                ModulesController.Instance.GetModules(PortalSettings, Deleted, out total, ModuleName, ModuleTitle,
                    PageId, (Page > 0 ? Page - 1 : 0), Max).Select(x => ModuleInfoModel.FromDnnModuleInfo(x, Deleted)).ToList();
            var totalPages = total / Max + (total % Max == 0 ? 0 : 1);
            var pageNo = Page > 0 ? Page : 1;
            return new ConsoleResultModel
            {
                Data = modules,
                PagingInfo = new PagingInfo
                {
                    PageNo = pageNo,
                    TotalPages = totalPages,
                    PageSize = Max
                },
                Records = modules.Count,
                Output =
                    pageNo <= totalPages
                        ? LocalizeString("Prompt_ListModulesOutput")
                        : LocalizeString("Prompt_NoModules")
            };
        }
    }
}