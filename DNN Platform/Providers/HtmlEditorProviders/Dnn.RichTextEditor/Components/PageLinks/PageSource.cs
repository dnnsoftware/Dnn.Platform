// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.RichTextEditorProvider.Components.PageLinks
{
    using System.Collections.Generic;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;

    /// <summary>Gets pages through <see cref="TabController"/>, applying the current user's view permissions.</summary>
    public class PageSource : IPageSource
    {
        private readonly IHostSettings hostSettings;
        private readonly IApplicationStatusInfo appStatus;
        private readonly IPortalController portalController;

        /// <summary>Initializes a new instance of the <see cref="PageSource"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="portalController">The portal controller.</param>
        public PageSource(IHostSettings hostSettings, IApplicationStatusInfo appStatus, IPortalController portalController)
        {
            this.hostSettings = hostSettings;
            this.appStatus = appStatus;
            this.portalController = portalController;
        }

        /// <inheritdoc/>
        public IEnumerable<TabInfo> GetViewablePages(int portalId)
        {
            // Pages in the site's current language, including hidden and link-type pages but not deleted ones,
            // that the user can view. Host pages are only included for superusers.
            return TabController.GetPortalTabs(
                this.hostSettings,
                this.appStatus,
                portalId,
                excludeTabId: Null.NullInteger,
                includeNoneSpecified: false,
                noneSpecifiedText: null,
                includeHidden: true,
                includeDeleted: false,
                includeUrl: true,
                checkViewPermission: true,
                checkEditPermission: false);
        }

        /// <inheritdoc/>
        public string GetUrl(TabInfo page)
        {
            // Same URL the CKEditor provider's page links used, so new links match existing content.
            var path = Globals.ApplicationURL(page.TabID);
            if (!string.IsNullOrEmpty(page.CultureCode)
                && PortalController.GetPortalSettingAsBoolean(this.portalController, "ContentLocalizationEnabled", page.PortalID, false))
            {
                path += "&language=" + page.CultureCode;
            }

            return Globals.FriendlyUrl(page, path);
        }
    }
}
