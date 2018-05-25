using System.Collections.Generic;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Pages.Components.Prompt.Models;
using Dnn.PersonaBar.Pages.Components.Security;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using Dnn.PersonaBar.Library.Helper;

namespace Dnn.PersonaBar.Pages.Components.Prompt.Commands
{
    [ConsoleCommand("get-page", Constants.PagesCategory, "Prompt_GetPage_Description")]
    public class GetPage : ConsoleCommandBase
    {
        public override string LocalResourceFile => Constants.LocalResourceFile;

        [FlagParameter("name", "Prompt_GetPage_FlagName", "String")]
        private const string FlagName = "name";
        [FlagParameter("id", "Prompt_GetPage_FlagId", "Integer")]
        private const string FlagId = "id";
        [FlagParameter("parentid", "Prompt_GetPage_FlagParentId", "Integer")]
        private const string FlagParentId = "parentid";

        private readonly ITabController _tabController;
        private readonly ISecurityService _securityService;
        private readonly IContentVerifier _contentVerifier;

        private int PageId { get; set; } = -1;
        private string PageName { get; set; }
        private int ParentId { get; set; } = -1;

        public GetPage() :
            this(TabController.Instance, SecurityService.Instance, new ContentVerifier())
        {
        }

        public GetPage(
            ITabController tabController,
            ISecurityService securityService,
            IContentVerifier contentVerifier
            )
        {
            this._tabController = tabController;
            this._securityService = securityService;
            this._contentVerifier = contentVerifier;
        }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {

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
                ? _tabController.GetTab(PageId, PortalId)
                : (ParentId > 0
                    ? _tabController.GetTabByName(PageName, PortalId, ParentId)
                    : _tabController.GetTabByName(PageName, PortalId));

            if (tab != null)
            {
                if (!_securityService.CanManagePage(PageId))
                {
                    return new ConsoleErrorResultModel(LocalizeString("MethodPermissionDenied"));
                }

                if (_contentVerifier.IsContentExistsForRequestedPortal(tab.PortalID, PortalSettings))
                {
                    lst.Add(new PageModel(tab));
                    return new ConsoleResultModel { Data = lst, Records = lst.Count, Output = LocalizeString("Prompt_PageFound") };
                }
            }

            return new ConsoleErrorResultModel(LocalizeString("Prompt_PageNotFound"));
        }
    }
}