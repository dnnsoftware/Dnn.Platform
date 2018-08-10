using Dnn.PersonaBar.Library.Helper;
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

        private readonly ITabController _tabController;
        private readonly IContentVerifier _contentVerifier;
        private readonly IRecyclebinController _recyclebinController;

        private int PageId { get; set; }
        private string PageName { get; set; }
        private int ParentId { get; set; }

        public RestorePage() : this(TabController.Instance, RecyclebinController.Instance, new ContentVerifier())
        {
        }

        public RestorePage(ITabController tabController, IRecyclebinController recyclebinController, IContentVerifier contentVerifier)
        {
            this._tabController = tabController;
            this._recyclebinController = recyclebinController;
            this._contentVerifier = contentVerifier;
        }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
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
            string message = string.Format(LocalizeString("PageNotFound"), PageId);

            if (PageId > 0)
            {
                tab = _tabController.GetTab(PageId, PortalId);
                if (tab == null)
                {
                    return new ConsoleErrorResultModel(message);
                }
            }
            else if (!string.IsNullOrEmpty(PageName))
            {
                tab = ParentId > 0
                    ? _tabController.GetTabByName(PageName, PortalId, ParentId)
                    : _tabController.GetTabByName(PageName, PortalId);

                if (tab == null)
                {
                    message = string.Format(LocalizeString("PageNotFoundWithName"), PageName);
                    return new ConsoleErrorResultModel(message);
                }
            }
            else
            {
                return new ConsoleErrorResultModel(LocalizeString("Prompt_RestorePageNoParams"));
            }

            if (!_contentVerifier.IsContentExistsForRequestedPortal(tab.PortalID, PortalSettings))
            {
                return new ConsoleErrorResultModel(message);
            }

            _recyclebinController.RestoreTab(tab, out message);

            if (string.IsNullOrEmpty(message))
            {
                var successMessage = string.Format(
                    LocalizeString("Prompt_PageRestoredSuccessfully"),
                    tab.TabID,
                    tab.TabName
                    );
                return new ConsoleResultModel(successMessage) { Records = 1 };
            }
            else
            {
                return new ConsoleErrorResultModel(message);
            }
        }
    }
}