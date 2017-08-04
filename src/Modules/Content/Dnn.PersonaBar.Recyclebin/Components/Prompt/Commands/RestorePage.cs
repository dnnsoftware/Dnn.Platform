using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Recyclebin.Components.Prompt.Commands
{
    [ConsoleCommand("restore-page", Constants.RecylcleBinCategory, "Prompt_RestorePage_Description")]
    public class RestorePage : ConsoleCommandBase
    {
        public override string LocalResourceFile => Constants.LocalResourcesFile;

        [FlagParameter("name", "Prompt_RestorePage_FlagName", "String")]
        private const string FlagName = "name";

        [FlagParameter("parentid", "Prompt_RestorePage_FlagParentId", "Integer")]
        private const string FlagParentId = "parentid";

        [FlagParameter("id", "Prompt_RestorePage_FlagId", "Integer")]
        private const string FlagId = "id";

        private int PageId { get; set; }
        private string PageName { get; set; }
        private int ParentId { get; set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            PageId = GetFlagValue(FlagId, "Page Id", -1, false, true);
            ParentId = GetFlagValue(FlagParentId, "Parent Id", -1);
            PageName = GetFlagValue(FlagName, "Page Name", string.Empty);
            if (PageId <= 0 && string.IsNullOrEmpty(PageName))
            {
                AddMessage(LocalizeString("Prompt_RestorePageNoParams"));
            }
        }

        public override ConsoleResultModel Run()
        {
            TabInfo tab;
            if (PageId > 0)
            {
                tab = TabController.Instance.GetTab(PageId, PortalId);
                if (tab == null)
                {
                    return new ConsoleErrorResultModel(string.Format(LocalizeString("PageNotFound"), PageId));
                }
            }
            else if (!string.IsNullOrEmpty(PageName))
            {
                tab = ParentId > 0 ? TabController.Instance.GetTabByName(PageName, PortalId, ParentId) : TabController.Instance.GetTabByName(PageName, PortalId);
                if (tab == null)
                    return
                        new ConsoleErrorResultModel(
                            string.Format(
                                LocalizeString("PageNotFoundWithName"),
                                PageName));
            }
            else
            {
                return new ConsoleErrorResultModel(LocalizeString("Prompt_RestorePageNoParams"));
            }
            string message;
            RecyclebinController.Instance.RestoreTab(tab, out message);
            return string.IsNullOrEmpty(message) ? new ConsoleResultModel(string.Format(LocalizeString("Prompt_PageRestoredSuccessfully"), tab.TabID, tab.TabName)) { Records = 1 } : new ConsoleErrorResultModel(message);
        }
    }
}