#region Copyright
// 
// DotNetNuke? - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

#region Usings

using System;
using System.Text;
using System.Text.RegularExpressions;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Tabs;

#endregion

namespace DotNetNuke.UI.Skins.Controls
{
    /// -----------------------------------------------------------------------------
    /// <summary></summary>
    /// <returns></returns>
    /// <remarks></remarks>
    public partial class BreadCrumb : SkinObjectBase
    {
        private const string UrlRegex = "(href|src)=(\\\"|'|)(.[^\\\"']*)(\\\"|'|)";
        private string _separator = "<img alt=\"breadcrumb separator\" src=\"" + Globals.ApplicationPath + "/images/breadcrumb.gif\">";
        private string _cssClass = "SkinObject";
        private int _rootLevel = 0;
        private bool _showRoot = false;
        private readonly StringBuilder _breadcrumb = new StringBuilder("<span itemscope itemtype=\"http://schema.org/BreadcrumbList\">");
        private string _homeUrl = "";
        private string _homeTabName = "Root";

        // Separator between breadcrumb elements
        public string Separator
        {
            get { return _separator; }
            set { _separator = value; }
        }

        public string CssClass
        {
            get { return _cssClass; }
            set { _cssClass = value; }
        }

        // Level to begin processing breadcrumb at.
        // -1 means show root breadcrumb
        public string RootLevel
        {
            get { return _rootLevel.ToString(); }
            set
            {
                _rootLevel = int.Parse(value);

                if (_rootLevel < 0)
                {
                    _showRoot = true;
                    _rootLevel = 0;
                }
            }
        }

        // Use the page title instead of page name
        public bool UseTitle { get; set; }

        // Do not show when there is no breadcrumb (only has current tab)
        public bool HideWithNoBreadCrumb { get; set; }

		public int ProfileUserId
        {
            get
            {
                return string.IsNullOrEmpty(Request.Params["UserId"])
                    ? Null.NullInteger
                    : int.Parse(Request.Params["UserId"]);
            }
        }

        public int GroupId
        {
            get
            {
                return string.IsNullOrEmpty(Request.Params["GroupId"])
                    ? Null.NullInteger
                    : int.Parse(Request.Params["GroupId"]);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Position in breadcrumb list
            var position = 1;

            //resolve image path in separator content
            ResolveSeparatorPaths();

            // If we have enabled hiding when there are no breadcrumbs, simply return
            if (HideWithNoBreadCrumb && PortalSettings.ActiveTab.BreadCrumbs.Count == (_rootLevel + 1))
            {
                return;
            }

            // Without checking if the current tab is the home tab, we would duplicate the root tab
            if (_showRoot && PortalSettings.ActiveTab.TabID != PortalSettings.HomeTabId)
            {
                // Add the current protocal to the current URL
                _homeUrl = Globals.AddHTTP(PortalSettings.PortalAlias.HTTPAlias);

                // Make sure we have a home tab ID set
                if (PortalSettings.HomeTabId != -1)
                {
                    _homeUrl = Globals.NavigateURL(PortalSettings.HomeTabId);

                    var tc = new TabController();
                    var homeTab = tc.GetTab(PortalSettings.HomeTabId, PortalSettings.PortalId, false);
                    _homeTabName = homeTab.LocalizedTabName;

                    // Check if we should use the tab's title instead
                    if (UseTitle && !string.IsNullOrEmpty(homeTab.Title))
                    {
                        _homeTabName = homeTab.Title;
                    }
                }

                // Append all of the HTML for the root breadcrumb
                _breadcrumb.Append("<span itemprop=\"itemListElement\" itemscope itemtype=\"http://schema.org/ListItem\">");
                _breadcrumb.Append("<a href=\"" + _homeUrl + "\" class=\"" + _cssClass + "\" itemprop=\"item\" ><span itemprop=\"name\">" + _homeTabName + "</span></a>");
                _breadcrumb.Append("<meta itemprop=\"position\" content=\"" + position++ + "\" />"); // Notice we post-increment the position variable
                _breadcrumb.Append("</span>");

                // Add a separator
                _breadcrumb.Append(_separator);
            }

            //process bread crumbs
            for (var i = _rootLevel; i < PortalSettings.ActiveTab.BreadCrumbs.Count; ++i)
            {
                // Only add separators if we're past the root level
                if (i > _rootLevel)
                {
                    _breadcrumb.Append(_separator);
                }

                // Grab the current tab
                var tab = (TabInfo)PortalSettings.ActiveTab.BreadCrumbs[i];

                var tabName = tab.LocalizedTabName;

                // Determine if we should use the tab's title instead of tab name
                if (UseTitle && !string.IsNullOrEmpty(tab.Title))
                {
                    tabName = tab.Title;
                }

                // Get the absolute URL of the tab
                var tabUrl = tab.FullUrl;

                // 
                if (ProfileUserId > -1)
                {
                    tabUrl = Globals.NavigateURL(tab.TabID, "", "UserId=" + ProfileUserId);
                }

                // 
                if (GroupId > -1)
                {
                    tabUrl = Globals.NavigateURL(tab.TabID, "", "GroupId=" + GroupId);
                }

                // Begin breadcrumb
                _breadcrumb.Append("<span itemprop=\"itemListElement\" itemscope itemtype=\"http://schema.org/ListItem\">");

                // Is this tab disabled? If so, only render the text
                if (tab.DisableLink)
                {
                    _breadcrumb.Append("<span class=\"" + _cssClass + "\" itemprop=\"name\">" + tabName + "</span>");
                }
                else
                {
                    _breadcrumb.Append("<a href=\"" + tabUrl + "\" class=\"" + _cssClass + "\" itemprop=\"item\"><span itemprop=\"name\">" + tabName + "</span></a>");
                }

                _breadcrumb.Append("<meta itemprop=\"position\" content=\"" + position++ + "\" />"); // Notice we post-increment the position variable
                _breadcrumb.Append("</span>");
            }

            _breadcrumb.Append("</span>"); //End of BreadcrumbList
            
            lblBreadCrumb.Text = _breadcrumb.ToString();
        }

        private void ResolveSeparatorPaths()
        {
            if (string.IsNullOrEmpty(_separator))
            {
                return;
            }

            var urlMatches = Regex.Matches(_separator, UrlRegex, RegexOptions.IgnoreCase);
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
                        url = string.Format("{0}{1}", PortalSettings.ActiveTab.SkinPath, url);
                        changed = true;
                    }

                    if (changed)
                    {
                        var newMatch = string.Format("{0}={1}{2}{3}", 
                                                        match.Groups[1].Value, 
                                                        match.Groups[2].Value, 
                                                        url,
                                                        match.Groups[4].Value);

                        _separator = _separator.Replace(match.Value, newMatch);
                    }
                }

            }
        }
    }
}