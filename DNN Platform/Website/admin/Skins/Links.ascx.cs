// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Tabs;

    /// <summary></summary>
    public partial class Links : SkinObjectBase
    {
        private static readonly Regex SrcRegex = new Regex("src=[']?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private string alignment;
        private bool forceLinks = true;
        private bool includeActiveTab = true;
        private string level;

        public string Alignment
        {
            get
            {
                return this.alignment;
            }

            set
            {
                this.alignment = value.ToLowerInvariant();
            }
        }

        public string CssClass { get; set; }

        public string Level
        {
            get
            {
                return this.level;
            }

            set
            {
                this.level = value.ToLowerInvariant();
            }
        }

        public string Separator { get; set; }

        public bool ShowDisabled { get; set; }

        public bool ForceLinks
        {
            get
            {
                return this.forceLinks;
            }

            set
            {
                this.forceLinks = value;
            }
        }

        public bool IncludeActiveTab
        {
            get
            {
                return this.includeActiveTab;
            }

            set
            {
                this.includeActiveTab = value;
            }
        }

        /// <inheritdoc/>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            string strCssClass;
            if (!string.IsNullOrEmpty(this.CssClass))
            {
                strCssClass = this.CssClass;
            }
            else
            {
                strCssClass = "SkinObject";
            }

            string strSeparator = string.Empty;
            if (!string.IsNullOrEmpty(this.Separator))
            {
                if (this.Separator.IndexOf("src=", StringComparison.Ordinal) != -1)
                {
                    // Add the skinpath to image paths
                    this.Separator = SrcRegex.Replace(this.Separator, "$&" + this.PortalSettings.ActiveTab.SkinPath);
                }

                // Wrap in a span
                this.Separator = string.Format("<span class=\"{0}\">{1}</span>", strCssClass, this.Separator);
            }
            else
            {
                this.Separator = " ";
            }

            // build links
            string strLinks = string.Empty;

            strLinks = this.BuildLinks(this.Level, strSeparator, strCssClass);

            // Render links, even if nothing is returned with the currently set level
            if (string.IsNullOrEmpty(strLinks) && this.ForceLinks)
            {
                strLinks = this.BuildLinks(string.Empty, strSeparator, strCssClass);
            }

            this.lblLinks.Text = strLinks;
        }

        private string BuildLinks(string strLevel, string strSeparator, string strCssClass)
        {
            var sbLinks = new StringBuilder();

            List<TabInfo> portalTabs = TabController.GetTabsBySortOrder(this.PortalSettings.PortalId);
            List<TabInfo> hostTabs = TabController.GetTabsBySortOrder(Null.NullInteger);

            foreach (TabInfo objTab in portalTabs)
            {
                sbLinks.Append(this.ProcessLink(this.ProcessTab(objTab, strLevel, strCssClass), sbLinks.ToString().Length));
            }

            foreach (TabInfo objTab in hostTabs)
            {
                sbLinks.Append(this.ProcessLink(this.ProcessTab(objTab, strLevel, strCssClass), sbLinks.ToString().Length));
            }

            return sbLinks.ToString();
        }

        private string ProcessTab(TabInfo objTab, string strLevel, string strCssClass)
        {
            if (Navigation.CanShowTab(objTab, this.AdminMode, this.ShowDisabled))
            {
                switch (strLevel)
                {
                    case "same": // Render tabs on the same level as the current tab
                    case "":
                        if (objTab.ParentId == this.PortalSettings.ActiveTab.ParentId)
                        {
                            if (this.IncludeActiveTab || objTab.TabID != this.PortalSettings.ActiveTab.TabID)
                            {
                                return this.AddLink(objTab.TabName, objTab.FullUrl, strCssClass);
                            }
                        }

                        break;
                    case "child": // Render the current tabs child tabs
                        if (objTab.ParentId == this.PortalSettings.ActiveTab.TabID)
                        {
                            return this.AddLink(objTab.TabName, objTab.FullUrl, strCssClass);
                        }

                        break;
                    case "parent": // Render the current tabs parenttab
                        if (objTab.TabID == this.PortalSettings.ActiveTab.ParentId)
                        {
                            return this.AddLink(objTab.TabName, objTab.FullUrl, strCssClass);
                        }

                        break;
                    case "root": // Render Root tabs
                        if (objTab.Level == 0)
                        {
                            return this.AddLink(objTab.TabName, objTab.FullUrl, strCssClass);
                        }

                        break;
                }
            }

            return string.Empty;
        }

        private string ProcessLink(string sLink, int iLinksLength)
        {
            // wrap in a div if set to vertical
            if (string.IsNullOrEmpty(sLink))
            {
                return string.Empty;
            }

            if (this.Alignment == "vertical")
            {
                sLink = string.Concat("<div>", this.Separator, sLink, "</div>");
            }
            else if (!string.IsNullOrEmpty(this.Separator) && iLinksLength > 0)
            {
                // If not vertical, then render the separator
                sLink = string.Concat(this.Separator, sLink);
            }

            return sLink;
        }

        private string AddLink(string strTabName, string strURL, string strCssClass)
        {
            return string.Format("<a class=\"{0}\" href=\"{1}\">{2}</a>", strCssClass, strURL, strTabName);
        }
    }
}
