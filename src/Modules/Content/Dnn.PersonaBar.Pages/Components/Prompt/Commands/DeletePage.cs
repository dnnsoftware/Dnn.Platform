using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Pages.Components.Exceptions;
using Dnn.PersonaBar.Pages.Components.Security;
using Dnn.PersonaBar.Pages.Services.Dto;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Pages.Components.Prompt.Commands
{
    [ConsoleCommand("delete-page", "Deletes the specified page", new[]{
        "id",
        "parentid",
        "name"
    })]
    public class DeletePage : ConsoleCommandBase
    {
        private const string FlagName = "name";
        private const string FlagId = "id";
        private const string FlagParentId = "parentid";

        private int PageId { get; set; } = -1;
        private string PageName { get; set; }
        private int ParentId { get; set; } = -1;

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            var sbErrors = new StringBuilder();

            if (args.Length == 2)
            {
                int tmpId;
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
                sbErrors.Append(DotNetNuke.Services.Localization.Localization.GetString("Prompt_ParameterRequired", Constants.LocalResourceFile));
            }
            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {
            PageId = PageId != -1
                ? PageId
                : (ParentId > 0 ? TabController.Instance.GetTabByName(PageName, PortalId, ParentId) : TabController.Instance.GetTabByName(PageName, PortalId))?.TabID ?? -1;

            if (PageId == -1)
            {
                return new ConsoleErrorResultModel(DotNetNuke.Services.Localization.Localization.GetString("Prompt_PageNotFound", Constants.LocalResourceFile));
            }
            if (!SecurityService.Instance.CanDeletePage(PageId))
            {
                return new ConsoleErrorResultModel(DotNetNuke.Services.Localization.Localization.GetString("MethodPermissionDenied", Constants.LocalResourceFile));
            }
            try
            {
                PagesController.Instance.DeletePage(new PageItem { Id = PageId });
            }
            catch (PageNotFoundException)
            {
                return new ConsoleErrorResultModel(DotNetNuke.Services.Localization.Localization.GetString("Prompt_PageNotFound", Constants.LocalResourceFile));
            }
            return new ConsoleResultModel(DotNetNuke.Services.Localization.Localization.GetString("PageDeletedMessage", Constants.LocalResourceFile)) { Records = 1 };
        }
    }
}