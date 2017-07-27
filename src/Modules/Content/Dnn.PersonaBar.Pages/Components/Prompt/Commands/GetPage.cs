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
        protected override string LocalResourceFile => Constants.LocalResourceFile;

        private const string FlagName = "name";
        private const string FlagId = "id";
        private const string FlagParentId = "parentid";

        private int PageId { get; set; } = -1;
        private string PageName { get; set; }
        private int ParentId { get; set; } = -1;

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            PageId = GetFlagValue(FlagId, "Page Id", -1, false, true);
            PageName = GetFlagValue(FlagName, "Page Name", string.Empty);
            ParentId = GetFlagValue(FlagParentId, "Parent Id", -1);
            if (PageId == -1 && string.IsNullOrEmpty(PageName))
            {
                AddMessage(LocalizeString("Prompt_ParameterRequired"));
            }
        }

        public override ConsoleResultModel Run()
        {
            var lst = new List<PageModel>();
            var tab = PageId != -1
                ? TabController.Instance.GetTab(PageId, PortalId)
                : (ParentId > 0 ? TabController.Instance.GetTabByName(PageName, PortalId, ParentId) : TabController.Instance.GetTabByName(PageName, PortalId));

            if (tab == null)
            {
                return new ConsoleErrorResultModel(LocalizeString("Prompt_PageNotFound"));
            }
            if (!SecurityService.Instance.CanManagePage(PageId))
            {
                return new ConsoleErrorResultModel(LocalizeString("MethodPermissionDenied"));
            }

            lst.Add(new PageModel(tab));
            return new ConsoleResultModel { Data = lst, Records = lst.Count };
        }
    }
}