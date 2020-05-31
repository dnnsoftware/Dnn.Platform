// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Tabs;

#endregion

namespace DotNetNuke.UI.Skins.Controls
{
    /// -----------------------------------------------------------------------------
    /// <summary></summary>
    /// <remarks></remarks>
    /// -----------------------------------------------------------------------------
    public partial class Links : SkinObjectBase
    {
        private static readonly Regex SrcRegex = new Regex("src=[']?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		#region "Private Members"

        private string _alignment;
        private bool _forceLinks = true;
        private bool _includeActiveTab = true;
        private string _level;

		#endregion

		#region "Public Members"

        public string Alignment
        {
            get
            {
                return _alignment;
            }
            set
            {
                _alignment = value.ToLowerInvariant();
            }
        }

        public string CssClass { get; set; }

        public string Level
        {
            get
            {
                return _level;
            }
            set
            {
                _level = value.ToLowerInvariant();
            }
        }

        public string Separator { get; set; }

        public bool ShowDisabled { get; set; }

        public bool ForceLinks
        {
            get
            {
                return _forceLinks;
            }
            set
            {
                _forceLinks = value;
            }
        }

        public bool IncludeActiveTab
        {
            get
            {
                return _includeActiveTab;
            }
            set
            {
                _includeActiveTab = value;
            }
        }
		
		#endregion
		
		#region "Event Handlers"

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            string strCssClass;
            if (!String.IsNullOrEmpty(CssClass))
            {
                strCssClass = CssClass;
            }
            else
            {
                strCssClass = "SkinObject";
            }
            string strSeparator = string.Empty;
            if (!String.IsNullOrEmpty(Separator))
            {
                if (Separator.IndexOf("src=", StringComparison.Ordinal) != -1)
                {
					//Add the skinpath to image paths
                    Separator = SrcRegex.Replace(Separator, "$&" + PortalSettings.ActiveTab.SkinPath);
                }
				
				//Wrap in a span
                Separator = string.Format("<span class=\"{0}\">{1}</span>", strCssClass, Separator);
            }
            else
            {
                Separator = " ";
            }
			
            //build links
            string strLinks = "";

            strLinks = BuildLinks(Level, strSeparator, strCssClass);
			
			//Render links, even if nothing is returned with the currently set level
            if (String.IsNullOrEmpty(strLinks) && ForceLinks)
            {
                strLinks = BuildLinks("", strSeparator, strCssClass);
            }
            lblLinks.Text = strLinks;
        }
		
		#endregion
		
		#region "Private Methods"

        private string BuildLinks(string strLevel, string strSeparator, string strCssClass)
        {
            var sbLinks = new StringBuilder();

            List<TabInfo> portalTabs = TabController.GetTabsBySortOrder(PortalSettings.PortalId);
            List<TabInfo> hostTabs = TabController.GetTabsBySortOrder(Null.NullInteger);

            foreach (TabInfo objTab in portalTabs)
            {
                sbLinks.Append(ProcessLink(ProcessTab(objTab, strLevel, strCssClass), sbLinks.ToString().Length));
            }
            foreach (TabInfo objTab in hostTabs)
            {
                sbLinks.Append(ProcessLink(ProcessTab(objTab, strLevel, strCssClass), sbLinks.ToString().Length));
            }
            return sbLinks.ToString();
        }

        private string ProcessTab(TabInfo objTab, string strLevel, string strCssClass)
        {
            if (Navigation.CanShowTab(objTab, AdminMode, ShowDisabled))
            {
                switch (strLevel)
                {
                    case "same": //Render tabs on the same level as the current tab
                    case "":
                        if (objTab.ParentId == PortalSettings.ActiveTab.ParentId)
                        {
                            if (IncludeActiveTab || objTab.TabID != PortalSettings.ActiveTab.TabID)
                            {
                                return AddLink(objTab.TabName, objTab.FullUrl, strCssClass);
                            }
                        }
                        break;
                    case "child": //Render the current tabs child tabs
                        if (objTab.ParentId == PortalSettings.ActiveTab.TabID)
                        {
                            return AddLink(objTab.TabName, objTab.FullUrl, strCssClass);
                        }
                        break;
                    case "parent": //Render the current tabs parenttab
                        if (objTab.TabID == PortalSettings.ActiveTab.ParentId)
                        {
                            return AddLink(objTab.TabName, objTab.FullUrl, strCssClass);
                        }
                        break;
                    case "root": //Render Root tabs
                        if (objTab.Level == 0)
                        {
                            return AddLink(objTab.TabName, objTab.FullUrl, strCssClass);
                        }
                        break;
                }
            }
            return "";
        }

        private string ProcessLink(string sLink, int iLinksLength)
        {
			//wrap in a div if set to vertical
            if (String.IsNullOrEmpty(sLink))
            {
                return "";
            }
            if (Alignment == "vertical")
            {
                sLink = string.Concat("<div>", Separator, sLink, "</div>");
            }
            else if (!String.IsNullOrEmpty(Separator) && iLinksLength > 0)
            {
				//If not vertical, then render the separator
                sLink = string.Concat(Separator, sLink);
            }
            return sLink;
        }

        private string AddLink(string strTabName, string strURL, string strCssClass)
        {
            return string.Format("<a class=\"{0}\" href=\"{1}\">{2}</a>", strCssClass, strURL, strTabName);
        }
		
		#endregion
    }
}
