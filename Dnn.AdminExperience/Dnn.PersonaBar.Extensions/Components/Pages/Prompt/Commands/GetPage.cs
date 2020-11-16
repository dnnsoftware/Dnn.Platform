// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Components.Prompt.Commands
{
    using System.Collections.Generic;

    using Dnn.PersonaBar.Library.Helper;
    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.Pages.Components.Prompt.Models;
    using Dnn.PersonaBar.Pages.Components.Security;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;

    [ConsoleCommand("get-page", Constants.PagesCategory, "Prompt_GetPage_Description")]
    public class GetPage : ConsoleCommandBase
    {
        [FlagParameter("name", "Prompt_GetPage_FlagName", "String")]
        private const string FlagName = "name";

        [FlagParameter("id", "Prompt_GetPage_FlagId", "Integer")]
        private const string FlagId = "id";

        [FlagParameter("parentid", "Prompt_GetPage_FlagParentId", "Integer")]
        private const string FlagParentId = "parentid";

        private readonly ITabController _tabController;
        private readonly ISecurityService _securityService;
        private readonly IContentVerifier _contentVerifier;

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
            var lst = new List<PageModel>();
            var tab = this.PageId != -1
                ? this._tabController.GetTab(this.PageId, this.PortalId)
                : (this.ParentId > 0
                    ? this._tabController.GetTabByName(this.PageName, this.PortalId, this.ParentId)
                    : this._tabController.GetTabByName(this.PageName, this.PortalId));

            if (tab != null)
            {
                if (!this._securityService.CanManagePage(this.PageId))
                {
                    return new ConsoleErrorResultModel(this.LocalizeString("MethodPermissionDenied"));
                }

                if (this._contentVerifier.IsContentExistsForRequestedPortal(tab.PortalID, this.PortalSettings))
                {
                    lst.Add(new PageModel(tab));
                    return new ConsoleResultModel { Data = lst, Records = lst.Count, Output = this.LocalizeString("Prompt_PageFound") };
                }
            }

            return new ConsoleErrorResultModel(this.LocalizeString("Prompt_PageNotFound"));
        }
    }
}
