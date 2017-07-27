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
            PageId = PageId != -1
                ? PageId
                : (ParentId > 0 ? TabController.Instance.GetTabByName(PageName, PortalId, ParentId) : TabController.Instance.GetTabByName(PageName, PortalId))?.TabID ?? -1;

            if (PageId == -1)
            {
                return new ConsoleErrorResultModel(LocalizeString("Prompt_PageNotFound"));
            }
            if (!SecurityService.Instance.CanDeletePage(PageId))
            {
                return new ConsoleErrorResultModel(LocalizeString("MethodPermissionDenied"));
            }
            try
            {
                PagesController.Instance.DeletePage(new PageItem { Id = PageId });
            }
            catch (PageNotFoundException)
            {
                return new ConsoleErrorResultModel(LocalizeString("Prompt_PageNotFound"));
            }
            return new ConsoleResultModel(LocalizeString("PageDeletedMessage")) { Records = 1 };
        }
    }
}