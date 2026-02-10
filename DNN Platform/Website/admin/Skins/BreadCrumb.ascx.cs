// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Services.Localization;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A skin/theme object which displays the hierarchy of the current page.</summary>
    /// <remarks>
    /// The legacy renderings uses spans to mimic the old asp:Label output for backward compatibility.
    /// v2 rendering uses nav/ol/li markup for better semantics and accessibility.
    /// </remarks>
    public partial class BreadCrumb : SkinObjectBase
    {
        private const string MyFileName = "BreadCrumb.ascx";
        private const string HomeTabName = "Root";
        private const string UrlRegex = """(href|src)=(\"|'|)(.[^\"']*)(\"|'|)""";
        private readonly INavigationManager navigationManager;
        private readonly ITabController tabController;

        private int rootLevel;
        private bool showRoot;

        /// <summary>Initializes a new instance of the <see cref="BreadCrumb"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with INavigationManager. Scheduled removal in v12.0.0.")]
        public BreadCrumb()
            : this(null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="BreadCrumb"/> class.</summary>
        /// <param name="navigationManager">The navigation provider.</param>
        /// <param name="tabController">The tab controller.</param>
        public BreadCrumb(INavigationManager navigationManager, ITabController tabController)
        {
            this.navigationManager = navigationManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
            this.tabController = tabController ?? Globals.GetCurrentServiceProvider().GetRequiredService<ITabController>();
        }

        /// <summary>
        /// Gets the UserId of the profile being viewed, or -1 if not applicable.
        /// </summary>
        public int ProfileUserId
        {
            get
            {
                return string.IsNullOrEmpty(this.Request.Params["UserId"])
                    ? Null.NullInteger
                    : int.Parse(this.Request.Params["UserId"]);
            }
        }

        /// <summary>
        /// Gets the GroupId of the group being viewed, or -1 if not applicable.
        /// </summary>
        public int GroupId
        {
            get
            {
                return string.IsNullOrEmpty(this.Request.Params["GroupId"])
                    ? Null.NullInteger
                    : int.Parse(this.Request.Params["GroupId"]);
            }
        }

        /// <summary>
        /// Gets or sets the separator between breadcrumb elements.
        /// </summary>
        public string Separator { get; set; } = $"""<img alt="breadcrumb separator" src="{Globals.ApplicationPath}/images/breadcrumb.gif">""";

        /// <summary>
        /// Gets or sets the CSS class name applied to the element for styling purposes.
        /// </summary>
        public string CssClass { get; set; } = "SkinObject";

        /// <summary>
        /// Gets or sets the level to begin processing breadcrumb at.
        /// </summary>
        /// <remarks>-1 means show root breadcrumb.</remarks>
        public string RootLevel
        {
            get
            {
                return this.rootLevel.ToString(CultureInfo.InvariantCulture);
            }

            set
            {
                this.rootLevel = int.Parse(value, CultureInfo.InvariantCulture);
                if (this.rootLevel < 0)
                {
                    this.showRoot = true;
                    this.rootLevel = 0;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use the page title instead of page name.
        /// </summary>
        public bool UseTitle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the control is hidden when no breadcrumb is available and only the
        /// current tab is present.
        /// </summary>
        public bool HideWithNoBreadCrumb { get; set; }

        /// <summary>Gets or sets a value indicating whether to take advantage of the enhanced markup
        /// (remove extra wrapping elements). No effect when <see cref="SkinObjectBase.RenderingVersion" /> is 2 or higher.
        /// </summary>
        public bool CleanerMarkup { get; set; }

        private IPortalAliasInfo CurrentPortalAlias => this.PortalSettings.PortalAlias;

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.ResolveSeparatorPaths();

            var crumbCount = this.PortalSettings.ActiveTab.BreadCrumbs.Count;

            if (this.HideWithNoBreadCrumb && crumbCount == (this.rootLevel + 1))
            {
                return;
            }

            this.lblBreadCrumb.Text = this.RenderingVersion >= 2
                ? this.BuildListMarkup(crumbCount)
                : this.BuildLegacyMarkup(crumbCount);
        }

        private string BuildLegacyMarkup(int crumbCount)
        {
            var position = 1;

            // IMPORTANT:
            // We intentionally keep the legacy outer span wrapper so existing skins/CSS don't break.
            // This mimics the old asp:Label output wrapper and keeps schema breadcrumb attrs.
            var breadcrumb = new StringBuilder()
                .Append(
                    """
                    <span itemprop="breadcrumb" itemscope itemtype="https://schema.org/breadcrumb">
                    <span itemscope itemtype="http://schema.org/BreadcrumbList">
                    """);

            // Without checking if the current tab is the home tab, we would duplicate the root tab
            if (this.showRoot && this.PortalSettings.ActiveTab.TabID != this.PortalSettings.HomeTabId)
            {
                this.ResolveHome(out var homeUrl, out var homeNameEncoded);

                breadcrumb.Append(
                    $"""
                     <span itemprop="itemListElement" itemscope itemtype="http://schema.org/ListItem">
                         <a href="{homeUrl}" class="{this.CssClass}" itemprop="item"><span itemprop="name">{homeNameEncoded}</span></a>
                         <meta itemprop="position" content="{position++}" />
                     </span>
                     {this.Separator}
                     """);
            }

            for (var i = this.rootLevel; i < crumbCount; ++i)
            {
                if (i > this.rootLevel)
                {
                    breadcrumb.Append(this.Separator);
                }

                var tab = (TabInfo)this.PortalSettings.ActiveTab.BreadCrumbs[i];

                var tabName = this.GetTabDisplayName(tab);
                var tabNameEncoded = HttpUtility.HtmlEncode(tabName ?? string.Empty);

                var tabUrl = this.GetTabUrl(tab);

                if (tab.DisableLink)
                {
                    if (this.CleanerMarkup)
                    {
                        breadcrumb.AppendFormat(
                            CultureInfo.InvariantCulture,
                            """<span class="{0}">{1}</span>""",
                            this.CssClass,
                            tabNameEncoded);
                    }
                    else
                    {
                        breadcrumb.AppendFormat(
                            CultureInfo.InvariantCulture,
                            """<span><span class="{0}">{1}</span></span>""",
                            this.CssClass,
                            tabNameEncoded);
                    }
                }
                else
                {
                    breadcrumb.Append(
                        $"""
                         <span itemprop="itemListElement" itemscope itemtype="http://schema.org/ListItem">
                             <a href="{tabUrl}" class="{this.CssClass}" itemprop="item"><span itemprop="name">{tabNameEncoded}</span></a>
                             <meta itemprop="position" content="{position++}" />
                         </span>
                         """);
                }
            }

            // close both wrappers
            breadcrumb.Append("</span></span>");
            return breadcrumb.ToString();
        }

        private string BuildListMarkup(int crumbCount)
        {
            var position = 1;
            var breadcrumb = new StringBuilder();

            // IMPORTANT:
            // List markup must start with <nav> (no legacy wrapper).
            breadcrumb.Append(
                $"""
                 <nav class="dnnBreadcrumb" aria-label="{HttpUtility.HtmlEncode(this.GetAriaLabel())}">
                    <ol itemscope itemtype="https://schema.org/BreadcrumbList">
                 """);

            if (this.showRoot && this.PortalSettings.ActiveTab.TabID != this.PortalSettings.HomeTabId)
            {
                this.ResolveHome(out var homeUrl, out var homeNameEncoded);

                breadcrumb.Append(
                    $"""
                     <li itemprop="itemListElement" itemscope itemtype="https://schema.org/ListItem">
                         <a href="{homeUrl}" class="{this.CssClass}" itemprop="item"><span itemprop="name">{homeNameEncoded}</span></a>
                         <meta itemprop="position" content="{position++}" />
                     </li>
                     """);
            }

            for (var i = this.rootLevel; i < crumbCount; ++i)
            {
                var tab = (TabInfo)this.PortalSettings.ActiveTab.BreadCrumbs[i];

                var tabName = this.GetTabDisplayName(tab);
                var tabNameEncoded = HttpUtility.HtmlEncode(tabName ?? string.Empty);

                var tabUrl = this.GetTabUrl(tab);

                var isLastCrumb = i == (crumbCount - 1);
                var liAriaCurrent = isLastCrumb ? " aria-current=\"page\"" : string.Empty;

                breadcrumb.AppendFormat(
                    CultureInfo.InvariantCulture,
                    """<li itemprop="itemListElement" itemscope itemtype="https://schema.org/ListItem"{0}>""",
                    liAriaCurrent);

                if (tab.DisableLink)
                {
                    breadcrumb.AppendFormat(
                        CultureInfo.InvariantCulture,
                        """<span class="{0}" itemprop="name">{1}</span>""",
                        this.CssClass,
                        tabNameEncoded);
                }
                else
                {
                    breadcrumb.AppendFormat(
                        CultureInfo.InvariantCulture,
                        """<a href="{0}" class="{1}" itemprop="item"><span itemprop="name">{2}</span></a>""",
                        tabUrl,
                        this.CssClass,
                        tabNameEncoded);
                }

                breadcrumb.AppendFormat(
                    CultureInfo.InvariantCulture,
                    """<meta itemprop="position" content="{0}" />""",
                    position++);

                if (!isLastCrumb)
                {
                    breadcrumb.AppendFormat("""<span aria-hidden="true">{0}</span>""", this.Separator);
                }

                breadcrumb.Append("</li>");
            }

            breadcrumb.Append("</ol></nav>");
            return breadcrumb.ToString();
        }

        private void ResolveHome(out string homeUrl, out string homeNameEncoded)
        {
            homeUrl = Globals.AddHTTP(this.CurrentPortalAlias.HttpAlias);
            var homeName = HomeTabName;

            if (this.PortalSettings.HomeTabId != -1)
            {
                homeUrl = this.navigationManager.NavigateURL(this.PortalSettings.HomeTabId);

                var homeTab = this.tabController.GetTab(this.PortalSettings.HomeTabId, this.PortalSettings.PortalId, false);
                homeName = homeTab?.LocalizedTabName ?? homeName;

                if (this.UseTitle && homeTab != null && !string.IsNullOrEmpty(homeTab.Title))
                {
                    homeName = homeTab.Title;
                }
            }

            homeNameEncoded = HttpUtility.HtmlEncode(homeName ?? string.Empty);
        }

        private string GetTabDisplayName(TabInfo tab)
        {
            var tabName = tab.LocalizedTabName;
            if (this.UseTitle && !string.IsNullOrEmpty(tab.Title))
            {
                tabName = tab.Title;
            }

            return tabName;
        }

        private string GetTabUrl(TabInfo tab)
        {
            var tabUrl = tab.FullUrl;

            if (this.ProfileUserId > -1)
            {
                tabUrl = this.navigationManager.NavigateURL(
                    tab.TabID,
                    string.Empty,
                    "UserId=" + this.ProfileUserId.ToString(CultureInfo.InvariantCulture));
            }

            if (this.GroupId > -1)
            {
                tabUrl = this.navigationManager.NavigateURL(
                    tab.TabID,
                    string.Empty,
                    "GroupId=" + this.GroupId.ToString(CultureInfo.InvariantCulture));
            }

            return tabUrl;
        }

        private string GetAriaLabel()
        {
            // resource key (example):
            ////BreadCrumbAriaLabel.Text = Breadcrumb
            var localized = Localization.GetString("BreadCrumbAriaLabel.Text", Localization.GetResourceFile(this, MyFileName));
            return string.IsNullOrWhiteSpace(localized) ? "Breadcrumb" : localized;
        }

        private void ResolveSeparatorPaths()
        {
            if (string.IsNullOrEmpty(this.Separator))
            {
                return;
            }

            var urlMatches = Regex.Matches(this.Separator, UrlRegex, RegexOptions.IgnoreCase);
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
                            url = string.Format(CultureInfo.InvariantCulture, "{0}{1}", Globals.ApplicationPath, url);
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
                        url = string.Format(CultureInfo.InvariantCulture, "{0}{1}", this.PortalSettings.ActiveTab.SkinPath, url);
                        changed = true;
                    }

                    if (changed)
                    {
                        var newMatch = string.Format(
                            CultureInfo.InvariantCulture,
                            "{0}={1}{2}{3}",
                            match.Groups[1].Value,
                            match.Groups[2].Value,
                            url,
                            match.Groups[4].Value);

                        this.Separator = this.Separator.Replace(match.Value, newMatch);
                    }
                }
            }
        }
    }
}
