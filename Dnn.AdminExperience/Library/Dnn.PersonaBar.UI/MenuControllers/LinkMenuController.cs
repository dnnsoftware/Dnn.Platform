// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.UI.MenuControllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Dnn.PersonaBar.Library.Controllers;
    using Dnn.PersonaBar.Library.Model;
    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>An <see cref="IMenuItemController"/> for menu items that link to other pages.</summary>
    public class LinkMenuController : IMenuItemController
    {
        /// <summary>Initializes a new instance of the <see cref="LinkMenuController"/> class.</summary>
        public LinkMenuController()
        {
            this.NavigationManager = Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
        }

        /// <summary>Gets the navigation manager.</summary>
        protected INavigationManager NavigationManager { get; }

        /// <inheritdoc/>
        public void UpdateParameters(MenuItem menuItem)
        {
            if (!this.Visible(menuItem))
            {
                return;
            }

            var query = GetPathQuery(menuItem);

            int tabId, portalId;
            if (query.TryGetValue("path", out var path))
            {
                portalId = query.TryGetValue("portalId", out var queryPortalId) ? Convert.ToInt32(queryPortalId) : PortalSettings.Current.PortalId;
                tabId = TabController.GetTabByTabPath(portalId, path, string.Empty);
            }
            else
            {
                portalId = Convert.ToInt32(query["portalId"]);
                tabId = Convert.ToInt32(query["tabId"]);
            }

            var tabUrl = this.NavigationManager.NavigateURL(tabId, portalId == Null.NullInteger);
            var alias = Globals.AddHTTP(((IPortalAliasInfo)PortalSettings.Current.PortalAlias).HttpAlias);
            tabUrl = tabUrl.Replace(alias, string.Empty).TrimStart('/');

            menuItem.Link = tabUrl;
        }

        /// <inheritdoc/>
        public bool Visible(MenuItem menuItem)
        {
            var query = GetPathQuery(menuItem);
            if (PortalSettings.Current == null || query == null)
            {
                return false;
            }

            if (query.TryGetValue("sku", out var sku) && !string.IsNullOrEmpty(sku))
            {
                if (DotNetNukeContext.Current.Application.SKU != query["sku"])
                {
                    return false;
                }
            }

            int tabId, portalId;
            if (query.TryGetValue("path", out var path) && !string.IsNullOrEmpty(path))
            {
                portalId = query.TryGetValue("portalId", out var queryPortalId) ? Convert.ToInt32(queryPortalId) : PortalSettings.Current.PortalId;
                tabId = TabController.GetTabByTabPath(portalId, path, string.Empty);

                if (tabId == Null.NullInteger)
                {
                    return false;
                }
            }
            else
            {
                if (!query.TryGetValue("portalId", out var portalIdQuery) || !query.TryGetValue("tabId", out var tabIdQuery))
                {
                    return false;
                }

                portalId = Convert.ToInt32(portalIdQuery);
                tabId = Convert.ToInt32(tabIdQuery);
            }

            var tab = TabController.Instance.GetTab(tabId, portalId);
            return (portalId == Null.NullInteger || portalId == PortalSettings.Current.PortalId)
                   && tab is { IsDeleted: false, DisableLink: false, IsVisible: true };
        }

        /// <inheritdoc/>
        public IDictionary<string, object> GetSettings(MenuItem menuItem)
        {
            return null;
        }

        private static IDictionary<string, string> GetPathQuery(MenuItem menuItem)
        {
            var path = menuItem.Path;
            if (!path.Contains("?"))
            {
                return null;
            }

            return path.Substring(path.IndexOf("?", StringComparison.InvariantCultureIgnoreCase) + 1)
                .Split('&')
                .Select(p => p.Split('='))
                .Where(q => q.Length == 2 && !string.IsNullOrEmpty(q[0]) && !string.IsNullOrEmpty(q[1]))
                .ToDictionary(q => q[0], q => q[1]);
        }
    }
}
