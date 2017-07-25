using System.Linq;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Prompt.Components.Models;
using DotNetNuke.Services.Localization;

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

        private const string FlagModuleName = "name";
        private const string FlagModuleTitle = "title";
        private const string FlagPageid = "pageid";
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
            var sbErrors = new StringBuilder();

            if (HasFlag(FlagPageid))
            {
                int tmpId;
                if (int.TryParse(Flag(FlagPageid), out tmpId))
                {
                    PageId = tmpId;
                }
                else
                {
                    sbErrors.AppendFormat(Localization.GetString("Prompt_FlagNotInt", Constants.LocalResourcesFile), FlagPageid);
                }
            }
            else
            {
                if (args.Length >= 2 && !IsFlag(args[1]))
                {
                    // attempt to parse first arg as the page ID
                    int tmpId;
                    if (int.TryParse(args[1], out tmpId))
                    {
                        PageId = tmpId;
                    }
                    else
                    {
                        sbErrors.AppendFormat(Localization.GetString("Prompt_InvalidFlagValue", Constants.LocalResourcesFile), FlagPageid);
                    }
                }
            }

            ModuleName = Flag(FlagModuleName);
            ModuleTitle = Flag(FlagModuleTitle);
            if (HasFlag(FlagDeleted))
            {
                bool tmp;
                if (bool.TryParse(Flag(FlagDeleted), out tmp))
                {
                    Deleted = tmp;
                }
                else
                {
                    if (Flag(FlagDeleted, null) == null)
                    {
                        // user specified deleted flag with no value. Default to True
                        Deleted = true;
                    }
                }
            }
            if (HasFlag(FlagPage))
            {
                int tmpId;
                if (int.TryParse(Flag(FlagPage), out tmpId))
                    Page = tmpId;
            }
            if (HasFlag(FlagMax))
            {
                int tmpId;
                if (int.TryParse(Flag(FlagMax), out tmpId))
                    Max = tmpId > 0 && tmpId < 500 ? tmpId : Max;
            }

            ValidationMessage = sbErrors.ToString();
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
                        ? Localization.GetString("Prompt_ListModulesOutput", Constants.LocalResourcesFile)
                        : Localization.GetString("Prompt_NoModules", Constants.LocalResourcesFile)
            };
        }
    }
}