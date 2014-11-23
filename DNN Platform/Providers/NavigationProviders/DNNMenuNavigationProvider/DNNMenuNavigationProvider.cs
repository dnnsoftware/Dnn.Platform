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
using System.Collections.Generic;
using System.Web.UI;

using DotNetNuke.Common;
using DotNetNuke.Modules.NavigationProvider;
using DotNetNuke.UI.Skins;
using DotNetNuke.UI.Utilities.Animation;
using DotNetNuke.UI.WebControls;

#endregion

namespace DotNetNuke.NavigationControl
{
    public class DNNMenuNavigationProvider : NavigationProvider
    {
        private List<CustomAttribute> m_objCustomAttributes = new List<CustomAttribute>();
        private DNNMenu m_objMenu;
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

        public DNNMenu Menu
        {
            get
            {
                return m_objMenu;
            }
        }

        //JH - 2/5/07 - support for custom attributes
        public override List<CustomAttribute> CustomAttributes
        {
            get
            {
                return m_objCustomAttributes;
            }
            set
            {
                m_objCustomAttributes = value;
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
                return true;
            }
        }

        public override string WorkImage
        {
            get
            {
                return Menu.WorkImage;
            }
            set
            {
                Menu.WorkImage = value;
            }
        }

        public override string IndicateChildImageSub
        {
            get
            {
                return Menu.ChildArrowImage;
            }
            set
            {
                Menu.ChildArrowImage = value;
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
                if (Menu.Orientation == UI.WebControls.Orientation.Horizontal)
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
                switch (value)
                {
                    case Orientation.Horizontal:
                        Menu.Orientation = UI.WebControls.Orientation.Horizontal;
                        break;
                    case Orientation.Vertical:
                        Menu.Orientation = UI.WebControls.Orientation.Vertical;
                        break;
                }
            }
        }

        public override string CSSIndicateChildSub
        {
            get
            {
                return "";
            }
            set
            {
            }
        }

        public override string CSSIndicateChildRoot
        {
            get
            {
                return "";
            }
            set
            {
            }
        }

        public override string CSSBreadCrumbSub { get; set; }

        public override string CSSBreadCrumbRoot { get; set; }

        public override string CSSBreak { get; set; }

        public override string CSSControl
        {
            get
            {
                return Menu.MenuBarCssClass;
            }
            set
            {
                Menu.MenuBarCssClass = value;
            }
        }

        public override string CSSIcon
        {
            get
            {
                return Menu.DefaultIconCssClass;
            }
            set
            {
                Menu.DefaultIconCssClass = value;
            }
        }

        public override string CSSNode
        {
            get
            {
                return Menu.DefaultNodeCssClass;
            }
            set
            {
                Menu.DefaultNodeCssClass = value;
            }
        }

        public override string CSSNodeSelectedSub { get; set; }

        public override string CSSNodeSelectedRoot { get; set; }

        public override string CSSNodeHover
        {
            get
            {
                return Menu.DefaultNodeCssClassOver;
            }
            set
            {
                Menu.DefaultNodeCssClassOver = value;
            }
        }

        public override string CSSNodeRoot { get; set; }

        public override string CSSNodeHoverSub { get; set; }

        public override string CSSNodeHoverRoot { get; set; }

        public override string CSSContainerSub
        {
            get
            {
                return Menu.MenuCssClass;
            }
            set
            {
                Menu.MenuCssClass = value;
            }
        }

        public override string ForceDownLevel
        {
            get
            {
                return Menu.ForceDownLevel.ToString();
            }
            set
            {
                Menu.ForceDownLevel = Convert.ToBoolean(value);
            }
        }

        public override bool IndicateChildren { get; set; }

        public override bool PopulateNodesFromClient
        {
            get
            {
                return Menu.PopulateNodesFromClient;
            }
            set
            {
                Menu.PopulateNodesFromClient = value;
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

        public override string PathImage { get; set; }

        public override string PathSystemScript
        {
            get
            {
                return Menu.MenuScriptPath;
            }
            set
            {
            }
        }

        public override void Initialize()
        {
            m_objMenu = new DNNMenu();
            Menu.ID = m_strControlID;
            Menu.EnableViewState = false;
            Menu.NodeClick += DNNMenu_NodeClick;
            Menu.PopulateOnDemand += DNNMenu_PopulateOnDemand;
        }

        public override void Bind(DNNNodeCollection objNodes)
        {
            DNNNode objNode = null;
            MenuNode objMenuItem;
            DNNNode objPrevNode = null;
            bool RootFlag = false;
            int intIndex;
            if (IndicateChildren == false)
            {
                IndicateChildImageSub = "";
                IndicateChildImageRoot = "";
            }
            if (!String.IsNullOrEmpty(CSSNodeSelectedRoot) && CSSNodeSelectedRoot == CSSNodeSelectedSub)
            {
                Menu.DefaultNodeCssClassSelected = CSSNodeSelectedRoot;	//set on parent, thus decreasing overall payload
            }
			
            //JH - 2/5/07 - support for custom attributes
            foreach (CustomAttribute objAttr in CustomAttributes)
            {
                switch (objAttr.Name.ToLower())
                {
                    case "submenuorientation":
                        Menu.SubMenuOrientation = (UI.WebControls.Orientation) Enum.Parse(Menu.SubMenuOrientation.GetType(), objAttr.Value);
                        break;
                    case "usetables":
                        Menu.RenderMode = DNNMenu.MenuRenderMode.Normal;
                        break;
                    case "rendermode":
                        Menu.RenderMode = (DNNMenu.MenuRenderMode) Enum.Parse(typeof (DNNMenu.MenuRenderMode), objAttr.Value);
                        break;
                    case "animationtype":
                        Menu.Animation.AnimationType = (AnimationType) Enum.Parse(typeof (AnimationType), objAttr.Value);
                        break;
                    case "easingdirection":
                        Menu.Animation.EasingDirection = (EasingDirection) Enum.Parse(typeof (EasingDirection), objAttr.Value);
                        break;
                    case "easingtype":
                        Menu.Animation.EasingType = (EasingType) Enum.Parse(typeof (EasingType), objAttr.Value);
                        break;
                    case "animationinterval":
                        Menu.Animation.Interval = int.Parse(objAttr.Value);
                        break;
                    case "animationlength":
                        Menu.Animation.Length = int.Parse(objAttr.Value);
                        break;
                }
            }
            foreach (DNNNode node in objNodes)
            {
                objNode = node;
                if (objNode.Level == 0) //root menu
                {
                    intIndex = Menu.MenuNodes.Import(objNode, false);
                    objMenuItem = Menu.MenuNodes[intIndex];
                    if (objNode.BreadCrumb && string.IsNullOrEmpty(NodeRightHTMLBreadCrumbRoot) == false)
                    {
                        objMenuItem.RightHTML += NodeRightHTMLBreadCrumbRoot;
                    }
                    else if (string.IsNullOrEmpty(NodeRightHTMLRoot) == false)
                    {
                        objMenuItem.RightHTML = NodeRightHTMLRoot;
                    }
                    if (RootFlag) //first root item has already been entered
                    {
                        AddSeparator("All", objPrevNode, objNode, objMenuItem);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(SeparatorLeftHTML) == false || string.IsNullOrEmpty(SeparatorLeftHTMLBreadCrumb) == false || string.IsNullOrEmpty(SeparatorLeftHTMLActive) == false)
                        {
                            AddSeparator("Left", objPrevNode, objNode, objMenuItem);
                        }
                        RootFlag = true;
                    }
                    if (objNode.BreadCrumb && string.IsNullOrEmpty(NodeLeftHTMLBreadCrumbRoot) == false)
                    {
                        objMenuItem.LeftHTML += NodeLeftHTMLBreadCrumbRoot;
                    }
                    else if (string.IsNullOrEmpty(NodeLeftHTMLRoot) == false)
                    {
                        objMenuItem.LeftHTML += NodeLeftHTMLRoot;
                    }
                    if (!String.IsNullOrEmpty(CSSNodeRoot))
                    {
                        objMenuItem.CSSClass = CSSNodeRoot;
                    }
                    if (!String.IsNullOrEmpty(CSSNodeHoverRoot) && CSSNodeHoverRoot != CSSNodeHoverSub)
                    {
                        objMenuItem.CSSClassHover = CSSNodeHoverRoot;
                    }
                    objMenuItem.CSSIcon = " "; //< ignore for root...???
                    if (objNode.BreadCrumb)
                    {
                        if (!String.IsNullOrEmpty(CSSBreadCrumbRoot))
                        {
                            objMenuItem.CSSClass = CSSBreadCrumbRoot;
                        }
                        if (objNode.Selected && String.IsNullOrEmpty(Menu.DefaultNodeCssClassSelected))
                        {
                            objMenuItem.CSSClassSelected = CSSNodeSelectedRoot;
                        }
                    }
                }
                else //If Not blnRootOnly Then
                {
                    try
                    {
                        MenuNode objParent = Menu.MenuNodes.FindNode(objNode.ParentNode.ID);
                        if (objParent == null) //POD
                        {
                            objParent = Menu.MenuNodes[Menu.MenuNodes.Import(objNode.ParentNode.Clone(), true)];
                        }
                        objMenuItem = objParent.MenuNodes.FindNode(objNode.ID);
                        if (objMenuItem == null) //POD
                        {
                            objMenuItem = objParent.MenuNodes[objParent.MenuNodes.Import(objNode.Clone(), true)];
                        }
                        if (!String.IsNullOrEmpty(NodeLeftHTMLSub))
                        {
                            objMenuItem.LeftHTML = NodeLeftHTMLSub;
                        }
                        if (!String.IsNullOrEmpty(NodeRightHTMLSub))
                        {
                            objMenuItem.RightHTML = NodeRightHTMLSub;
                        }
                        if (!String.IsNullOrEmpty(CSSNodeHoverSub) && CSSNodeHoverRoot != CSSNodeHoverSub)
                        {
                            objMenuItem.CSSClassHover = CSSNodeHoverSub;
                        }
                        if (objNode.BreadCrumb)
                        {
                            if (!String.IsNullOrEmpty(CSSBreadCrumbSub))
                            {
                                objMenuItem.CSSClass = CSSBreadCrumbSub;
                            }
                            if (!String.IsNullOrEmpty(NodeLeftHTMLBreadCrumbSub))
                            {
                                objMenuItem.LeftHTML = NodeLeftHTMLBreadCrumbSub;
                            }
                            if (!String.IsNullOrEmpty(NodeRightHTMLBreadCrumbSub))
                            {
                                objMenuItem.RightHTML = NodeRightHTMLBreadCrumbSub;
                            }
                            if (objNode.Selected && String.IsNullOrEmpty(Menu.DefaultNodeCssClassSelected))
                            {
                                objMenuItem.CSSClass = CSSNodeSelectedSub;
                            }
                        }
                    }
                    catch
                    {
                        //throws exception if the parent tab has not been loaded ( may be related to user role security not allowing access to a parent tab )
                        objMenuItem = null;
                    }
                }
                if (!String.IsNullOrEmpty(objNode.Image))
                {
                    if (objNode.Image.StartsWith("~/images/"))
                    {
                        objNode.Image = objNode.Image.Replace("~/images/", PathSystemImage);
                    }
                    else if (objNode.Image.StartsWith("~/"))
                    {
                        objNode.Image = Globals.ResolveUrl(objNode.Image);
                    }
                    else if (!objNode.Image.Contains("://") && objNode.Image.StartsWith("/") == false && !String.IsNullOrEmpty(PathImage))
                    {
                        objNode.Image = PathImage + objNode.Image;
                    }
                    objMenuItem.Image = objNode.Image;
                }
                if (objMenuItem.IsBreak)
                {
                    objMenuItem.CSSClass = CSSBreak;
                }
                objMenuItem.ToolTip = objNode.ToolTip;
                Bind(objNode.DNNNodes);
                objPrevNode = objNode;
            }
            if (objNode != null && objNode.Level == 0) //root menu
            {
                //solpartactions has a hardcoded image with no path information.  Assume if value is present and no path we need to add one.
                if (!String.IsNullOrEmpty(IndicateChildImageSub) && IndicateChildImageSub.IndexOf("/") == -1)
                {
                    IndicateChildImageSub = PathSystemImage + IndicateChildImageSub;
                }
                if (!String.IsNullOrEmpty(IndicateChildImageRoot) && IndicateChildImageRoot.IndexOf("/") == -1)
                {
                    IndicateChildImageRoot = PathSystemImage + IndicateChildImageRoot;
                }
            }
        }

        private void AddSeparator(string strType, DNNNode objPrevNode, DNNNode objNextNode, MenuNode objMenuItem)
        {
            string strLeftHTML = SeparatorLeftHTML + SeparatorLeftHTMLBreadCrumb + SeparatorLeftHTMLActive;
            string strRightHTML = SeparatorRightHTML + SeparatorRightHTMLBreadCrumb + SeparatorRightHTMLActive;
            string strHTML = SeparatorHTML + strLeftHTML + strRightHTML;
            if (!String.IsNullOrEmpty(strHTML))
            {
                string strSeparator = "";
                string strSeparatorLeftHTML = "";
                string strSeparatorRightHTML = "";
                string strSeparatorClass = "";
                string strLeftSeparatorClass = "";
                string strRightSeparatorClass = "";
                if (string.IsNullOrEmpty(strLeftHTML) == false)
                {
                    strLeftSeparatorClass = GetSeparatorText(CSSLeftSeparator, CSSLeftSeparatorBreadCrumb, CSSLeftSeparatorSelection, objNextNode);
                    strSeparatorLeftHTML = GetSeparatorText(SeparatorLeftHTML, SeparatorLeftHTMLBreadCrumb, SeparatorLeftHTMLActive, objNextNode);
                }
                if (string.IsNullOrEmpty(SeparatorHTML) == false)
                {
                    if (!String.IsNullOrEmpty(CSSSeparator))
                    {
                        strSeparatorClass = CSSSeparator;
                    }
                    strSeparator = SeparatorHTML;
                }
                if (string.IsNullOrEmpty(strRightHTML) == false)
                {
                    strRightSeparatorClass = GetSeparatorText(CSSRightSeparator, CSSRightSeparatorBreadCrumb, CSSRightSeparatorSelection, objNextNode);
                    strSeparatorRightHTML = GetSeparatorText(SeparatorRightHTML, SeparatorRightHTMLBreadCrumb, SeparatorRightHTMLActive, objNextNode);
                }
                if (string.IsNullOrEmpty(strSeparatorRightHTML) == false)
                {
                    objMenuItem.RightHTML += GetSeparatorMarkup(strRightSeparatorClass, strSeparatorRightHTML);
                }
                if (string.IsNullOrEmpty(strSeparator) == false && strType == "All")
                {
                    objMenuItem.LeftHTML += GetSeparatorMarkup(strSeparatorClass, strSeparator);
                }
                if (string.IsNullOrEmpty(strSeparatorLeftHTML) == false)
                {
                    objMenuItem.LeftHTML += GetSeparatorMarkup(strLeftSeparatorClass, strSeparatorLeftHTML);
                }
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

        private string GetSeparatorMarkup(string strClass, string strHTML)
        {
            string strRet = "";
            if (string.IsNullOrEmpty(strClass) == false)
            {
                strRet += "<span class=\"" + strClass + "\">" + strHTML + "</span>";
            }
            else
            {
                strRet += strHTML;
            }
            return strRet;
        }

        private void DNNMenu_NodeClick(object source, DNNMenuNodeClickEventArgs e)
        {
            base.RaiseEvent_NodeClick(e.Node);
        }

        private void DNNMenu_PopulateOnDemand(object source, DNNMenuEventArgs e)
        {
            base.RaiseEvent_PopulateOnDemand(e.Node);
        }

        public override void ClearNodes()
        {
            Menu.MenuNodes.Clear();
        }
    }
}