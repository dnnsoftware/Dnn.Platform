using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Pages.Components.Security;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Pages.Components.Prompt.Commands
{
    [ConsoleCommand("goto", Constants.PagesCategory, "Prompt_Goto_Description")]
    public class Goto : ConsoleCommandBase
    {
        public override string LocalResourceFile => Constants.LocalResourceFile;
        [FlagParameter("name", "Prompt_Goto_FlagName", "String")]
        private const string FlagName = "name";
        [FlagParameter("id", "Prompt_Goto_FlagId", "Integer")]
        private const string FlagId = "id";
        [FlagParameter("parentid", "Prompt_Goto_FlagParentId", "Integer")]
        private const string FlagParentId = "parentid";

        private int PageId { get; set; } = -1;
        private string PageName { get; set; }
        private int ParentId { get; set; } = -1;

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            
            PageId = GetFlagValue(FlagId, "Page Id", -1, false, true);
            ParentId = GetFlagValue(FlagParentId, "Parent Id", -1);
            PageName = GetFlagValue(FlagName, "Page Name", string.Empty);

            if (PageId == -1 && string.IsNullOrEmpty(PageName))
            {
                AddMessage(LocalizeString("Prompt_ParameterRequired"));
            }
        }

        public override ConsoleResultModel Run()
        {
            var tab = PageId > 0
                ? TabController.Instance.GetTab(PageId, PortalId)
                : (ParentId > 0
                    ? TabController.Instance.GetTabByName(PageName, PortalId, ParentId)
                    : TabController.Instance.GetTabByName(PageName, PortalId));

            if (tab == null)
            {
                return new ConsoleErrorResultModel(LocalizeString("Prompt_PageNotFound"));
            }
            if (!SecurityService.Instance.CanManagePage(PageId))
            {
                return new ConsoleErrorResultModel(LocalizeString("MethodPermissionDenied"));
            }
            return new ConsoleResultModel(tab.FullUrl) {MustReload = true};
        }
    }
}