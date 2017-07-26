using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;

namespace Dnn.PersonaBar.Recyclebin.Components.Prompt.Commands
{
    [ConsoleCommand("restore-page", "Restores a previously deleted page", new[]{
        "id",
        "name",
        "parentid"
    })]
    public class RestorePage : ConsoleCommandBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RestorePage));

        private const string FlagName = "name";
        private const string FlagParentid = "parentid";
        private const string FlagId = "id";


        private int PageId { get; set; }
        private string PageName { get; set; }
        private int ParentId { get; set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            var sbErrors = new StringBuilder();

            if (args.Length == 2)
            {
                int tmpId;
                if (!int.TryParse(args[1], out tmpId))
                {
                    sbErrors.AppendFormat(Localization.GetString("Prompt_FlagNotInt", Constants.LocalResourcesFile), FlagId);
                }
                else
                {
                    PageId = tmpId;
                }
            }
            else if (HasFlag(FlagId))
            {
                int tmpId;
                if (!int.TryParse(Flag(FlagId), out tmpId))
                {
                    sbErrors.AppendFormat(Localization.GetString("Prompt_FlagNotInt", Constants.LocalResourcesFile), FlagId);
                }
                else
                {
                    PageId = tmpId;
                }
            }

            if (HasFlag(FlagParentid))
            {
                int tmpId;
                if (int.TryParse(Flag(FlagParentid), out tmpId))
                    ParentId = tmpId;
            }

            PageName = Flag(FlagName);

            if (PageId <= 0 && string.IsNullOrEmpty(PageName))
            {
                sbErrors.Append(Localization.GetString("Prompt_RestorePageNoParams", Constants.LocalResourcesFile));
            }
            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {
            TabInfo tab;
            if (PageId > 0)
            {
                tab = TabController.Instance.GetTab(PageId, PortalId);
                if (tab == null)
                {
                    return new ConsoleErrorResultModel(string.Format(Localization.GetString("PageNotFound", Constants.LocalResourcesFile), PageId));
                }
            }
            else if (!string.IsNullOrEmpty(PageName))
            {
                tab = ParentId > 0 ? TabController.Instance.GetTabByName(PageName, PortalId, ParentId) : TabController.Instance.GetTabByName(PageName, PortalId);
                if (tab == null)
                    return
                        new ConsoleErrorResultModel(
                            string.Format(
                                Localization.GetString("PageNotFoundWithName", Constants.LocalResourcesFile),
                                PageName));
            }
            else
            {
                return new ConsoleErrorResultModel(Localization.GetString("Prompt_RestorePageNoParams", Constants.LocalResourcesFile));
            }
            string message;
            RecyclebinController.Instance.RestoreTab(tab, out message);
            return string.IsNullOrEmpty(message) ? new ConsoleResultModel(string.Format(Localization.GetString("Prompt_PageRestoredSuccessfully", Constants.LocalResourcesFile), tab.TabID, tab.TabName)) { Records = 1 } : new ConsoleErrorResultModel(message);
        }


    }
}