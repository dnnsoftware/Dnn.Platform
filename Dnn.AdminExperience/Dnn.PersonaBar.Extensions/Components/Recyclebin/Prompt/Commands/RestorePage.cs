// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Recyclebin.Components.Prompt.Commands
{
    using Dnn.PersonaBar.Library.Helper;
    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;

    [ConsoleCommand("restore-page", Constants.RecylcleBinCategory, "Prompt_RestorePage_Description")]
    public class RestorePage : ConsoleCommandBase
    {
        [FlagParameter("name", "Prompt_RestorePage_FlagName", "String")]
        private const string FlagName = "name";

        [FlagParameter("parentid", "Prompt_RestorePage_FlagParentId", "Integer")]
        private const string FlagParentId = "parentid";

        [FlagParameter("id", "Prompt_RestorePage_FlagId", "Integer")]
        private const string FlagId = "id";

        private readonly ITabController _tabController;
        private readonly IContentVerifier _contentVerifier;
        private readonly IRecyclebinController _recyclebinController;

        public RestorePage() : this(TabController.Instance, RecyclebinController.Instance, new ContentVerifier())
        {
        }

        public RestorePage(ITabController tabController, IRecyclebinController recyclebinController, IContentVerifier contentVerifier)
        {
            this._tabController = tabController;
            this._recyclebinController = recyclebinController;
            this._contentVerifier = contentVerifier;
        }

        public override string LocalResourceFile => Constants.LocalResourcesFile;

        private int PageId { get; set; }
        private string PageName { get; set; }
        private int ParentId { get; set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            this.PageId = this.GetFlagValue(FlagId, "Page Id", -1, false, true);
            this.ParentId = this.GetFlagValue(FlagParentId, "Parent Id", -1);
            this.PageName = this.GetFlagValue(FlagName, "Page Name", string.Empty);
            if (this.PageId <= 0 && string.IsNullOrEmpty(this.PageName))
            {
                this.AddMessage(this.LocalizeString("Prompt_RestorePageNoParams"));
            }
        }

        public override ConsoleResultModel Run()
        {
            TabInfo tab;
            string message = string.Format(this.LocalizeString("PageNotFound"), this.PageId);

            if (this.PageId > 0)
            {
                tab = this._tabController.GetTab(this.PageId, this.PortalId);
                if (tab == null)
                {
                    return new ConsoleErrorResultModel(message);
                }
            }
            else if (!string.IsNullOrEmpty(this.PageName))
            {
                tab = this.ParentId > 0
                    ? this._tabController.GetTabByName(this.PageName, this.PortalId, this.ParentId)
                    : this._tabController.GetTabByName(this.PageName, this.PortalId);

                if (tab == null)
                {
                    message = string.Format(this.LocalizeString("PageNotFoundWithName"), this.PageName);
                    return new ConsoleErrorResultModel(message);
                }
            }
            else
            {
                return new ConsoleErrorResultModel(this.LocalizeString("Prompt_RestorePageNoParams"));
            }

            if (!this._contentVerifier.IsContentExistsForRequestedPortal(tab.PortalID, this.PortalSettings))
            {
                return new ConsoleErrorResultModel(message);
            }

            this._recyclebinController.RestoreTab(tab, out message);

            if (string.IsNullOrEmpty(message))
            {
                var successMessage = string.Format(
                    this.LocalizeString("Prompt_PageRestoredSuccessfully"),
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
