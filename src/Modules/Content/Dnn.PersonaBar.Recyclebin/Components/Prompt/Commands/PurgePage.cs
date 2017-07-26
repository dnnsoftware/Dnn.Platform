using System.Collections.Generic;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;

namespace Dnn.PersonaBar.Recyclebin.Components.Prompt.Commands
{
    [ConsoleCommand("purge-page", "Permanently deletes a page from the DNN Recycle Bin", new[]{
        "id",
        "deletechildren"
    })]
    public class PurgePage : ConsoleCommandBase
    {
        private const string FlagId = "id";
        private const string FlagDeleteChildren = "deletechildren";

        private int PageId { get; set; }
        private bool DeleteChildren { get; set; }

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

            if (HasFlag(FlagDeleteChildren))
            {
                bool tmpId;
                if (bool.TryParse(Flag(FlagDeleteChildren), out tmpId))
                {
                    DeleteChildren = tmpId;
                }
            }

            if (PageId <= 0)
                sbErrors.AppendFormat(Localization.GetString("Prompt_FlagNotPositiveInt", Constants.LocalResourcesFile), FlagId);

            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {
            var tabInfo = TabController.Instance.GetTab(PageId, PortalSettings.PortalId);
            if (tabInfo == null)
            {
                return new ConsoleErrorResultModel(string.Format(Localization.GetString("PageNotFound", Constants.LocalResourcesFile), PageId));
            }
            var errors = new StringBuilder();
            RecyclebinController.Instance.DeleteTabs(new List<TabInfo> { tabInfo }, errors, DeleteChildren);
            return errors.Length > 0
                ? new ConsoleErrorResultModel(string.Format(Localization.GetString("Service_RemoveTabError", Constants.LocalResourcesFile), errors))
                : new ConsoleResultModel(string.Format(Localization.GetString("Prompt_PagePurgedSuccessfully", Constants.LocalResourcesFile), PageId)) { Records = 1 };
        }
    }
}