// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Tabs;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A skin/theme object which displays the hierarchy of the current page.</summary>
    public partial class BreadCrumb : SkinObjectBase
    {
        private const string UrlRegex = "(href|src)=(\\\"|'|)(.[^\\\"']*)(\\\"|'|)";
        private readonly StringBuilder breadcrumb = new StringBuilder("<span itemscope itemtype=\"http://schema.org/BreadcrumbList\">");
        private readonly INavigationManager navigationManager;
        private string separator = "<img alt=\"breadcrumb separator\" src=\"" + Globals.ApplicationPath + "/images/breadcrumb.gif\">";
        private string cssClass = "SkinObject";
        private int rootLevel;
        private bool showRoot;
        private string homeUrl = string.Empty;
        private string homeTabName = "Root";

        public BreadCrumb()
        {
            this.navigationManager = Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
            this.CleanerMarkup = false;
        }

        public int ProfileUserId
        {
            get
            {
                return string.IsNullOrEmpty(this.Request.Params["UserId"])
                    ? Null.NullInteger
                    : int.Parse(this.Request.Params["UserId"]);
            }
        }

        public int GroupId
        {
            get
            {
                return string.IsNullOrEmpty(this.Request.Params["GroupId"])
                    ? Null.NullInteger
                    : int.Parse(this.Request.Params["GroupId"]);
            }
        }

        // Separator between breadcrumb elements
        public string Separator
        {
            get { return this.separator; }
            set { this.separator = value; }
        }

        public string CssClass
        {
            get { return this.cssClass; }
            set { this.cssClass = value; }
        }

        // Level to begin processing breadcrumb at.
        // -1 means show root breadcrumb
        public string RootLevel
        {
            get
            {
                return this.rootLevel.ToString();
            }

            set
            {
                this.rootLevel = int.Parse(value);
                if (this.rootLevel < 0)
                {
                    this.showRoot = true;
                    this.rootLevel = 0;
                }
            }
        }

        // Use the page title instead of page name
        public bool UseTitle { get; set; }

        // Do not show when there is no breadcrumb (only has current tab)
        public bool HideWithNoBreadCrumb { get; set; }

        /// <summary>Gets or sets a value indicating whether to take advantage of the enhanced markup (remove extra wrapping elements).</summary>
        public bool CleanerMarkup { get; set; }

        private IPortalAliasInfo CurrentPortalAlias => this.PortalSettings.PortalAlias;

        /// <inheritdoc/>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Position in breadcrumb list
            var position = 1;

            // resolve image path in separator content
            this.ResolveSeparatorPaths();

            // If we have enabled hiding when there are no breadcrumbs, simply return
            if (this.HideWithNoBreadCrumb && this.PortalSettings.ActiveTab.BreadCrumbs.Count == (this.rootLevel + 1))
            {
                return;
            }

            // Without checking if the current tab is the home tab, we would duplicate the root tab
            if (this.showRoot && this.PortalSettings.ActiveTab.TabID != this.PortalSettings.HomeTabId)
            {
                // Add the current protocol to the current URL
                this.homeUrl = Globals.AddHTTP(this.CurrentPortalAlias.HttpAlias);

                // Make sure we have a home tab ID set
                if (this.PortalSettings.HomeTabId != -1)
                {
                    this.homeUrl = this.navigationManager.NavigateURL(this.PortalSettings.HomeTabId);

                    var tc = new TabController();
                    var homeTab = tc.GetTab(this.PortalSettings.HomeTabId, this.PortalSettings.PortalId, false);
                    this.homeTabName = homeTab.LocalizedTabName;

                    // Check if we should use the tab's title instead
                    if (this.UseTitle && !string.IsNullOrEmpty(homeTab.Title))
                    {
                        this.homeTabName = homeTab.Title;
                    }
                }

                // Append all of the HTML for the root breadcrumb
                this.breadcrumb.Append("<span itemprop=\"itemListElement\" itemscope itemtype=\"http://schema.org/ListItem\">");
                this.breadcrumb.Append("<a href=\"" + this.homeUrl + "\" class=\"" + this.cssClass + "\" itemprop=\"item\" ><span itemprop=\"name\">" + this.homeTabName + "</span></a>");
                this.breadcrumb.Append("<meta itemprop=\"position\" content=\"" + position++ + "\" />"); // Notice we post-increment the position variable
                this.breadcrumb.Append("</span>");

                // Add a separator
                this.breadcrumb.Append(this.separator);
            }

            // process bread crumbs
            for (var i = this.rootLevel; i < this.PortalSettings.ActiveTab.BreadCrumbs.Count; ++i)
            {
                // Only add separators if we're past the root level
                if (i > this.rootLevel)
                {
                    this.breadcrumb.Append(this.separator);
                }

                // Grab the current tab
                var tab = (TabInfo)this.PortalSettings.ActiveTab.BreadCrumbs[i];

                var tabName = tab.LocalizedTabName;

                // Determine if we should use the tab's title instead of tab name
                if (this.UseTitle && !string.IsNullOrEmpty(tab.Title))
                {
                    tabName = tab.Title;
                }

                // Get the absolute URL of the tab
                var tabUrl = tab.FullUrl;

                if (this.ProfileUserId > -1)
                {
                    tabUrl = this.navigationManager.NavigateURL(tab.TabID, string.Empty, "UserId=" + this.ProfileUserId);
                }

                if (this.GroupId > -1)
                {
                    tabUrl = this.navigationManager.NavigateURL(tab.TabID, string.Empty, "GroupId=" + this.GroupId);
                }

                // Is this tab disabled? If so, only render a span
                if (tab.DisableLink)
                {
                    if (this.CleanerMarkup)
                    {
                        this.breadcrumb.Append("<span class=\"" + this.cssClass + "\">" + tabName + "</span>");
                    }
                    else
                    {
                        this.breadcrumb.Append("<span><span class=\"" + this.cssClass + "\">" + tabName + "</span></span>");
                    }
                }
                else
                {
                    // An enabled page, render the breadcrumb
                    this.breadcrumb.Append("<span itemprop=\"itemListElement\" itemscope itemtype=\"http://schema.org/ListItem\">");
                    this.breadcrumb.Append("<a href=\"" + tabUrl + "\" class=\"" + this.cssClass + "\" itemprop=\"item\"><span itemprop=\"name\">" + tabName + "</span></a>");
                    this.breadcrumb.Append("<meta itemprop=\"position\" content=\"" + position++ + "\" />"); // Notice we post-increment the position variable
                    this.breadcrumb.Append("</span>");
                }
            }

            this.breadcrumb.Append("</span>"); // End of BreadcrumbList

            this.lblBreadCrumb.Text = this.breadcrumb.ToString();
        }

        private void ResolveSeparatorPaths()
        {
            if (string.IsNullOrEmpty(this.separator))
            {
                return;
            }

            var urlMatches = Regex.Matches(this.separator, UrlRegex, RegexOptions.IgnoreCase);
            if (urlMatches.Count > 0)
            {
                foreach (Match match in urlMatches)
                {
                    var url = match.Groups[3].Value;
                    var changed = false;

                    if (url.StartsWith("/", StringComparison.Ordinal))
                    {
                        if (!string.IsNullOrEmpty(Globals.ApplicationPath))
                        {
                            url = string.Format("{0}{1}", Globals.ApplicationPath, url);
                            changed = true;
                        }
                    }
                    else if (url.StartsWith("~/", StringComparison.Ordinal))
                    {
                        url = Globals.ResolveUrl(url);
                        changed = true;
                    }
                    else
                    {
                        url = string.Format("{0}{1}", this.PortalSettings.ActiveTab.SkinPath, url);
                        changed = true;
                    }

                    if (changed)
                    {
                        var newMatch = string.Format(
                            "{0}={1}{2}{3}",
                            match.Groups[1].Value,
                            match.Groups[2].Value,
                            url,
                            match.Groups[4].Value);

                        this.separator = this.separator.Replace(match.Value, newMatch);
                    }
                }
            }
        }
    }
}
