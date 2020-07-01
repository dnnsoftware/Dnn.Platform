// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Tabs;
    using Microsoft.Extensions.DependencyInjection;

    /// -----------------------------------------------------------------------------
    /// <summary></summary>
    /// <returns></returns>
    /// <remarks></remarks>
    public partial class BreadCrumb : SkinObjectBase
    {
        private const string UrlRegex = "(href|src)=(\\\"|'|)(.[^\\\"']*)(\\\"|'|)";
        private readonly StringBuilder _breadcrumb = new StringBuilder("<span itemscope itemtype=\"http://schema.org/BreadcrumbList\">");
        private readonly INavigationManager _navigationManager;
        private string _separator = "<img alt=\"breadcrumb separator\" src=\"" + Globals.ApplicationPath + "/images/breadcrumb.gif\">";
        private string _cssClass = "SkinObject";
        private int _rootLevel = 0;
        private bool _showRoot = false;
        private string _homeUrl = string.Empty;
        private string _homeTabName = "Root";

        public BreadCrumb()
        {
            this._navigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
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
            get { return this._separator; }
            set { this._separator = value; }
        }

        public string CssClass
        {
            get { return this._cssClass; }
            set { this._cssClass = value; }
        }

        // Level to begin processing breadcrumb at.
        // -1 means show root breadcrumb
        public string RootLevel
        {
            get { return this._rootLevel.ToString(); }

            set
            {
                this._rootLevel = int.Parse(value);

                if (this._rootLevel < 0)
                {
                    this._showRoot = true;
                    this._rootLevel = 0;
                }
            }
        }

        // Use the page title instead of page name
        public bool UseTitle { get; set; }

        // Do not show when there is no breadcrumb (only has current tab)
        public bool HideWithNoBreadCrumb { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Position in breadcrumb list
            var position = 1;

            // resolve image path in separator content
            this.ResolveSeparatorPaths();

            // If we have enabled hiding when there are no breadcrumbs, simply return
            if (this.HideWithNoBreadCrumb && this.PortalSettings.ActiveTab.BreadCrumbs.Count == (this._rootLevel + 1))
            {
                return;
            }

            // Without checking if the current tab is the home tab, we would duplicate the root tab
            if (this._showRoot && this.PortalSettings.ActiveTab.TabID != this.PortalSettings.HomeTabId)
            {
                // Add the current protocal to the current URL
                this._homeUrl = Globals.AddHTTP(this.PortalSettings.PortalAlias.HTTPAlias);

                // Make sure we have a home tab ID set
                if (this.PortalSettings.HomeTabId != -1)
                {
                    this._homeUrl = this._navigationManager.NavigateURL(this.PortalSettings.HomeTabId);

                    var tc = new TabController();
                    var homeTab = tc.GetTab(this.PortalSettings.HomeTabId, this.PortalSettings.PortalId, false);
                    this._homeTabName = homeTab.LocalizedTabName;

                    // Check if we should use the tab's title instead
                    if (this.UseTitle && !string.IsNullOrEmpty(homeTab.Title))
                    {
                        this._homeTabName = homeTab.Title;
                    }
                }

                // Append all of the HTML for the root breadcrumb
                this._breadcrumb.Append("<span itemprop=\"itemListElement\" itemscope itemtype=\"http://schema.org/ListItem\">");
                this._breadcrumb.Append("<a href=\"" + this._homeUrl + "\" class=\"" + this._cssClass + "\" itemprop=\"item\" ><span itemprop=\"name\">" + this._homeTabName + "</span></a>");
                this._breadcrumb.Append("<meta itemprop=\"position\" content=\"" + position++ + "\" />"); // Notice we post-increment the position variable
                this._breadcrumb.Append("</span>");

                // Add a separator
                this._breadcrumb.Append(this._separator);
            }

            // process bread crumbs
            for (var i = this._rootLevel; i < this.PortalSettings.ActiveTab.BreadCrumbs.Count; ++i)
            {
                // Only add separators if we're past the root level
                if (i > this._rootLevel)
                {
                    this._breadcrumb.Append(this._separator);
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
                    tabUrl = this._navigationManager.NavigateURL(tab.TabID, string.Empty, "UserId=" + this.ProfileUserId);
                }

                if (this.GroupId > -1)
                {
                    tabUrl = this._navigationManager.NavigateURL(tab.TabID, string.Empty, "GroupId=" + this.GroupId);
                }

                // Begin breadcrumb
                this._breadcrumb.Append("<span itemprop=\"itemListElement\" itemscope itemtype=\"http://schema.org/ListItem\">");

                // Is this tab disabled? If so, only render the text
                if (tab.DisableLink)
                {
                    this._breadcrumb.Append("<span class=\"" + this._cssClass + "\" itemprop=\"name\">" + tabName + "</span>");
                }
                else
                {
                    this._breadcrumb.Append("<a href=\"" + tabUrl + "\" class=\"" + this._cssClass + "\" itemprop=\"item\"><span itemprop=\"name\">" + tabName + "</span></a>");
                }

                this._breadcrumb.Append("<meta itemprop=\"position\" content=\"" + position++ + "\" />"); // Notice we post-increment the position variable
                this._breadcrumb.Append("</span>");
            }

            this._breadcrumb.Append("</span>"); // End of BreadcrumbList

            this.lblBreadCrumb.Text = this._breadcrumb.ToString();
        }

        private void ResolveSeparatorPaths()
        {
            if (string.IsNullOrEmpty(this._separator))
            {
                return;
            }

            var urlMatches = Regex.Matches(this._separator, UrlRegex, RegexOptions.IgnoreCase);
            if (urlMatches.Count > 0)
            {
                foreach (Match match in urlMatches)
                {
                    var url = match.Groups[3].Value;
                    var changed = false;

                    if (url.StartsWith("/"))
                    {
                        if (!string.IsNullOrEmpty(Globals.ApplicationPath))
                        {
                            url = string.Format("{0}{1}", Globals.ApplicationPath, url);
                            changed = true;
                        }
                    }
                    else if (url.StartsWith("~/"))
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

                        this._separator = this._separator.Replace(match.Value, newMatch);
                    }
                }
            }
        }
    }
}
