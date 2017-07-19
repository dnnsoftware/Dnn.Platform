using System.Collections.Generic;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Pages.Components.Prompt.Models;
using Dnn.PersonaBar.Pages.Components.Security;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Pages.Components.Prompt.Commands
{
    [ConsoleCommand("get-page", "Retrieves information about the specified or current page", new[]{
        "id",
        "parentid",
        "name"
    })]
    public class GetPage : ConsoleCommandBase
    {
        private const string FlagName = "name";
        private const string FlagId = "id";
        private const string FlagParentId = "parentid";

        int PageId { get; set; } = -1;
        string PageName { get; set; }
        int ParentId { get; set; } = -1;

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            var sbErrors = new StringBuilder();

            // default usage: return current page if nothing else specified
            if (args.Length == 1)
            {
                PageId = TabId;
            }
            else if (args.Length == 2)
            {
                var tmpId = 0;
                if (!int.TryParse(args[1], out tmpId))
                {
                    sbErrors.Append(DotNetNuke.Services.Localization.Localization.GetString("Prompt_NoPageId", Constants.LocalResourceFile));
                }
                else
                {
                    PageId = tmpId;
                }
            }
            else
            {
                if (HasFlag(FlagId))
                {
                    int tmpId;
                    if (!int.TryParse(Flag(FlagId), out tmpId))
                    {
                        sbErrors.Append(DotNetNuke.Services.Localization.Localization.GetString("Prompt_InvalidPageId", Constants.LocalResourceFile));
                    }
                    else
                    {
                        PageId = tmpId;
                    }
                }
            }

            PageName = Flag(FlagName);
            if (HasFlag(FlagParentId))
            {
                int tmpId;
                if (int.TryParse(Flag(FlagParentId), out tmpId))
                    ParentId = tmpId;
            }

            if (PageId == -1 && string.IsNullOrEmpty(PageName))
            {
                sbErrors.AppendFormat(DotNetNuke.Services.Localization.Localization.GetString("Prompt_ParameterRequired", Constants.LocalResourceFile), "Page ID, Page Name");
            }
            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {
            var lst = new List<PageModel>();
            var tab = PageId != -1
                ? TabController.Instance.GetTab(PageId, PortalId)
                : (ParentId > 0 ? TabController.Instance.GetTabByName(PageName, PortalId, ParentId) : TabController.Instance.GetTabByName(PageName, PortalId));

            if (tab == null)
            {
                return new ConsoleErrorResultModel(DotNetNuke.Services.Localization.Localization.GetString("Prompt_PageNotFound", Constants.LocalResourceFile));
            }
            if (!SecurityService.Instance.CanManagePage(PageId))
            {
                return new ConsoleErrorResultModel(DotNetNuke.Services.Localization.Localization.GetString("MethodPermissionDenied", Constants.LocalResourceFile));
            }

            lst.Add(new PageModel(tab));
            return new ConsoleResultModel { Data = lst };
        }
    }
}