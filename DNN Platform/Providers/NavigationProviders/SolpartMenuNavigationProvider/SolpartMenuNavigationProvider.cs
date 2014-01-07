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
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

using DotNetNuke.Common;
using DotNetNuke.Modules.NavigationProvider;
using DotNetNuke.UI.WebControls;

using Solpart.WebControls;

#endregion

namespace DotNetNuke.NavigationControl
{
    public class SolpartMenuNavigationProvider : NavigationProvider
    {
        private SolpartMenu m_objMenu;
        private string m_strControlID;
        private string m_strNodeLeftHTMLBreadCrumbRoot = "";
        private string m_strNodeLeftHTMLBreadCrumbSub = "";
        private string m_strNodeLeftHTMLRoot = "";
        private string m_strNodeLeftHTMLSub = "";
        private string m_strNodeRightHTMLBreadCrumbRoot = "";
        private string m_strNodeRightHTMLBreadCrumbSub = "";
        private string m_strNodeRightHTMLRoot = "";
        private string m_strNodeRightHTMLSub = "";
        private string m_strSeparatorHTML = "";
        private string m_strSeparatorLeftHTML = "";
        private string m_strSeparatorLeftHTMLActive = "";
        private string m_strSeparatorLeftHTMLBreadCrumb = "";
        private string m_strSeparatorRightHTML = "";
        private string m_strSeparatorRightHTMLActive = "";
        private string m_strSeparatorRightHTMLBreadCrumb = "";

        public SolpartMenu Menu
        {
            get
            {
                return m_objMenu;
            }
        }

        public override Control NavigationControl
        {
            get
            {
                return Menu;
            }
        }

        public override bool SupportsPopulateOnDemand
        {
            get
            {
                return false;
            }
        }

        public override string IndicateChildImageSub
        {
            get
            {
                return Menu.ArrowImage;
            }
            set
            {
                Menu.ArrowImage = value;
            }
        }

        public override string IndicateChildImageRoot
        {
            get
            {
                return Menu.RootArrowImage;
            }
            set
            {
                Menu.RootArrowImage = value;
            }
        }

        public override Alignment ControlAlignment
        {
            get
            {
                switch (Menu.MenuAlignment.ToLower())
                {
                    case "left":
                        return Alignment.Left;
                    case "right":
                        return Alignment.Right;
                    case "center":
                        return Alignment.Center;
                    default:
                        return Alignment.Justify;
                }
            }
            set
            {
                switch (value)
                {
                    case Alignment.Left:
                        Menu.MenuAlignment = "Left";
                        break;
                    case Alignment.Right:
                        Menu.MenuAlignment = "Right";
                        break;
                    case Alignment.Center:
                        Menu.MenuAlignment = "Center";
                        break;
                    case Alignment.Justify:
                        Menu.MenuAlignment = "Justify";
                        break;
                }
            }
        }

        public override string ControlID
        {
            get
            {
                return m_strControlID;
            }
            set
            {
                m_strControlID = value;
            }
        }

        public override Orientation ControlOrientation
        {
            get
            {
                if (Menu.Display.ToLower() == "horizontal")
                {
                    return Orientation.Horizontal;
                }
                else
                {
                    return Orientation.Vertical;
                }
            }
            set
            {
                if (value == Orientation.Horizontal)
                {
                    Menu.Display = "Horizontal";
                }
                else
                {
                    Menu.Display = "Vertical";
                }
            }
        }

        public override string CSSIndicateChildSub
        {
            get
            {
                return Menu.MenuCSS.MenuArrow;
            }
            set
            {
                Menu.MenuCSS.MenuArrow = value;
            }
        }

        public override string CSSIndicateChildRoot
        {
            get
            {
                return Menu.MenuCSS.RootMenuArrow;
            }
            set
            {
                Menu.MenuCSS.RootMenuArrow = value;
            }
        }

        public override string CSSBreadCrumbSub { get; set; }

        public override string CSSBreadCrumbRoot { get; set; }

        public override string CSSBreak
        {
            get
            {
                return Menu.MenuCSS.MenuBreak;
            }
            set
            {
                Menu.MenuCSS.MenuBreak = value;
            }
        }

        public override string CSSContainerRoot
        {
            get
            {
                return Menu.MenuCSS.MenuContainer;
            }
            set
            {
                Menu.MenuCSS.MenuContainer = value;
            }
        }

        public override string CSSControl
        {
            get
            {
                return Menu.MenuCSS.MenuBar;
            }
            set
            {
                Menu.MenuCSS.MenuBar = value;
            }
        }

        public override string CSSIcon
        {
            get
            {
                return Menu.MenuCSS.MenuIcon;
            }
            set
            {
                Menu.MenuCSS.MenuIcon = value;
            }
        }

        public override string CSSLeftSeparator { get; set; }

        public override string CSSLeftSeparatorBreadCrumb { get; set; }

        public override string CSSLeftSeparatorSelection { get; set; }

        public override string CSSNode
        {
            get
            {
                return Menu.MenuCSS.MenuItem;
            }
            set
            {
                Menu.MenuCSS.MenuItem = value;
            }
        }

        public override string CSSNodeSelectedSub { get; set; }

        public override string CSSNodeSelectedRoot { get; set; }

        public override string CSSNodeHover
        {
            get
            {
                return Menu.MenuCSS.MenuItemSel;
            }
            set
            {
                Menu.MenuCSS.MenuItemSel = value;
            }
        }

        public override string CSSNodeRoot { get; set; }

        public override string CSSNodeHoverSub { get; set; }

        public override string CSSNodeHoverRoot { get; set; }

        public override string CSSRightSeparator { get; set; }

        public override string CSSRightSeparatorBreadCrumb { get; set; }

        public override string CSSRightSeparatorSelection { get; set; }

        public override string CSSSeparator { get; set; }

        public override string CSSContainerSub
        {
            get
            {
                return Menu.MenuCSS.SubMenu;
            }
            set
            {
                Menu.MenuCSS.SubMenu = value;
            }
        }

        public override string ForceCrawlerDisplay
        {
            get
            {
                return Menu.ForceFullMenuList.ToString();
            }
            set
            {
                Menu.ForceFullMenuList = Convert.ToBoolean(value);
            }
        }

        public override string ForceDownLevel
        {
            get
            {
                return Menu.ForceDownlevel.ToString();
            }
            set
            {
                Menu.ForceDownlevel = Convert.ToBoolean(value);
            }
        }

        public override string PathImage
        {
            get
            {
                return Menu.IconImagesPath;
            }
            set
            {
                Menu.IconImagesPath = value;
            }
        }

        public override bool IndicateChildren { get; set; }

        public override string EffectsStyle
        {
            get
            {
                return Menu.MenuEffects.get_Style(false);
            }
            set
            {
                string.Concat(Menu.MenuEffects.get_Style(false), value);
            }
        }

        public override double EffectsDuration
        {
            get
            {
                return Menu.MenuEffects.MenuTransitionLength;
            }
            set
            {
                Menu.MenuEffects.MenuTransitionLength = value;
            }
        }

        public override string EffectsShadowColor
        {
            get
            {
                return ColorTranslator.ToHtml(Menu.ShadowColor);
            }
            set
            {
                Menu.ShadowColor = Color.FromName(value);
            }
        }

        public override string EffectsShadowDirection
        {
            get
            {
                return Menu.MenuEffects.ShadowDirection;
            }
            set
            {
                Menu.MenuEffects.ShadowDirection = value;
            }
        }

        public override int EffectsShadowStrength
        {
            get
            {
                return Menu.MenuEffects.ShadowStrength;
            }
            set
            {
                Menu.MenuEffects.ShadowStrength = value;
            }
        }

        public override string EffectsTransition
        {
            get
            {
                return Menu.MenuEffects.MenuTransition;
            }
            set
            {
                Menu.MenuEffects.MenuTransition = value;
            }
        }

        public override decimal MouseOutHideDelay
        {
            get
            {
                return Menu.MenuEffects.MouseOutHideDelay;
            }
            set
            {
                Menu.MenuEffects.MouseOutHideDelay = Convert.ToInt32(value);
            }
        }

        public override HoverAction MouseOverAction
        {
            get
            {
                if (Menu.MenuEffects.MouseOverExpand)
                {
                    return HoverAction.Expand;
                }
                else
                {
                    return HoverAction.None;
                }
            }
            set
            {
                if (value == HoverAction.Expand)
                {
                    Menu.MenuEffects.MouseOverExpand = true;
                }
                else
                {
                    Menu.MenuEffects.MouseOverExpand = false;
                }
            }
        }

        public override HoverDisplay MouseOverDisplay
        {
            get
            {
                switch (Menu.MenuEffects.MouseOverDisplay)
                {
                    case MenuEffectsMouseOverDisplay.Highlight:
                        return HoverDisplay.Highlight;
                    case MenuEffectsMouseOverDisplay.Outset:
                        return HoverDisplay.Outset;
                    default:
                        return HoverDisplay.None;
                }
            }
            set
            {
                switch (value)
                {
                    case HoverDisplay.Highlight:
                        Menu.MenuEffects.MouseOverDisplay = MenuEffectsMouseOverDisplay.Highlight;
                        break;
                    case HoverDisplay.Outset:
                        Menu.MenuEffects.MouseOverDisplay = MenuEffectsMouseOverDisplay.Outset;
                        break;
                    default:
                        Menu.MenuEffects.MouseOverDisplay = MenuEffectsMouseOverDisplay.None;
                        break;
                }
            }
        }

        public override string NodeLeftHTMLSub
        {
            get
            {
                return m_strNodeLeftHTMLSub;
            }
            set
            {
                m_strNodeLeftHTMLSub = value;
            }
        }

        public override string NodeLeftHTMLBreadCrumbSub
        {
            get
            {
                return m_strNodeLeftHTMLBreadCrumbSub;
            }
            set
            {
                m_strNodeLeftHTMLBreadCrumbSub = value;
            }
        }

        public override string NodeLeftHTMLBreadCrumbRoot
        {
            get
            {
                return m_strNodeLeftHTMLBreadCrumbRoot;
            }
            set
            {
                m_strNodeLeftHTMLBreadCrumbRoot = value;
            }
        }

        public override string NodeLeftHTMLRoot
        {
            get
            {
                return m_strNodeLeftHTMLRoot;
            }
            set
            {
                m_strNodeLeftHTMLRoot = value;
            }
        }

        public override string NodeRightHTMLSub
        {
            get
            {
                return m_strNodeRightHTMLSub;
            }
            set
            {
                m_strNodeRightHTMLSub = value;
            }
        }

        public override string NodeRightHTMLBreadCrumbSub
        {
            get
            {
                return m_strNodeRightHTMLBreadCrumbSub;
            }
            set
            {
                m_strNodeRightHTMLBreadCrumbSub = value;
            }
        }

        public override string NodeRightHTMLBreadCrumbRoot
        {
            get
            {
                return m_strNodeRightHTMLBreadCrumbRoot;
            }
            set
            {
                m_strNodeRightHTMLBreadCrumbRoot = value;
            }
        }

        public override string NodeRightHTMLRoot
        {
            get
            {
                return m_strNodeRightHTMLRoot;
            }
            set
            {
                m_strNodeRightHTMLRoot = value;
            }
        }

        public override string SeparatorHTML
        {
            get
            {
                return m_strSeparatorHTML;
            }
            set
            {
                m_strSeparatorHTML = value;
            }
        }

        public override string SeparatorLeftHTML
        {
            get
            {
                return m_strSeparatorLeftHTML;
            }
            set
            {
                m_strSeparatorLeftHTML = value;
            }
        }

        public override string SeparatorLeftHTMLActive
        {
            get
            {
                return m_strSeparatorLeftHTMLActive;
            }
            set
            {
                m_strSeparatorLeftHTMLActive = value;
            }
        }

        public override string SeparatorLeftHTMLBreadCrumb
        {
            get
            {
                return m_strSeparatorLeftHTMLBreadCrumb;
            }
            set
            {
                m_strSeparatorLeftHTMLBreadCrumb = value;
            }
        }

        public override string SeparatorRightHTML
        {
            get
            {
                return m_strSeparatorRightHTML;
            }
            set
            {
                m_strSeparatorRightHTML = value;
            }
        }

        public override string SeparatorRightHTMLActive
        {
            get
            {
                return m_strSeparatorRightHTMLActive;
            }
            set
            {
                m_strSeparatorRightHTMLActive = value;
            }
        }

        public override string SeparatorRightHTMLBreadCrumb
        {
            get
            {
                return m_strSeparatorRightHTMLBreadCrumb;
            }
            set
            {
                m_strSeparatorRightHTMLBreadCrumb = value;
            }
        }

        public override string StyleBackColor
        {
            get
            {
                return ColorTranslator.ToHtml(Menu.BackColor);
            }
            set
            {
                Menu.BackColor = Color.FromName(value);
            }
        }

        public override decimal StyleBorderWidth
        {
            get
            {
                return Menu.MenuBorderWidth;
            }
            set
            {
                Menu.MenuBorderWidth = Convert.ToInt32(value);
            }
        }

        public override decimal StyleControlHeight
        {
            get
            {
                return Menu.MenuBarHeight;
            }
            set
            {
                Menu.MenuBarHeight = Convert.ToInt32(value);
            }
        }

        public override string StyleFontBold
        {
            get
            {
                return Menu.Font.Bold.ToString();
            }
            set
            {
                Menu.Font.Bold = Convert.ToBoolean(value);
            }
        }

        public override string StyleFontNames
        {
            get
            {
                return String.Join(";", Menu.Font.Names);
            }
            set
            {
                Menu.Font.Names = value.Split(Convert.ToChar(";"));
            }
        }

        public override decimal StyleFontSize
        {
            get
            {
                return Convert.ToDecimal(Menu.Font.Size.Unit.Value);
            }
            set
            {
                Menu.Font.Size = FontUnit.Parse(value.ToString());
            }
        }

        public override string StyleForeColor
        {
            get
            {
                return ColorTranslator.ToHtml(Menu.ForeColor);
            }
            set
            {
                Menu.ForeColor = Color.FromName(value);
            }
        }

        public override string StyleHighlightColor
        {
            get
            {
                return ColorTranslator.ToHtml(Menu.HighlightColor);
            }
            set
            {
                Menu.HighlightColor = Color.FromName(value);
            }
        }

        public override string StyleIconBackColor
        {
            get
            {
                return ColorTranslator.ToHtml(Menu.IconBackgroundColor);
            }
            set
            {
                Menu.IconBackgroundColor = Color.FromName(value);
            }
        }

        public override decimal StyleIconWidth
        {
            get
            {
                return Menu.IconWidth;
            }
            set
            {
                Menu.IconWidth = Convert.ToInt32(value);
            }
        }

        public override decimal StyleNodeHeight
        {
            get
            {
                return Menu.MenuItemHeight;
            }
            set
            {
                Menu.MenuItemHeight = Convert.ToInt32(value);
            }
        }

        public override string StyleSelectionBorderColor
        {
            get
            {
                return ColorTranslator.ToHtml(Menu.SelectedBorderColor);
            }
            set
            {
                if (value != null)
                {
                    Menu.SelectedBorderColor = Color.FromName(value);
                }
                else
                {
                    Menu.SelectedBorderColor = Color.Empty;
                }
            }
        }

        public override string StyleSelectionColor
        {
            get
            {
                return ColorTranslator.ToHtml(Menu.SelectedColor);
            }
            set
            {
                Menu.SelectedColor = Color.FromName(value);
            }
        }

        public override string StyleSelectionForeColor
        {
            get
            {
                return ColorTranslator.ToHtml(Menu.SelectedForeColor);
            }
            set
            {
                Menu.SelectedForeColor = Color.FromName(value);
            }
        }

        public override string StyleRoot { get; set; }

        public override string PathSystemImage
        {
            get
            {
                return Menu.SystemImagesPath;
            }
            set
            {
                Menu.SystemImagesPath = value;
            }
        }

        public override string PathSystemScript
        {
            get
            {
                return Menu.SystemScriptPath;
            }
            set
            {
                Menu.SystemScriptPath = value;
            }
        }

        public override void Initialize()
        {
            m_objMenu = new SolpartMenu();
            Menu.ID = m_strControlID;
            Menu.SeparateCSS = true;
            StyleSelectionBorderColor = null;
            m_objMenu.MenuClick += ctlActions_MenuClick;
        }

        private void ctlActions_MenuClick(string ID)
        {
            RaiseEvent_NodeClick(ID);
        }

        public override void Bind(DNNNodeCollection objNodes)
        {
            DNNNode objNode = null;
            SPMenuItemNode objMenuItem = null;
            DNNNode objPrevNode = null;
            bool RootFlag = false;
            if (IndicateChildren == false)
            {
            }
            else
            {
                if (!String.IsNullOrEmpty(IndicateChildImageRoot))
                {
                    Menu.RootArrow = true;
                }
            }
            foreach (DNNNode node in objNodes)
            {
                objNode = node;
                try
                {
                    if (objNode.Level == 0)
                    {
                        if (RootFlag)
                        {
                            AddSeparator("All", objPrevNode, objNode);
                        }
                        else
                        {
                            if (!String.IsNullOrEmpty(SeparatorLeftHTML) || !String.IsNullOrEmpty(SeparatorLeftHTMLBreadCrumb) || !String.IsNullOrEmpty(SeparatorLeftHTMLActive))
                            {
                                AddSeparator("Left", objPrevNode, objNode);
                            }
                            RootFlag = true;
                        }
                        if (objNode.Enabled == false)
                        {
                            objMenuItem = new SPMenuItemNode(Menu.AddMenuItem(objNode.ID, objNode.Text, ""));
                        }
                        else
                        {
                            if (!String.IsNullOrEmpty(objNode.JSFunction))
                            {
                                objMenuItem = new SPMenuItemNode(Menu.AddMenuItem(objNode.ID, objNode.Text, GetClientScriptURL(objNode.JSFunction, objNode.ID)));
                            }
                            else
                            {
                                objMenuItem = new SPMenuItemNode(Menu.AddMenuItem(objNode.ID, objNode.Text, objNode.NavigateURL));
                            }
                        }
                        if (!String.IsNullOrEmpty(StyleRoot))
                        {
                            objMenuItem.ItemStyle = StyleRoot;
                        }
                        if (!String.IsNullOrEmpty(CSSNodeRoot))
                        {
                            objMenuItem.ItemCss = CSSNodeRoot;
                        }
                        if (!String.IsNullOrEmpty(CSSNodeHoverRoot))
                        {
                            objMenuItem.ItemSelectedCss = CSSNodeHoverRoot;
                        }
                        if (!String.IsNullOrEmpty(NodeLeftHTMLRoot))
                        {
                            objMenuItem.LeftHTML = NodeLeftHTMLRoot;
                        }
                        if (objNode.BreadCrumb)
                        {
                            objMenuItem.ItemCss = objMenuItem.ItemCss + " " + CSSBreadCrumbRoot;
                            if (!String.IsNullOrEmpty(NodeLeftHTMLBreadCrumbRoot))
                            {
                                objMenuItem.LeftHTML = NodeLeftHTMLBreadCrumbRoot;
                            }
                            if (!String.IsNullOrEmpty(NodeRightHTMLBreadCrumbRoot))
                            {
                                objMenuItem.RightHTML = NodeRightHTMLBreadCrumbRoot;
                            }
                            if (objNode.Selected)
                            {
                                objMenuItem.ItemCss = objMenuItem.ItemCss + " " + CSSNodeSelectedRoot;
                            }
                        }
                        if (!String.IsNullOrEmpty(NodeRightHTMLRoot))
                        {
                            objMenuItem.RightHTML = NodeRightHTMLRoot;
                        }
                    }
                    else if (objNode.IsBreak)
                    {
                        Menu.AddBreak(objNode.ParentNode.ID);
                    }
                    else
                    {
                        try
                        {
                            if (objNode.Enabled == false)
                            {
                                objMenuItem = new SPMenuItemNode(Menu.AddMenuItem(objNode.ParentNode.ID, objNode.ID, "&nbsp;" + objNode.Text, ""));
                            }
                            else
                            {
                                if (!String.IsNullOrEmpty(objNode.JSFunction))
                                {
                                    objMenuItem = new SPMenuItemNode(Menu.AddMenuItem(objNode.ParentNode.ID, objNode.ID, "&nbsp;" + objNode.Text, GetClientScriptURL(objNode.JSFunction, objNode.ID)));
                                }
                                else
                                {
                                    objMenuItem = new SPMenuItemNode(Menu.AddMenuItem(objNode.ParentNode.ID, objNode.ID, "&nbsp;" + objNode.Text, objNode.NavigateURL));
                                }
                            }
                            if (objNode.ClickAction == eClickAction.PostBack)
                            {
                                objMenuItem.RunAtServer = true;
                            }
                            if (!String.IsNullOrEmpty(CSSNodeHoverSub))
                            {
                                objMenuItem.ItemSelectedCss = CSSNodeHoverSub;
                            }
                            if (!String.IsNullOrEmpty(NodeLeftHTMLSub))
                            {
                                objMenuItem.LeftHTML = NodeLeftHTMLSub;
                            }
                            if (objNode.BreadCrumb)
                            {
                                objMenuItem.ItemCss = CSSBreadCrumbSub;
                                if (!String.IsNullOrEmpty(NodeLeftHTMLBreadCrumbSub))
                                {
                                    objMenuItem.LeftHTML = NodeLeftHTMLBreadCrumbSub;
                                }
                                if (!String.IsNullOrEmpty(NodeRightHTMLBreadCrumbSub))
                                {
                                    objMenuItem.RightHTML = NodeRightHTMLBreadCrumbSub;
                                }
                                if (objNode.Selected)
                                {
                                    objMenuItem.ItemCss = CSSNodeSelectedSub;
                                    DNNNode objParentNode = objNode;
                                    do
                                    {
                                        objParentNode = objParentNode.ParentNode;
                                        Menu.FindMenuItem(objParentNode.ID).ItemCss = CSSNodeSelectedSub;
                                    } while (objParentNode.Level != 0);
                                    Menu.FindMenuItem(objParentNode.ID).ItemCss = CSSBreadCrumbRoot + " " + CSSNodeSelectedRoot;
                                }
                            }
                            if (!String.IsNullOrEmpty(NodeRightHTMLSub))
                            {
                                objMenuItem.RightHTML = NodeRightHTMLSub;
                            }
                        }
                        catch
                        {
                            objMenuItem = null;
                        }
                    }
                    if (!String.IsNullOrEmpty(objNode.Image))
                    {
                        if (objNode.Image.StartsWith("~/images/"))
                        {
                            objMenuItem.Image = objNode.Image.Replace("~/images/", "");
                        }
                        else if (objNode.Image.IndexOf("/") > -1)
                        {
                            string strImage = objNode.Image;
                            if (strImage.StartsWith(Menu.IconImagesPath))
                            {
                                strImage = strImage.Substring(Menu.IconImagesPath.Length);
                            }
                            if (strImage.IndexOf("/") > -1)
                            {
                                objMenuItem.Image = strImage.Substring(strImage.LastIndexOf("/") + 1);
                                if (strImage.StartsWith("/"))
                                {
                                    objMenuItem.ImagePath = strImage.Substring(0, strImage.LastIndexOf("/") + 1);
                                }
                                else if (strImage.StartsWith("~/"))
                                {
                                    objMenuItem.ImagePath = Globals.ResolveUrl(strImage.Substring(0, strImage.LastIndexOf("/") + 1));
                                }
                                else
                                {
                                    objMenuItem.ImagePath = Menu.IconImagesPath + strImage.Substring(0, strImage.LastIndexOf("/") + 1);
                                }
                            }
                            else
                            {
                                objMenuItem.Image = strImage;
                            }
                        }
                        else
                        {
                            objMenuItem.Image = objNode.Image;
                        }
                    }
                    if (!String.IsNullOrEmpty(objNode.ToolTip))
                    {
                        objMenuItem.ToolTip = objNode.ToolTip;
                    }
                    Bind(objNode.DNNNodes);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                objPrevNode = objNode;
            }
            if (objNode != null && objNode.Level == 0)
            {
                if (!String.IsNullOrEmpty(SeparatorRightHTML) || !String.IsNullOrEmpty(SeparatorRightHTMLBreadCrumb) || !String.IsNullOrEmpty(SeparatorRightHTMLActive))
                {
                    AddSeparator("Right", objPrevNode, null);
                }
            }
        }

        private void AddSeparator(string strType, DNNNode objPrevNode, DNNNode objNextNode)
        {
            string strLeftHTML = SeparatorLeftHTML + SeparatorLeftHTMLBreadCrumb + SeparatorLeftHTMLActive;
            string strRightHTML = SeparatorRightHTML + SeparatorRightHTMLBreadCrumb + SeparatorRightHTMLActive;
            string strHTML = SeparatorHTML + strLeftHTML + strRightHTML;
            XmlNode objBreak;
            if (!String.IsNullOrEmpty(strHTML))
            {
                string strSeparatorTable = "";
                string strSeparator = "";
                string strSeparatorLeftHTML = "";
                string strSeparatorRightHTML = "";
                string strSeparatorClass = "";
                string strLeftSeparatorClass = "";
                string strRightSeparatorClass = "";
                if (!String.IsNullOrEmpty(strLeftHTML))
                {
                    strLeftSeparatorClass = GetSeparatorText(CSSLeftSeparator, CSSLeftSeparatorBreadCrumb, CSSLeftSeparatorSelection, objNextNode);
                    strSeparatorLeftHTML = GetSeparatorText(SeparatorLeftHTML, SeparatorLeftHTMLBreadCrumb, SeparatorLeftHTMLActive, objNextNode);
                }
                if (!String.IsNullOrEmpty(SeparatorHTML))
                {
                    if (!String.IsNullOrEmpty(CSSSeparator))
                    {
                        strSeparatorClass = CSSSeparator;
                    }
                    strSeparator = SeparatorHTML;
                }
                if (!String.IsNullOrEmpty(strRightHTML))
                {
                    strRightSeparatorClass = GetSeparatorText(CSSRightSeparator, CSSRightSeparatorBreadCrumb, CSSRightSeparatorSelection, objPrevNode);
                    strSeparatorRightHTML = GetSeparatorText(SeparatorRightHTML, SeparatorRightHTMLBreadCrumb, SeparatorRightHTMLActive, objPrevNode);
                }
                strSeparatorTable = "<table summary=\"Table for menu separator design\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\"><tr>";
                if (!String.IsNullOrEmpty(strSeparatorRightHTML) && strType != "Left")
                {
                    strSeparatorTable += GetSeparatorTD(strRightSeparatorClass, strSeparatorRightHTML);
                }
                if (!String.IsNullOrEmpty(strSeparator) && strType != "Left" && strType != "Right")
                {
                    strSeparatorTable += GetSeparatorTD(strSeparatorClass, strSeparator);
                }
                if (!String.IsNullOrEmpty(strSeparatorLeftHTML) && strType != "Right")
                {
                    strSeparatorTable += GetSeparatorTD(strLeftSeparatorClass, strSeparatorLeftHTML);
                }
                strSeparatorTable += "</tr></table>";
                objBreak = Menu.AddBreak("", strSeparatorTable);
            }
        }

        private string GetSeparatorText(string strNormal, string strBreadCrumb, string strSelection, DNNNode objNode)
        {
            string strRet = "";
            if (!String.IsNullOrEmpty(strNormal))
            {
                strRet = strNormal;
            }
            if (!String.IsNullOrEmpty(strBreadCrumb) && objNode != null && objNode.BreadCrumb)
            {
                strRet = strBreadCrumb;
            }
            if (!String.IsNullOrEmpty(strSelection) && objNode != null && objNode.Selected)
            {
                strRet = strSelection;
            }
            return strRet;
        }

        private string GetSeparatorTD(string strClass, string strHTML)
        {
            string strRet = "";
            if (!String.IsNullOrEmpty(strClass))
            {
                strRet += "<td class = \"" + strClass + "\">";
            }
            else
            {
                strRet += "<td>";
            }
            strRet += strHTML + "</td>";
            return strRet;
        }

        private string GetClientScriptURL(string strScript, string strID)
        {
            if (strScript.ToLower().StartsWith("javascript:") == false)
            {
                strScript = "javascript: " + strScript;
            }
            return strScript;
        }
    }
}