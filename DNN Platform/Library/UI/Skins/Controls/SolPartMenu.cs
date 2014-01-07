#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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

using DotNetNuke.Common;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.UI.WebControls;

#endregion

namespace DotNetNuke.UI.Skins.Controls
{
    /// -----------------------------------------------------------------------------
    /// <summary></summary>
    /// <remarks></remarks>
    /// <history>
    /// 	[cniknet]	10/15/2004	Replaced public members with properties and removed
    ///                             brackets from property names
    /// </history>
    /// -----------------------------------------------------------------------------
    public class SolPartMenu : NavObjectBase
    {
		#region "Public Members"
		
        public string SeparateCss { get; set; }

        public string MenuBarCssClass
        {
            get
            {
                return CSSControl;
            }
            set
            {
                CSSControl = value;
            }
        }

        public string MenuContainerCssClass
        {
            get
            {
                return CSSContainerRoot;
            }
            set
            {
                CSSContainerRoot = value;
            }
        }

        public string MenuItemCssClass
        {
            get
            {
                return CSSNode;
            }
            set
            {
                CSSNode = value;
            }
        }

        public string MenuIconCssClass
        {
            get
            {
                return CSSIcon;
            }
            set
            {
                CSSIcon = value;
            }
        }

        public string SubMenuCssClass
        {
            get
            {
                return CSSContainerSub;
            }
            set
            {
                CSSContainerSub = value;
            }
        }

        public string MenuItemSelCssClass
        {
            get
            {
                return CSSNodeHover;
            }
            set
            {
                CSSNodeHover = value;
            }
        }

        public string MenuBreakCssClass
        {
            get
            {
                return CSSBreak;
            }
            set
            {
                CSSBreak = value;
            }
        }

        public string MenuArrowCssClass
        {
            get
            {
                return CSSIndicateChildSub;
            }
            set
            {
                CSSIndicateChildSub = value;
            }
        }

        public string MenuRootArrowCssClass
        {
            get
            {
                return CSSIndicateChildRoot;
            }
            set
            {
                CSSIndicateChildRoot = value;
            }
        }

        public string BackColor
        {
            get
            {
                return StyleBackColor;
            }
            set
            {
                StyleBackColor = value;
            }
        }

        public string ForeColor
        {
            get
            {
                return StyleForeColor;
            }
            set
            {
                StyleForeColor = value;
            }
        }

        public string HighlightColor
        {
            get
            {
                return StyleHighlightColor;
            }
            set
            {
                StyleHighlightColor = value;
            }
        }

        public string IconBackgroundColor
        {
            get
            {
                return StyleIconBackColor;
            }
            set
            {
                StyleIconBackColor = value;
            }
        }

        public string SelectedBorderColor
        {
            get
            {
                return StyleSelectionBorderColor;
            }
            set
            {
                StyleSelectionBorderColor = value;
            }
        }

        public string SelectedColor
        {
            get
            {
                return StyleSelectionColor;
            }
            set
            {
                StyleSelectionColor = value;
            }
        }

        public string SelectedForeColor
        {
            get
            {
                return StyleSelectionForeColor;
            }
            set
            {
                StyleSelectionForeColor = value;
            }
        }

        public string Display
        {
            get
            {
                return ControlOrientation;
            }
            set
            {
                ControlOrientation = value;
            }
        }

        public string MenuBarHeight
        {
            get
            {
                return StyleControlHeight;
            }
            set
            {
                StyleControlHeight = value;
            }
        }

        public string MenuBorderWidth
        {
            get
            {
                return StyleBorderWidth;
            }
            set
            {
                StyleBorderWidth = value;
            }
        }

        public string MenuItemHeight
        {
            get
            {
                return StyleNodeHeight;
            }
            set
            {
                StyleNodeHeight = value;
            }
        }

        public string Moveable
        {
            get
            {
                return string.Empty;
            }
            set
            {
            }
        }

        public string IconWidth
        {
            get
            {
                return StyleIconWidth;
            }
            set
            {
                StyleIconWidth = value;
            }
        }

        public string MenuEffectsShadowColor
        {
            get
            {
                return EffectsShadowColor;
            }
            set
            {
                EffectsShadowColor = value;
            }
        }

        public string MenuEffectsMouseOutHideDelay
        {
            get
            {
                return MouseOutHideDelay;
            }
            set
            {
                MouseOutHideDelay = value;
            }
        }

        public string MenuEffectsMouseOverDisplay
        {
            get
            {
                return MouseOverDisplay;
            }
            set
            {
                MouseOverDisplay = value;
            }
        }

        public string MenuEffectsMouseOverExpand
        {
            get
            {
                return MouseOverAction;
            }
            set
            {
                MouseOverAction = value;
            }
        }

        public string MenuEffectsStyle
        {
            get
            {
                return EffectsStyle;
            }
            set
            {
                EffectsStyle = value;
            }
        }

        public string FontNames
        {
            get
            {
                return StyleFontNames;
            }
            set
            {
                StyleFontNames = value;
            }
        }

        public string FontSize
        {
            get
            {
                return StyleFontSize;
            }
            set
            {
                StyleFontSize = value;
            }
        }

        public string FontBold
        {
            get
            {
                return StyleFontBold;
            }
            set
            {
                StyleFontBold = value;
            }
        }

        public string MenuEffectsShadowStrength
        {
            get
            {
                return EffectsShadowStrength;
            }
            set
            {
                EffectsShadowStrength = value;
            }
        }

        public string MenuEffectsMenuTransition
        {
            get
            {
                return EffectsTransition;
            }
            set
            {
                EffectsTransition = value;
            }
        }

        public string MenuEffectsMenuTransitionLength
        {
            get
            {
                return EffectsDuration;
            }
            set
            {
                EffectsDuration = value;
            }
        }

        public string MenuEffectsShadowDirection
        {
            get
            {
                return EffectsShadowDirection;
            }
            set
            {
                EffectsShadowDirection = value;
            }
        }

        public string ForceFullMenuList
        {
            get
            {
                return ForceCrawlerDisplay;
            }
            set
            {
                ForceCrawlerDisplay = value;
            }
        }

        public string UseSkinPathArrowImages { get; set; }

        public string UseRootBreadCrumbArrow { get; set; }

        public string UseSubMenuBreadCrumbArrow { get; set; }

        public string RootMenuItemBreadCrumbCssClass
        {
            get
            {
                return CSSBreadCrumbRoot;
            }
            set
            {
                CSSBreadCrumbRoot = value;
            }
        }

        public string SubMenuItemBreadCrumbCssClass
        {
            get
            {
                return CSSBreadCrumbSub;
            }
            set
            {
                CSSBreadCrumbSub = value;
            }
        }

        public string RootMenuItemCssClass
        {
            get
            {
                return CSSNodeRoot;
            }
            set
            {
                CSSNodeRoot = value;
            }
        }

        public string RootBreadCrumbArrow { get; set; }

        public string SubMenuBreadCrumbArrow { get; set; }

        public string UseArrows
        {
            get
            {
                return IndicateChildren;
            }
            set
            {
                IndicateChildren = value;
            }
        }

        public string DownArrow { get; set; }

        public string RightArrow { get; set; }

        public string RootMenuItemActiveCssClass
        {
            get
            {
                return CSSNodeSelectedRoot;
            }
            set
            {
                CSSNodeSelectedRoot = value;
            }
        }

        public string SubMenuItemActiveCssClass
        {
            get
            {
                return CSSNodeSelectedSub;
            }
            set
            {
                CSSNodeSelectedSub = value;
            }
        }

        public string RootMenuItemSelectedCssClass
        {
            get
            {
                return CSSNodeHoverRoot;
            }
            set
            {
                CSSNodeHoverRoot = value;
            }
        }

        public string SubMenuItemSelectedCssClass
        {
            get
            {
                return CSSNodeHoverSub;
            }
            set
            {
                CSSNodeHoverSub = value;
            }
        }

        public string Separator
        {
            get
            {
                return SeparatorHTML;
            }
            set
            {
                SeparatorHTML = value;
            }
        }

        public string SeparatorCssClass
        {
            get
            {
                return CSSSeparator;
            }
            set
            {
                CSSSeparator = value;
            }
        }

        public string RootMenuItemLeftHtml
        {
            get
            {
                return NodeLeftHTMLRoot;
            }
            set
            {
                NodeLeftHTMLRoot = value;
            }
        }

        public string RootMenuItemRightHtml
        {
            get
            {
                return NodeRightHTMLRoot;
            }
            set
            {
                NodeRightHTMLRoot = value;
            }
        }

        public string SubMenuItemLeftHtml
        {
            get
            {
                return NodeLeftHTMLSub;
            }
            set
            {
                NodeLeftHTMLSub = value;
            }
        }

        public string SubMenuItemRightHtml
        {
            get
            {
                return NodeRightHTMLSub;
            }
            set
            {
                NodeRightHTMLSub = value;
            }
        }

        public string LeftSeparator
        {
            get
            {
                return SeparatorLeftHTML;
            }
            set
            {
                SeparatorLeftHTML = value;
            }
        }

        public string RightSeparator
        {
            get
            {
                return SeparatorRightHTML;
            }
            set
            {
                SeparatorRightHTML = value;
            }
        }

        public string LeftSeparatorActive
        {
            get
            {
                return SeparatorLeftHTMLActive;
            }
            set
            {
                SeparatorLeftHTMLActive = value;
            }
        }

        public string RightSeparatorActive
        {
            get
            {
                return SeparatorRightHTMLActive;
            }
            set
            {
                SeparatorRightHTMLActive = value;
            }
        }

        public string LeftSeparatorBreadCrumb
        {
            get
            {
                return SeparatorLeftHTMLBreadCrumb;
            }
            set
            {
                SeparatorLeftHTMLBreadCrumb = value;
            }
        }

        public string RightSeparatorBreadCrumb
        {
            get
            {
                return SeparatorRightHTMLBreadCrumb;
            }
            set
            {
                SeparatorRightHTMLBreadCrumb = value;
            }
        }

        public string LeftSeparatorCssClass
        {
            get
            {
                return CSSLeftSeparator;
            }
            set
            {
                CSSLeftSeparator = value;
            }
        }

        public string RightSeparatorCssClass
        {
            get
            {
                return CSSRightSeparator;
            }
            set
            {
                CSSRightSeparator = value;
            }
        }

        public string LeftSeparatorActiveCssClass
        {
            get
            {
                return CSSLeftSeparatorSelection;
            }
            set
            {
                CSSLeftSeparatorSelection = value;
            }
        }

        public string RightSeparatorActiveCssClass
        {
            get
            {
                return CSSRightSeparatorSelection;
            }
            set
            {
                CSSRightSeparatorSelection = value;
            }
        }

        public string LeftSeparatorBreadCrumbCssClass
        {
            get
            {
                return CSSLeftSeparatorBreadCrumb;
            }
            set
            {
                CSSLeftSeparatorBreadCrumb = value;
            }
        }

        public string RightSeparatorBreadCrumbCssClass
        {
            get
            {
                return CSSRightSeparatorBreadCrumb;
            }
            set
            {
                CSSRightSeparatorBreadCrumb = value;
            }
        }

        public string MenuAlignment
        {
            get
            {
                return ControlAlignment;
            }
            set
            {
                ControlAlignment = value;
            }
        }

        public string ClearDefaults { get; set; }

        public string DelaySubmenuLoad
        {
            get
            {
                return string.Empty;
            }
            set
            {
            }
        }

        public string RootOnly { get; set; }
		
		#endregion
		
		#region "Protected Methods"
        
		/// <summary>
		/// The Page_Load server event handler on this page is used
        /// to populate the role information for the page
		/// </summary>
		protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);


            try
            {
                bool blnUseSkinPathArrowImages = bool.Parse(GetValue(UseSkinPathArrowImages, "False"));
                bool blnUseRootBreadcrumbArrow = bool.Parse(GetValue(UseRootBreadCrumbArrow, "True"));
                bool blnUseSubMenuBreadcrumbArrow = bool.Parse(GetValue(UseSubMenuBreadCrumbArrow, "False"));
                bool blnUseArrows = bool.Parse(GetValue(UseArrows, "True"));
                bool blnRootOnly = bool.Parse(GetValue(RootOnly, "False"));
                string strRootBreadcrumbArrow;
                string strSubMenuBreadcrumbArrow;
                string strRightArrow;
                string strDownArrow;
                if (blnRootOnly)
                {
                    blnUseArrows = false;
                    PopulateNodesFromClient = false;
                    StartTabId = -1;
                    ExpandDepth = 1;
                }
                var objSkins = new SkinController();
                //image for root menu breadcrumb marking
                if (!String.IsNullOrEmpty(RootBreadCrumbArrow))
                {
                    strRootBreadcrumbArrow = PortalSettings.ActiveTab.SkinPath + RootBreadCrumbArrow;
                }
                else
                {
                    strRootBreadcrumbArrow = Globals.ApplicationPath + "/images/breadcrumb.gif";
                }
				
				//image for submenu breadcrumb marking
                if (!String.IsNullOrEmpty(SubMenuBreadCrumbArrow))
                {
                    strSubMenuBreadcrumbArrow = PortalSettings.ActiveTab.SkinPath + SubMenuBreadCrumbArrow;
                }
                if (blnUseSubMenuBreadcrumbArrow)
                {
                    strSubMenuBreadcrumbArrow = Globals.ApplicationPath + "/images/breadcrumb.gif";
                    NodeLeftHTMLBreadCrumbSub = "<img alt=\"*\" BORDER=\"0\" src=\"" + strSubMenuBreadcrumbArrow + "\">";
                }
                if (blnUseRootBreadcrumbArrow)
                {
                    NodeLeftHTMLBreadCrumbRoot = "<img alt=\"*\" BORDER=\"0\" src=\"" + strRootBreadcrumbArrow + "\">";
                }
				
				//image for right facing arrow
                if (!String.IsNullOrEmpty(RightArrow))
                {
                    strRightArrow = RightArrow;
                }
                else
                {
                    strRightArrow = "breadcrumb.gif";
                }
                if (!String.IsNullOrEmpty(DownArrow))
                {
                    strDownArrow = DownArrow;
                }
                else
                {
                    strDownArrow = "menu_down.gif";
                }
				
				//Set correct image path for all separator images
                if (!String.IsNullOrEmpty(Separator))
                {
                    if (Separator.IndexOf("src=") != -1)
                    {
                        Separator = Separator.Replace("src=\"", "src=\"" + PortalSettings.ActiveTab.SkinPath);
                    }
                }
                if (!String.IsNullOrEmpty(LeftSeparator))
                {
                    if (LeftSeparator.IndexOf("src=") != -1)
                    {
                        LeftSeparator = LeftSeparator.Replace("src=\"", "src=\"" + PortalSettings.ActiveTab.SkinPath);
                    }
                }
                if (!String.IsNullOrEmpty(RightSeparator))
                {
                    if (RightSeparator.IndexOf("src=") != -1)
                    {
                        RightSeparator = RightSeparator.Replace("src=\"", "src=\"" + PortalSettings.ActiveTab.SkinPath);
                    }
                }
                if (!String.IsNullOrEmpty(LeftSeparatorBreadCrumb))
                {
                    if (LeftSeparatorBreadCrumb.IndexOf("src=") != -1)
                    {
                        LeftSeparatorBreadCrumb = LeftSeparatorBreadCrumb.Replace("src=\"", "src=\"" + PortalSettings.ActiveTab.SkinPath);
                    }
                }
                if (!String.IsNullOrEmpty(RightSeparatorBreadCrumb))
                {
                    if (RightSeparatorBreadCrumb.IndexOf("src=") != -1)
                    {
                        RightSeparatorBreadCrumb = RightSeparatorBreadCrumb.Replace("src=\"", "src=\"" + PortalSettings.ActiveTab.SkinPath);
                    }
                }
                if (!String.IsNullOrEmpty(LeftSeparatorActive))
                {
                    if (LeftSeparatorActive.IndexOf("src=") != -1)
                    {
                        LeftSeparatorActive = LeftSeparatorActive.Replace("src=\"", "src=\"" + PortalSettings.ActiveTab.SkinPath);
                    }
                }
                if (!String.IsNullOrEmpty(RightSeparatorActive))
                {
                    if (RightSeparatorActive.IndexOf("src=") != -1)
                    {
                        RightSeparatorActive = RightSeparatorActive.Replace("src=\"", "src=\"" + PortalSettings.ActiveTab.SkinPath);
                    }
                }
				
				//generate dynamic menu
                if (blnUseSkinPathArrowImages)
                {
                    PathSystemImage = PortalSettings.ActiveTab.SkinPath;
                }
                else
                {
                    PathSystemImage = Globals.ApplicationPath + "/images/";
                }
                if (String.IsNullOrEmpty(PathImage))
                {
                    PathImage = PortalSettings.HomeDirectory;
                }
                if (blnUseArrows)
                {
                    IndicateChildImageSub = strRightArrow;
                    if (ControlOrientation.ToLower() == "vertical") //NavigationProvider.NavigationProvider.Orientation.Vertical Then
                    {
                        IndicateChildImageRoot = strRightArrow;
                    }
                    else
                    {
                        IndicateChildImageRoot = strDownArrow;
                    }
                }
                else
                {
                    PathSystemImage = Globals.ApplicationPath + "/images/";
                    IndicateChildImageSub = "spacer.gif";
                }
                if (String.IsNullOrEmpty(PathSystemScript))
                {
                    PathSystemScript = Globals.ApplicationPath + "/controls/SolpartMenu/";
                }
                BuildNodes(null);
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void BuildNodes(DNNNode objNode)
        {
            DNNNodeCollection objNodes;
            objNodes = GetNavigationNodes(objNode);
            Control.ClearNodes(); //since we always bind we need to clear the nodes for providers that maintain their state
            Bind(objNodes);
        }

        private void SetAttributes()
        {
            SeparateCss = "1";
            if (bool.Parse(GetValue(ClearDefaults, "False")))
            {
            }
            else
            {
                if (String.IsNullOrEmpty(MouseOutHideDelay))
                {
                    MouseOutHideDelay = "500";
                }
                if (String.IsNullOrEmpty(MouseOverAction))
                {
                    MouseOverAction = true.ToString(); //NavigationProvider.NavigationProvider.HoverAction.Expand
                }
                if (String.IsNullOrEmpty(StyleBorderWidth))
                {
                    StyleBorderWidth = "0";
                }
                if (String.IsNullOrEmpty(StyleControlHeight))
                {
                    StyleControlHeight = "16";
                }
                if (String.IsNullOrEmpty(StyleNodeHeight))
                {
                    StyleNodeHeight = "21";
                }
                if (String.IsNullOrEmpty(StyleIconWidth))
                {
                    StyleIconWidth = "0";
                }
                if (String.IsNullOrEmpty(StyleSelectionColor))
                {
                    StyleSelectionColor = "#CCCCCC";
                }
                if (String.IsNullOrEmpty(StyleSelectionForeColor))
                {
                    StyleSelectionForeColor = "White";
                }
                if (String.IsNullOrEmpty(StyleHighlightColor))
                {
                    StyleHighlightColor = "#FF8080";
                }
                if (String.IsNullOrEmpty(StyleIconBackColor))
                {
                    StyleIconBackColor = "#333333";
                }
                if (String.IsNullOrEmpty(EffectsShadowColor))
                {
                    EffectsShadowColor = "#404040";
                }
                if (String.IsNullOrEmpty(MouseOverDisplay))
                {
                    MouseOverDisplay = "highlight"; //NavigationProvider.NavigationProvider.HoverDisplay.Highlight
                }
                if (String.IsNullOrEmpty(EffectsStyle))
                {
                    EffectsStyle = "filter:progid:DXImageTransform.Microsoft.Shadow(color='DimGray', Direction=135, Strength=3);";
                }
                if (String.IsNullOrEmpty(StyleFontSize))
                {
                    StyleFontSize = "9";
                }
                if (String.IsNullOrEmpty(StyleFontBold))
                {
                    StyleFontBold = "True";
                }
                if (String.IsNullOrEmpty(StyleFontNames))
                {
                    StyleFontNames = "Tahoma,Arial,Helvetica";
                }
                if (String.IsNullOrEmpty(StyleForeColor))
                {
                    StyleForeColor = "White";
                }
                if (String.IsNullOrEmpty(StyleBackColor))
                {
                    StyleBackColor = "#333333";
                }
                if (String.IsNullOrEmpty(PathSystemImage))
                {
                    PathSystemImage = "/";
                }
            }
            if (SeparateCss == "1")
            {
                if (!String.IsNullOrEmpty(MenuBarCssClass))
                {
                    CSSControl = MenuBarCssClass;
                }
                else
                {
                    CSSControl = "MainMenu_MenuBar";
                }
                if (!String.IsNullOrEmpty(MenuContainerCssClass))
                {
                    CSSContainerRoot = MenuContainerCssClass;
                }
                else
                {
                    CSSContainerRoot = "MainMenu_MenuContainer";
                }
                if (!String.IsNullOrEmpty(MenuItemCssClass))
                {
                    CSSNode = MenuItemCssClass;
                }
                else
                {
                    CSSNode = "MainMenu_MenuItem";
                }
                if (!String.IsNullOrEmpty(MenuIconCssClass))
                {
                    CSSIcon = MenuIconCssClass;
                }
                else
                {
                    CSSIcon = "MainMenu_MenuIcon";
                }
                if (!String.IsNullOrEmpty(SubMenuCssClass))
                {
                    CSSContainerSub = SubMenuCssClass;
                }
                else
                {
                    CSSContainerSub = "MainMenu_SubMenu";
                }
                if (!String.IsNullOrEmpty(MenuBreakCssClass))
                {
                    CSSBreak = MenuBreakCssClass;
                }
                else
                {
                    CSSBreak = "MainMenu_MenuBreak";
                }
                if (!String.IsNullOrEmpty(MenuItemSelCssClass))
                {
                    CSSNodeHover = MenuItemSelCssClass;
                }
                else
                {
                    CSSNodeHover = "MainMenu_MenuItemSel";
                }
                if (!String.IsNullOrEmpty(MenuArrowCssClass))
                {
                    CSSIndicateChildSub = MenuArrowCssClass;
                }
                else
                {
                    CSSIndicateChildSub = "MainMenu_MenuArrow";
                }
                if (!String.IsNullOrEmpty(MenuRootArrowCssClass))
                {
                    CSSIndicateChildRoot = MenuRootArrowCssClass;
                }
                else
                {
                    CSSIndicateChildRoot = "MainMenu_RootMenuArrow";
                }
            }
        }

        protected override void OnInit(EventArgs e)
        {
            SetAttributes();
            InitializeNavControl(this, "SolpartMenuNavigationProvider");
            base.OnInit(e);
        }
		
		#endregion
    }
}
