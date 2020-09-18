// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Components.Prompt.Commands
{
    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.Pages.Components.Exceptions;
    using Dnn.PersonaBar.Pages.Components.Security;
    using Dnn.PersonaBar.Pages.Services.Dto;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;

    [ConsoleCommand("delete-page", Constants.PagesCategory, "Prompt_DeletePage_Description")]
    public class DeletePage : ConsoleCommandBase
    {
        [FlagParameter("name", "Prompt_DeletePage_FlagName", "String")]
        private const string FlagName = "name";

        [FlagParameter("id", "Prompt_DeletePage_FlagId", "Integer")]
        private const string FlagId = "id";

        [FlagParameter("parentid", "Prompt_DeletePage_FlagParentId", "Integer")]
        private const string FlagParentId = "parentid";

        public override string LocalResourceFile => Constants.LocalResourceFile;

        private int PageId { get; set; } = -1;

        private string PageName { get; set; }

        private int ParentId { get; set; } = -1;

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            this.PageId = this.GetFlagValue(FlagId, "Page Id", -1, false, true);
            this.PageName = this.GetFlagValue(FlagName, "Page Name", string.Empty);
            this.ParentId = this.GetFlagValue(FlagParentId, "Parent Id", -1);

            if (this.PageId == -1 && string.IsNullOrEmpty(this.PageName))
            {
                this.AddMessage(this.LocalizeString("Prompt_ParameterRequired"));
            }
        }

        public override ConsoleResultModel Run()
        {
            this.PageId = this.PageId != -1
                ? this.PageId
                : (this.ParentId > 0 ? TabController.Instance.GetTabByName(this.PageName, this.PortalId, this.ParentId) : TabController.Instance.GetTabByName(this.PageName, this.PortalId))?.TabID ?? -1;

            if (this.PageId == -1)
            {
                return new ConsoleErrorResultModel(this.LocalizeString("Prompt_PageNotFound"));
            }
            if (!SecurityService.Instance.CanDeletePage(this.PageId))
            {
                return new ConsoleErrorResultModel(this.LocalizeString("MethodPermissionDenied"));
            }
            try
            {
                PagesController.Instance.DeletePage(new PageItem { Id = this.PageId }, this.PortalSettings);
            }
            catch (PageNotFoundException)
            {
                return new ConsoleErrorResultModel(this.LocalizeString("Prompt_PageNotFound"));
            }
            return new ConsoleResultModel(this.LocalizeString("PageDeletedMessage")) { Records = 1 };
        }
    }
}
