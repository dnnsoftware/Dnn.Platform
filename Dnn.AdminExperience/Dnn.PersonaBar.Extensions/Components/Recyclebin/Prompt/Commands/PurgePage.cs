// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Recyclebin.Components.Prompt.Commands;

using System.Collections.Generic;
using System.Text;

using Dnn.PersonaBar.Library.Helper;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;

[ConsoleCommand("purge-page", Constants.RecylcleBinCategory, "Prompt_PurgePage_Description")]

public class PurgePage : ConsoleCommandBase
{
    [FlagParameter("id", "Prompt_PurgePage_FlagId", "Integer", true)]

    private const string FlagId = "id";

    [FlagParameter("deletechildren", "Prompt_PurgePage_FlagDeleteChildren", "Boolean", "false")]

    private const string FlagDeleteChildren = "deletechildren";

    private readonly ITabController tabController;
    private readonly IRecyclebinController recyclebinController;
    private readonly IContentVerifier contentVerifier;

    /// <summary>Initializes a new instance of the <see cref="PurgePage"/> class.</summary>
    public PurgePage()
        : this(
            TabController.Instance,
            RecyclebinController.Instance,
            new ContentVerifier())
    {
    }

    /// <summary>Initializes a new instance of the <see cref="PurgePage"/> class.</summary>
    /// <param name="tabController"></param>
    /// <param name="recyclebinController"></param>
    /// <param name="contentVerifier"></param>
    public PurgePage(ITabController tabController, IRecyclebinController recyclebinController, IContentVerifier contentVerifier)
    {
        this.tabController = tabController;
        this.recyclebinController = recyclebinController;
        this.contentVerifier = contentVerifier;
    }

    /// <inheritdoc/>
    public override string LocalResourceFile => Constants.LocalResourcesFile;

    private int PageId { get; set; }

    private bool DeleteChildren { get; set; }

    /// <inheritdoc/>
    public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
    {
        this.PageId = this.GetFlagValue(FlagId, "Page Id", -1, true, true, true);
        this.DeleteChildren = this.GetFlagValue(FlagDeleteChildren, "Delete Children", false);
    }

    /// <inheritdoc/>
    public override ConsoleResultModel Run()
    {
        var tabInfo = this.tabController.GetTab(this.PageId, this.PortalSettings.PortalId);
        if (tabInfo == null ||
            !this.contentVerifier.IsContentExistsForRequestedPortal(tabInfo.PortalID, this.PortalSettings))
        {
            return new ConsoleErrorResultModel(string.Format(this.LocalizeString("PageNotFound"), this.PageId));
        }

        var errors = new StringBuilder();
        this.recyclebinController.DeleteTabs(new List<TabInfo> { tabInfo }, errors, this.DeleteChildren);

        return errors.Length > 0
            ? new ConsoleErrorResultModel(string.Format(this.LocalizeString("Service_RemoveTabError"), errors))
            : new ConsoleResultModel(string.Format(this.LocalizeString("Prompt_PagePurgedSuccessfully"), this.PageId)) { Records = 1 };
    }
}
