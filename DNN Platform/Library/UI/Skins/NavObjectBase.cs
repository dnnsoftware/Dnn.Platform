#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;

using DotNetNuke.Common;
using DotNetNuke.Modules.NavigationProvider;
using DotNetNuke.UI.WebControls;

#endregion

namespace DotNetNuke.UI.Skins
{
    public class NavObjectBase : SkinObjectBase
    {
		#region "Private Members"
		
        private readonly List<CustomAttribute> m_objCustomAttributes = new List<CustomAttribute>();
        private bool m_blnPopulateNodesFromClient = true;
        private int m_intExpandDepth = -1;
        private int m_intStartTabId = -1;
        private NavigationProvider m_objControl;
        private string m_strCSSBreadCrumbRoot;

        private string m_strCSSBreadCrumbSub;
        private string m_strCSSBreak;
        private string m_strCSSContainerRoot;
        private string m_strCSSContainerSub;
        private string m_strCSSControl;
        private string m_strCSSIcon;
        private string m_strCSSIndicateChildRoot;
        private string m_strCSSIndicateChildSub;
        private string m_strCSSLeftSeparator;
        private string m_strCSSLeftSeparatorBreadCrumb;
        private string m_strCSSLeftSeparatorSelection;
        private string m_strCSSNode;
        private string m_strCSSNodeHover;
        private string m_strCSSNodeHoverRoot;
        private string m_strCSSNodeHoverSub;
        private string m_strCSSNodeRoot;
        private string m_strCSSNodeSelectedRoot;
        private string m_strCSSNodeSelectedSub;
        private string m_strCSSRightSeparator;
        private string m_strCSSRightSeparatorBreadCrumb;
        private string m_strCSSRightSeparatorSelection;
        private string m_strCSSSeparator;
        private string m_strControlAlignment;
        private string m_strControlOrientation;
        private string m_strEffectsDuration;
        private string m_strEffectsShadowColor;
        private string m_strEffectsShadowDirection;
        private string m_strEffectsShadowStrength;
        private string m_strEffectsStyle;
        private string m_strEffectsTransition;
        private string m_strForceCrawlerDisplay;
        private string m_strForceDownLevel;
        private string m_strIndicateChildImageExpandedRoot;
        private string m_strIndicateChildImageExpandedSub;
        private string m_strIndicateChildImageRoot;
        private string m_strIndicateChildImageSub;
        private string m_strIndicateChildren;
        private string m_strLevel = "";
        private string m_strMouseOutHideDelay;
        private string m_strMouseOverAction;
        private string m_strMouseOverDisplay;
        private string m_strNodeLeftHTMLBreadCrumbRoot;
        private string m_strNodeLeftHTMLBreadCrumbSub;
        private string m_strNodeLeftHTMLRoot;
        private string m_strNodeLeftHTMLSub;
        private string m_strNodeRightHTMLBreadCrumbRoot;
        private string m_strNodeRightHTMLBreadCrumbSub;
        private string m_strNodeRightHTMLRoot;
        private string m_strNodeRightHTMLSub;
        private string m_strPathImage;
        private string m_strPathSystemImage;
        private string m_strPathSystemScript;
        private string m_strProviderName = "";
        private string m_strSeparatorHTML;
        private string m_strSeparatorLeftHTML;
        private string m_strSeparatorLeftHTMLActive;
        private string m_strSeparatorLeftHTMLBreadCrumb;
        private string m_strSeparatorRightHTML;
        private string m_strSeparatorRightHTMLActive;
        private string m_strSeparatorRightHTMLBreadCrumb;
        private string m_strStyleBackColor;
        private string m_strStyleBorderWidth;
        private string m_strStyleControlHeight;
        private string m_strStyleFontBold;
        private string m_strStyleFontNames;
        private string m_strStyleFontSize;
        private string m_strStyleForeColor;
        private string m_strStyleHighlightColor;
        private string m_strStyleIconBackColor;
        private string m_strStyleIconWidth;
        private string m_strStyleNodeHeight;
        private string m_strStyleSelectionBorderColor;
        private string m_strStyleSelectionColor;
        private string m_strStyleSelectionForeColor;
        private string m_strToolTip = "";
        private string m_strWorkImage;

		#endregion

		#region "Public Properties"
		//JH - 2/5/07 - support for custom attributes
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty)]
        public List<CustomAttribute> CustomAttributes
        {
            get
            {
                return m_objCustomAttributes;
            }
        }

        public bool ShowHiddenTabs { get; set; }
      
        public string ProviderName
        {
            get
            {
                return m_strProviderName;
            }
            set
            {
                m_strProviderName = value;
            }
        }

        protected NavigationProvider Control
        {
            get
            {
                return m_objControl;
            }
        }

        public string Level
        {
            get
            {
                return m_strLevel;
            }
            set
            {
                m_strLevel = value;
            }
        }

        public string ToolTip
        {
            get
            {
                return m_strToolTip;
            }
            set
            {
                m_strToolTip = value;
            }
        }

        public bool PopulateNodesFromClient
        {
            get
            {
                return m_blnPopulateNodesFromClient;
            }
            set
            {
                m_blnPopulateNodesFromClient = value;
            }
        }

        public int ExpandDepth
        {
            get
            {
                return m_intExpandDepth;
            }
            set
            {
                m_intExpandDepth = value;
            }
        }

        public int StartTabId
        {
            get
            {
                return m_intStartTabId;
            }
            set
            {
                m_intStartTabId = value;
            }
        }

        public string PathSystemImage
        {
            get
            {
                if (Control == null)
                {
                    return m_strPathSystemImage;
                }
                else
                {
                    return Control.PathSystemImage;
                }
            }
            set
            {
                value = GetPath(value);
                if (Control == null)
                {
                    m_strPathSystemImage = value;
                }
                else
                {
                    Control.PathSystemImage = value;
                }
            }
        }

        public string PathImage
        {
            get
            {
                if (Control == null)
                {
                    return m_strPathImage;
                }
                else
                {
                    return Control.PathImage;
                }
            }
            set
            {
                value = GetPath(value);
                if (Control == null)
                {
                    m_strPathImage = value;
                }
                else
                {
                    Control.PathImage = value;
                }
            }
        }

        public string WorkImage
        {
            get
            {
                if (Control == null)
                {
                    return m_strWorkImage;
                }
                else
                {
                    return Control.WorkImage;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strWorkImage = value;
                }
                else
                {
                    Control.WorkImage = value;
                }
            }
        }

        public string PathSystemScript
        {
            get
            {
                if (Control == null)
                {
                    return m_strPathSystemScript;
                }
                else
                {
                    return Control.PathSystemScript;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strPathSystemScript = value;
                }
                else
                {
                    Control.PathSystemScript = value;
                }
            }
        }

        public string ControlOrientation
        {
            get
            {
                string retValue = "";
                if (Control == null)
                {
                    retValue = m_strControlOrientation;
                }
                else
                {
                    switch (Control.ControlOrientation)
                    {
                        case NavigationProvider.Orientation.Horizontal:
                            retValue = "Horizontal";
                            break;
                        case NavigationProvider.Orientation.Vertical:
                            retValue = "Vertical";
                            break;
                    }
                }
                return retValue;
            }
            set
            {
                if (Control == null)
                {
                    m_strControlOrientation = value;
                }
                else
                {
                    switch (value.ToLower())
                    {
                        case "horizontal":
                            Control.ControlOrientation = NavigationProvider.Orientation.Horizontal;
                            break;
                        case "vertical":
                            Control.ControlOrientation = NavigationProvider.Orientation.Vertical;
                            break;
                    }
                }
            }
        }

        public string ControlAlignment
        {
            get
            {
                string retValue = "";
                if (Control == null)
                {
                    retValue = m_strControlAlignment;
                }
                else
                {
                    switch (Control.ControlAlignment)
                    {
                        case NavigationProvider.Alignment.Left:
                            retValue = "Left";
                            break;
                        case NavigationProvider.Alignment.Right:
                            retValue = "Right";
                            break;
                        case NavigationProvider.Alignment.Center:
                            retValue = "Center";
                            break;
                        case NavigationProvider.Alignment.Justify:
                            retValue = "Justify";
                            break;
                    }
                }
                return retValue;
            }
            set
            {
                if (Control == null)
                {
                    m_strControlAlignment = value;
                }
                else
                {
                    switch (value.ToLower())
                    {
                        case "left":
                            Control.ControlAlignment = NavigationProvider.Alignment.Left;
                            break;
                        case "right":
                            Control.ControlAlignment = NavigationProvider.Alignment.Right;
                            break;
                        case "center":
                            Control.ControlAlignment = NavigationProvider.Alignment.Center;
                            break;
                        case "justify":
                            Control.ControlAlignment = NavigationProvider.Alignment.Justify;
                            break;
                    }
                }
            }
        }

        public string ForceCrawlerDisplay
        {
            get
            {
                if (Control == null)
                {
                    return m_strForceCrawlerDisplay;
                }
                else
                {
                    return Control.ForceCrawlerDisplay;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strForceCrawlerDisplay = value;
                }
                else
                {
                    Control.ForceCrawlerDisplay = value;
                }
            }
        }

        public string ForceDownLevel
        {
            get
            {
                if (Control == null)
                {
                    return m_strForceDownLevel;
                }
                else
                {
                    return Control.ForceDownLevel;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strForceDownLevel = value;
                }
                else
                {
                    Control.ForceDownLevel = value;
                }
            }
        }

        public string MouseOutHideDelay
        {
            get
            {
                if (Control == null)
                {
                    return m_strMouseOutHideDelay;
                }
                else
                {
                    return Control.MouseOutHideDelay.ToString();
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strMouseOutHideDelay = value;
                }
                else
                {
                    Control.MouseOutHideDelay = Convert.ToDecimal(value);
                }
            }
        }

        public string MouseOverDisplay
        {
            get
            {
                string retValue = "";
                if (Control == null)
                {
                    retValue = m_strMouseOverDisplay;
                }
                else
                {
                    switch (Control.MouseOverDisplay)
                    {
                        case NavigationProvider.HoverDisplay.Highlight:
                            retValue = "Highlight";
                            break;
                        case NavigationProvider.HoverDisplay.None:
                            retValue = "None";
                            break;
                        case NavigationProvider.HoverDisplay.Outset:
                            retValue = "Outset";
                            break;
                    }
                }
                return retValue;
            }
            set
            {
                if (Control == null)
                {
                    m_strMouseOverDisplay = value;
                }
                else
                {
                    switch (value.ToLower())
                    {
                        case "highlight":
                            Control.MouseOverDisplay = NavigationProvider.HoverDisplay.Highlight;
                            break;
                        case "outset":
                            Control.MouseOverDisplay = NavigationProvider.HoverDisplay.Outset;
                            break;
                        case "none":
                            Control.MouseOverDisplay = NavigationProvider.HoverDisplay.None;
                            break;
                    }
                }
            }
        }

        public string MouseOverAction
        {
            get
            {
                string retValue = "";
                if (Control == null)
                {
                    retValue = m_strMouseOverAction;
                }
                else
                {
                    switch (Control.MouseOverAction)
                    {
                        case NavigationProvider.HoverAction.Expand:
                            retValue = "True";
                            break;
                        case NavigationProvider.HoverAction.None:
                            retValue = "False";
                            break;
                    }
                }
                return retValue;
            }
            set
            {
                if (Control == null)
                {
                    m_strMouseOverAction = value;
                }
                else
                {
                    if (Convert.ToBoolean(GetValue(value, "True")))
                    {
                        Control.MouseOverAction = NavigationProvider.HoverAction.Expand;
                    }
                    else
                    {
                        Control.MouseOverAction = NavigationProvider.HoverAction.None;
                    }
                }
            }
        }

        public string IndicateChildren
        {
            get
            {
                if (Control == null)
                {
                    return m_strIndicateChildren;
                }
                else
                {
                    return Control.IndicateChildren.ToString();
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strIndicateChildren = value;
                }
                else
                {
                    Control.IndicateChildren = Convert.ToBoolean(value);
                }
            }
        }

        public string IndicateChildImageRoot
        {
            get
            {
                if (Control == null)
                {
                    return m_strIndicateChildImageRoot;
                }
                else
                {
                    return Control.IndicateChildImageRoot;
                }
            }
            set
            {
                value = GetPath(value);
                if (Control == null)
                {
                    m_strIndicateChildImageRoot = value;
                }
                else
                {
                    Control.IndicateChildImageRoot = value;
                }
            }
        }

        public string IndicateChildImageSub
        {
            get
            {
                if (Control == null)
                {
                    return m_strIndicateChildImageSub;
                }
                else
                {
                    return Control.IndicateChildImageSub;
                }
            }
            set
            {
                value = GetPath(value);
                if (Control == null)
                {
                    m_strIndicateChildImageSub = value;
                }
                else
                {
                    Control.IndicateChildImageSub = value;
                }
            }
        }

        public string IndicateChildImageExpandedRoot
        {
            get
            {
                if (Control == null)
                {
                    return m_strIndicateChildImageExpandedRoot;
                }
                else
                {
                    return Control.IndicateChildImageExpandedRoot;
                }
            }
            set
            {
                value = GetPath(value);
                if (Control == null)
                {
                    m_strIndicateChildImageExpandedRoot = value;
                }
                else
                {
                    Control.IndicateChildImageExpandedRoot = value;
                }
            }
        }

        public string IndicateChildImageExpandedSub
        {
            get
            {
                if (Control == null)
                {
                    return m_strIndicateChildImageExpandedSub;
                }
                else
                {
                    return Control.IndicateChildImageExpandedSub;
                }
            }
            set
            {
                value = GetPath(value);
                if (Control == null)
                {
                    m_strIndicateChildImageExpandedSub = value;
                }
                else
                {
                    Control.IndicateChildImageExpandedSub = value;
                }
            }
        }

        public string NodeLeftHTMLRoot
        {
            get
            {
                if (Control == null)
                {
                    return m_strNodeLeftHTMLRoot;
                }
                else
                {
                    return Control.NodeLeftHTMLRoot;
                }
            }
            set
            {
                value = GetPath(value);
                if (Control == null)
                {
                    m_strNodeLeftHTMLRoot = value;
                }
                else
                {
                    Control.NodeLeftHTMLRoot = value;
                }
            }
        }

        public string NodeRightHTMLRoot
        {
            get
            {
                if (Control == null)
                {
                    return m_strNodeRightHTMLRoot;
                }
                else
                {
                    return Control.NodeRightHTMLRoot;
                }
            }
            set
            {
                value = GetPath(value);
                if (Control == null)
                {
                    m_strNodeRightHTMLRoot = value;
                }
                else
                {
                    Control.NodeRightHTMLRoot = value;
                }
            }
        }

        public string NodeLeftHTMLSub
        {
            get
            {
                if (Control == null)
                {
                    return m_strNodeLeftHTMLSub;
                }
                else
                {
                    return Control.NodeLeftHTMLSub;
                }
            }
            set
            {
                value = GetPath(value);
                if (Control == null)
                {
                    m_strNodeLeftHTMLSub = value;
                }
                else
                {
                    Control.NodeLeftHTMLSub = value;
                }
            }
        }

        public string NodeRightHTMLSub
        {
            get
            {
                if (Control == null)
                {
                    return m_strNodeRightHTMLSub;
                }
                else
                {
                    return Control.NodeRightHTMLSub;
                }
            }
            set
            {
                value = GetPath(value);
                if (Control == null)
                {
                    m_strNodeRightHTMLSub = value;
                }
                else
                {
                    Control.NodeRightHTMLSub = value;
                }
            }
        }

        public string NodeLeftHTMLBreadCrumbRoot
        {
            get
            {
                if (Control == null)
                {
                    return m_strNodeLeftHTMLBreadCrumbRoot;
                }
                else
                {
                    return Control.NodeLeftHTMLBreadCrumbRoot;
                }
            }
            set
            {
                value = GetPath(value);
                if (Control == null)
                {
                    m_strNodeLeftHTMLBreadCrumbRoot = value;
                }
                else
                {
                    Control.NodeLeftHTMLBreadCrumbRoot = value;
                }
            }
        }

        public string NodeLeftHTMLBreadCrumbSub
        {
            get
            {
                if (Control == null)
                {
                    return m_strNodeLeftHTMLBreadCrumbSub;
                }
                else
                {
                    return Control.NodeLeftHTMLBreadCrumbSub;
                }
            }
            set
            {
                value = GetPath(value);
                if (Control == null)
                {
                    m_strNodeLeftHTMLBreadCrumbSub = value;
                }
                else
                {
                    Control.NodeLeftHTMLBreadCrumbSub = value;
                }
            }
        }

        public string NodeRightHTMLBreadCrumbRoot
        {
            get
            {
                if (Control == null)
                {
                    return m_strNodeRightHTMLBreadCrumbRoot;
                }
                else
                {
                    return Control.NodeRightHTMLBreadCrumbRoot;
                }
            }
            set
            {
                value = GetPath(value);
                if (Control == null)
                {
                    m_strNodeRightHTMLBreadCrumbRoot = value;
                }
                else
                {
                    Control.NodeRightHTMLBreadCrumbRoot = value;
                }
            }
        }

        public string NodeRightHTMLBreadCrumbSub
        {
            get
            {
                if (Control == null)
                {
                    return m_strNodeRightHTMLBreadCrumbSub;
                }
                else
                {
                    return Control.NodeRightHTMLBreadCrumbSub;
                }
            }
            set
            {
                value = GetPath(value);
                if (Control == null)
                {
                    m_strNodeRightHTMLBreadCrumbSub = value;
                }
                else
                {
                    Control.NodeRightHTMLBreadCrumbSub = value;
                }
            }
        }

        public string SeparatorHTML
        {
            get
            {
                if (Control == null)
                {
                    return m_strSeparatorHTML;
                }
                else
                {
                    return Control.SeparatorHTML;
                }
            }
            set
            {
                value = GetPath(value);
                if (Control == null)
                {
                    m_strSeparatorHTML = value;
                }
                else
                {
                    Control.SeparatorHTML = value;
                }
            }
        }

        public string SeparatorLeftHTML
        {
            get
            {
                if (Control == null)
                {
                    return m_strSeparatorLeftHTML;
                }
                else
                {
                    return Control.SeparatorLeftHTML;
                }
            }
            set
            {
                value = GetPath(value);
                if (Control == null)
                {
                    m_strSeparatorLeftHTML = value;
                }
                else
                {
                    Control.SeparatorLeftHTML = value;
                }
            }
        }

        public string SeparatorRightHTML
        {
            get
            {
                if (Control == null)
                {
                    return m_strSeparatorRightHTML;
                }
                else
                {
                    return Control.SeparatorRightHTML;
                }
            }
            set
            {
                value = GetPath(value);
                if (Control == null)
                {
                    m_strSeparatorRightHTML = value;
                }
                else
                {
                    Control.SeparatorRightHTML = value;
                }
            }
        }

        public string SeparatorLeftHTMLActive
        {
            get
            {
                if (Control == null)
                {
                    return m_strSeparatorLeftHTMLActive;
                }
                else
                {
                    return Control.SeparatorLeftHTMLActive;
                }
            }
            set
            {
                value = GetPath(value);
                if (Control == null)
                {
                    m_strSeparatorLeftHTMLActive = value;
                }
                else
                {
                    Control.SeparatorLeftHTMLActive = value;
                }
            }
        }

        public string SeparatorRightHTMLActive
        {
            get
            {
                if (Control == null)
                {
                    return m_strSeparatorRightHTMLActive;
                }
                else
                {
                    return Control.SeparatorRightHTMLActive;
                }
            }
            set
            {
                value = GetPath(value);
                if (Control == null)
                {
                    m_strSeparatorRightHTMLActive = value;
                }
                else
                {
                    Control.SeparatorRightHTMLActive = value;
                }
            }
        }

        public string SeparatorLeftHTMLBreadCrumb
        {
            get
            {
                if (Control == null)
                {
                    return m_strSeparatorLeftHTMLBreadCrumb;
                }
                else
                {
                    return Control.SeparatorLeftHTMLBreadCrumb;
                }
            }
            set
            {
                value = GetPath(value);
                if (Control == null)
                {
                    m_strSeparatorLeftHTMLBreadCrumb = value;
                }
                else
                {
                    Control.SeparatorLeftHTMLBreadCrumb = value;
                }
            }
        }

        public string SeparatorRightHTMLBreadCrumb
        {
            get
            {
                if (Control == null)
                {
                    return m_strSeparatorRightHTMLBreadCrumb;
                }
                else
                {
                    return Control.SeparatorRightHTMLBreadCrumb;
                }
            }
            set
            {
                value = GetPath(value);
                if (Control == null)
                {
                    m_strSeparatorRightHTMLBreadCrumb = value;
                }
                else
                {
                    Control.SeparatorRightHTMLBreadCrumb = value;
                }
            }
        }

        public string CSSControl
        {
            get
            {
                if (Control == null)
                {
                    return m_strCSSControl;
                }
                else
                {
                    return Control.CSSControl;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strCSSControl = value;
                }
                else
                {
                    Control.CSSControl = value;
                }
            }
        }

        public string CSSContainerRoot
        {
            get
            {
                if (Control == null)
                {
                    return m_strCSSContainerRoot;
                }
                else
                {
                    return Control.CSSContainerRoot;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strCSSContainerRoot = value;
                }
                else
                {
                    Control.CSSContainerRoot = value;
                }
            }
        }

        public string CSSNode
        {
            get
            {
                if (Control == null)
                {
                    return m_strCSSNode;
                }
                else
                {
                    return Control.CSSNode;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strCSSNode = value;
                }
                else
                {
                    Control.CSSNode = value;
                }
            }
        }

        public string CSSIcon
        {
            get
            {
                if (Control == null)
                {
                    return m_strCSSIcon;
                }
                else
                {
                    return Control.CSSIcon;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strCSSIcon = value;
                }
                else
                {
                    Control.CSSIcon = value;
                }
            }
        }

        public string CSSContainerSub
        {
            get
            {
                if (Control == null)
                {
                    return m_strCSSContainerSub;
                }
                else
                {
                    return Control.CSSContainerSub;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strCSSContainerSub = value;
                }
                else
                {
                    Control.CSSContainerSub = value;
                }
            }
        }

        public string CSSNodeHover
        {
            get
            {
                if (Control == null)
                {
                    return m_strCSSNodeHover;
                }
                else
                {
                    return Control.CSSNodeHover;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strCSSNodeHover = value;
                }
                else
                {
                    Control.CSSNodeHover = value;
                }
            }
        }

        public string CSSBreak
        {
            get
            {
                if (Control == null)
                {
                    return m_strCSSBreak;
                }
                else
                {
                    return Control.CSSBreak;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strCSSBreak = value;
                }
                else
                {
                    Control.CSSBreak = value;
                }
            }
        }

        public string CSSIndicateChildSub
        {
            get
            {
                if (Control == null)
                {
                    return m_strCSSIndicateChildSub;
                }
                else
                {
                    return Control.CSSIndicateChildSub;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strCSSIndicateChildSub = value;
                }
                else
                {
                    Control.CSSIndicateChildSub = value;
                }
            }
        }

        public string CSSIndicateChildRoot
        {
            get
            {
                if (Control == null)
                {
                    return m_strCSSIndicateChildRoot;
                }
                else
                {
                    return Control.CSSIndicateChildRoot;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strCSSIndicateChildRoot = value;
                }
                else
                {
                    Control.CSSIndicateChildRoot = value;
                }
            }
        }

        public string CSSBreadCrumbRoot
        {
            get
            {
                if (Control == null)
                {
                    return m_strCSSBreadCrumbRoot;
                }
                else
                {
                    return Control.CSSBreadCrumbRoot;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strCSSBreadCrumbRoot = value;
                }
                else
                {
                    Control.CSSBreadCrumbRoot = value;
                }
            }
        }

        public string CSSBreadCrumbSub
        {
            get
            {
                if (Control == null)
                {
                    return m_strCSSBreadCrumbSub;
                }
                else
                {
                    return Control.CSSBreadCrumbSub;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strCSSBreadCrumbSub = value;
                }
                else
                {
                    Control.CSSBreadCrumbSub = value;
                }
            }
        }

        public string CSSNodeRoot
        {
            get
            {
                if (Control == null)
                {
                    return m_strCSSNodeRoot;
                }
                else
                {
                    return Control.CSSNodeRoot;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strCSSNodeRoot = value;
                }
                else
                {
                    Control.CSSNodeRoot = value;
                }
            }
        }

        public string CSSNodeSelectedRoot
        {
            get
            {
                if (Control == null)
                {
                    return m_strCSSNodeSelectedRoot;
                }
                else
                {
                    return Control.CSSNodeSelectedRoot;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strCSSNodeSelectedRoot = value;
                }
                else
                {
                    Control.CSSNodeSelectedRoot = value;
                }
            }
        }

        public string CSSNodeSelectedSub
        {
            get
            {
                if (Control == null)
                {
                    return m_strCSSNodeSelectedSub;
                }
                else
                {
                    return Control.CSSNodeSelectedSub;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strCSSNodeSelectedSub = value;
                }
                else
                {
                    Control.CSSNodeSelectedSub = value;
                }
            }
        }

        public string CSSNodeHoverRoot
        {
            get
            {
                if (Control == null)
                {
                    return m_strCSSNodeHoverRoot;
                }
                else
                {
                    return Control.CSSNodeHoverRoot;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strCSSNodeHoverRoot = value;
                }
                else
                {
                    Control.CSSNodeHoverRoot = value;
                }
            }
        }

        public string CSSNodeHoverSub
        {
            get
            {
                if (Control == null)
                {
                    return m_strCSSNodeHoverSub;
                }
                else
                {
                    return Control.CSSNodeHoverSub;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strCSSNodeHoverSub = value;
                }
                else
                {
                    Control.CSSNodeHoverSub = value;
                }
            }
        }

        public string CSSSeparator
        {
            get
            {
                if (Control == null)
                {
                    return m_strCSSSeparator;
                }
                else
                {
                    return Control.CSSSeparator;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strCSSSeparator = value;
                }
                else
                {
                    Control.CSSSeparator = value;
                }
            }
        }

        public string CSSLeftSeparator
        {
            get
            {
                if (Control == null)
                {
                    return m_strCSSLeftSeparator;
                }
                else
                {
                    return Control.CSSLeftSeparator;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strCSSLeftSeparator = value;
                }
                else
                {
                    Control.CSSLeftSeparator = value;
                }
            }
        }

        public string CSSRightSeparator
        {
            get
            {
                if (Control == null)
                {
                    return m_strCSSRightSeparator;
                }
                else
                {
                    return Control.CSSRightSeparator;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strCSSRightSeparator = value;
                }
                else
                {
                    Control.CSSRightSeparator = value;
                }
            }
        }

        public string CSSLeftSeparatorSelection
        {
            get
            {
                if (Control == null)
                {
                    return m_strCSSLeftSeparatorSelection;
                }
                else
                {
                    return Control.CSSLeftSeparatorSelection;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strCSSLeftSeparatorSelection = value;
                }
                else
                {
                    Control.CSSLeftSeparatorSelection = value;
                }
            }
        }

        public string CSSRightSeparatorSelection
        {
            get
            {
                if (Control == null)
                {
                    return m_strCSSRightSeparatorSelection;
                }
                else
                {
                    return Control.CSSRightSeparatorSelection;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strCSSRightSeparatorSelection = value;
                }
                else
                {
                    Control.CSSRightSeparatorSelection = value;
                }
            }
        }

        public string CSSLeftSeparatorBreadCrumb
        {
            get
            {
                if (Control == null)
                {
                    return m_strCSSLeftSeparatorBreadCrumb;
                }
                else
                {
                    return Control.CSSLeftSeparatorBreadCrumb;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strCSSLeftSeparatorBreadCrumb = value;
                }
                else
                {
                    Control.CSSLeftSeparatorBreadCrumb = value;
                }
            }
        }

        public string CSSRightSeparatorBreadCrumb
        {
            get
            {
                if (Control == null)
                {
                    return m_strCSSRightSeparatorBreadCrumb;
                }
                else
                {
                    return Control.CSSRightSeparatorBreadCrumb;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strCSSRightSeparatorBreadCrumb = value;
                }
                else
                {
                    Control.CSSRightSeparatorBreadCrumb = value;
                }
            }
        }

        public string StyleBackColor
        {
            get
            {
                if (Control == null)
                {
                    return m_strStyleBackColor;
                }
                else
                {
                    return Control.StyleBackColor;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strStyleBackColor = value;
                }
                else
                {
                    Control.StyleBackColor = value;
                }
            }
        }

        public string StyleForeColor
        {
            get
            {
                if (Control == null)
                {
                    return m_strStyleForeColor;
                }
                else
                {
                    return Control.StyleForeColor;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strStyleForeColor = value;
                }
                else
                {
                    Control.StyleForeColor = value;
                }
            }
        }

        public string StyleHighlightColor
        {
            get
            {
                if (Control == null)
                {
                    return m_strStyleHighlightColor;
                }
                else
                {
                    return Control.StyleHighlightColor;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strStyleHighlightColor = value;
                }
                else
                {
                    Control.StyleHighlightColor = value;
                }
            }
        }

        public string StyleIconBackColor
        {
            get
            {
                if (Control == null)
                {
                    return m_strStyleIconBackColor;
                }
                else
                {
                    return Control.StyleIconBackColor;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strStyleIconBackColor = value;
                }
                else
                {
                    Control.StyleIconBackColor = value;
                }
            }
        }

        public string StyleSelectionBorderColor
        {
            get
            {
                if (Control == null)
                {
                    return m_strStyleSelectionBorderColor;
                }
                else
                {
                    return Control.StyleSelectionBorderColor;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strStyleSelectionBorderColor = value;
                }
                else
                {
                    Control.StyleSelectionBorderColor = value;
                }
            }
        }

        public string StyleSelectionColor
        {
            get
            {
                if (Control == null)
                {
                    return m_strStyleSelectionColor;
                }
                else
                {
                    return Control.StyleSelectionColor;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strStyleSelectionColor = value;
                }
                else
                {
                    Control.StyleSelectionColor = value;
                }
            }
        }

        public string StyleSelectionForeColor
        {
            get
            {
                if (Control == null)
                {
                    return m_strStyleSelectionForeColor;
                }
                else
                {
                    return Control.StyleSelectionForeColor;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strStyleSelectionForeColor = value;
                }
                else
                {
                    Control.StyleSelectionForeColor = value;
                }
            }
        }

        public string StyleControlHeight
        {
            get
            {
                if (Control == null)
                {
                    return m_strStyleControlHeight;
                }
                else
                {
                    return Control.StyleControlHeight.ToString();
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strStyleControlHeight = value;
                }
                else
                {
                    Control.StyleControlHeight = Convert.ToDecimal(value);
                }
            }
        }

        public string StyleBorderWidth
        {
            get
            {
                if (Control == null)
                {
                    return m_strStyleBorderWidth;
                }
                else
                {
                    return Control.StyleBorderWidth.ToString();
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strStyleBorderWidth = value;
                }
                else
                {
                    Control.StyleBorderWidth = Convert.ToDecimal(value);
                }
            }
        }

        public string StyleNodeHeight
        {
            get
            {
                if (Control == null)
                {
                    return m_strStyleNodeHeight;
                }
                else
                {
                    return Control.StyleNodeHeight.ToString();
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strStyleNodeHeight = value;
                }
                else
                {
                    Control.StyleNodeHeight = Convert.ToDecimal(value);
                }
            }
        }

        public string StyleIconWidth
        {
            get
            {
                if (Control == null)
                {
                    return m_strStyleIconWidth;
                }
                else
                {
                    return Control.StyleIconWidth.ToString();
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strStyleIconWidth = value;
                }
                else
                {
                    Control.StyleIconWidth = Convert.ToDecimal(value);
                }
            }
        }

        public string StyleFontNames
        {
            get
            {
                if (Control == null)
                {
                    return m_strStyleFontNames;
                }
                else
                {
                    return Control.StyleFontNames;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strStyleFontNames = value;
                }
                else
                {
                    Control.StyleFontNames = value;
                }
            }
        }

        public string StyleFontSize
        {
            get
            {
                if (Control == null)
                {
                    return m_strStyleFontSize;
                }
                else
                {
                    return Control.StyleFontSize.ToString();
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strStyleFontSize = value;
                }
                else
                {
                    Control.StyleFontSize = Convert.ToDecimal(value);
                }
            }
        }

        public string StyleFontBold
        {
            get
            {
                if (Control == null)
                {
                    return m_strStyleFontBold;
                }
                else
                {
                    return Control.StyleFontBold;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strStyleFontBold = value;
                }
                else
                {
                    Control.StyleFontBold = value;
                }
            }
        }

        public string EffectsShadowColor
        {
            get
            {
                if (Control == null)
                {
                    return m_strEffectsShadowColor;
                }
                else
                {
                    return Control.EffectsShadowColor;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strEffectsShadowColor = value;
                }
                else
                {
                    Control.EffectsShadowColor = value;
                }
            }
        }

        public string EffectsStyle
        {
            get
            {
                if (Control == null)
                {
                    return m_strEffectsStyle;
                }
                else
                {
                    return Control.EffectsStyle;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strEffectsStyle = value;
                }
                else
                {
                    Control.EffectsStyle = value;
                }
            }
        }

        public string EffectsShadowStrength
        {
            get
            {
                if (Control == null)
                {
                    return m_strEffectsShadowStrength;
                }
                else
                {
                    return Control.EffectsShadowStrength.ToString();
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strEffectsShadowStrength = value;
                }
                else
                {
                    Control.EffectsShadowStrength = Convert.ToInt32(value);
                }
            }
        }

        public string EffectsTransition
        {
            get
            {
                if (Control == null)
                {
                    return m_strEffectsTransition;
                }
                else
                {
                    return Control.EffectsTransition;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strEffectsTransition = value;
                }
                else
                {
                    Control.EffectsTransition = value;
                }
            }
        }

        public string EffectsDuration
        {
            get
            {
                if (Control == null)
                {
                    return m_strEffectsDuration;
                }
                else
                {
                    return Control.EffectsDuration.ToString();
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strEffectsDuration = value;
                }
                else
                {
                    Control.EffectsDuration = Convert.ToDouble(value);
                }
            }
        }

        public string EffectsShadowDirection
        {
            get
            {
                if (Control == null)
                {
                    return m_strEffectsShadowDirection;
                }
                else
                {
                    return Control.EffectsShadowDirection;
                }
            }
            set
            {
                if (Control == null)
                {
                    m_strEffectsShadowDirection = value;
                }
                else
                {
                    Control.EffectsShadowDirection = value;
                }
            }
        }

		#endregion

		#region "Public Methods"

		public DNNNodeCollection GetNavigationNodes(DNNNode objNode)
        {
            int intRootParent = PortalSettings.ActiveTab.TabID;
            DNNNodeCollection objNodes = null;
            Navigation.ToolTipSource eToolTips;
            int intNavNodeOptions = 0;
            int intDepth = ExpandDepth;
            switch (Level.ToLower())
            {
                case "child":
                    break;
                case "parent":
                    intNavNodeOptions = (int) Navigation.NavNodeOptions.IncludeParent + (int) Navigation.NavNodeOptions.IncludeSelf;
                    break;
                case "same":
                    intNavNodeOptions = (int) Navigation.NavNodeOptions.IncludeSiblings + (int) Navigation.NavNodeOptions.IncludeSelf;
                    break;
                default:
                    intRootParent = -1;
                    intNavNodeOptions = (int) Navigation.NavNodeOptions.IncludeSiblings + (int) Navigation.NavNodeOptions.IncludeSelf;
                    break;
            }

            if (ShowHiddenTabs) intNavNodeOptions += (int) Navigation.NavNodeOptions.IncludeHiddenNodes;

            switch (ToolTip.ToLower())
            {
                case "name":
                    eToolTips = Navigation.ToolTipSource.TabName;
                    break;
                case "title":
                    eToolTips = Navigation.ToolTipSource.Title;
                    break;
                case "description":
                    eToolTips = Navigation.ToolTipSource.Description;
                    break;
                default:
                    eToolTips = Navigation.ToolTipSource.None;
                    break;
            }
            if (PopulateNodesFromClient && Control.SupportsPopulateOnDemand)
            {
                intNavNodeOptions += (int) Navigation.NavNodeOptions.MarkPendingNodes;
            }
            if (PopulateNodesFromClient && Control.SupportsPopulateOnDemand == false)
            {
                ExpandDepth = -1;
            }
            if (StartTabId != -1)
            {
                intRootParent = StartTabId;
            }
            if (objNode != null)
            {
                intRootParent = Convert.ToInt32(objNode.ID);
                intNavNodeOptions = (int) Navigation.NavNodeOptions.MarkPendingNodes;
                objNodes = Navigation.GetNavigationNodes(objNode, eToolTips, intRootParent, intDepth, intNavNodeOptions);
            }
            else
            {
                objNodes = Navigation.GetNavigationNodes(Control.ClientID, eToolTips, intRootParent, intDepth, intNavNodeOptions);
            }
            return objNodes;
        }

		#endregion

		#region "Protected Methods"

		protected string GetValue(string strVal, string strDefault)
        {
            if (String.IsNullOrEmpty(strVal))
            {
                return strDefault;
            }
            else
            {
                return strVal;
            }
        }

        protected void InitializeNavControl(Control objParent, string strDefaultProvider)
        {
            if (String.IsNullOrEmpty(ProviderName))
            {
                ProviderName = strDefaultProvider;
            }
            m_objControl = NavigationProvider.Instance(ProviderName);
            Control.ControlID = "ctl" + ID;
            Control.Initialize();
            AssignControlProperties();
            objParent.Controls.Add(Control.NavigationControl);
        }

		#endregion

		#region "Private Methods"

		private void AssignControlProperties()
        {
            if (!String.IsNullOrEmpty(m_strPathSystemImage))
            {
                Control.PathSystemImage = m_strPathSystemImage;
            }
            if (!String.IsNullOrEmpty(m_strPathImage))
            {
                Control.PathImage = m_strPathImage;
            }
            if (!String.IsNullOrEmpty(m_strPathSystemScript))
            {
                Control.PathSystemScript = m_strPathSystemScript;
            }
            if (!String.IsNullOrEmpty(m_strWorkImage))
            {
                Control.WorkImage = m_strWorkImage;
            }
            if (!String.IsNullOrEmpty(m_strControlOrientation))
            {
                switch (m_strControlOrientation.ToLower())
                {
                    case "horizontal":
                        Control.ControlOrientation = NavigationProvider.Orientation.Horizontal;
                        break;
                    case "vertical":
                        Control.ControlOrientation = NavigationProvider.Orientation.Vertical;
                        break;
                }
            }
            if (!String.IsNullOrEmpty(m_strControlAlignment))
            {
                switch (m_strControlAlignment.ToLower())
                {
                    case "left":
                        Control.ControlAlignment = NavigationProvider.Alignment.Left;
                        break;
                    case "right":
                        Control.ControlAlignment = NavigationProvider.Alignment.Right;
                        break;
                    case "center":
                        Control.ControlAlignment = NavigationProvider.Alignment.Center;
                        break;
                    case "justify":
                        Control.ControlAlignment = NavigationProvider.Alignment.Justify;
                        break;
                }
            }
            Control.ForceCrawlerDisplay = GetValue(m_strForceCrawlerDisplay, "False");
            Control.ForceDownLevel = GetValue(m_strForceDownLevel, "False");
            if (!String.IsNullOrEmpty(m_strMouseOutHideDelay))
            {
                Control.MouseOutHideDelay = Convert.ToDecimal(m_strMouseOutHideDelay);
            }
            if (!String.IsNullOrEmpty(m_strMouseOverDisplay))
            {
                switch (m_strMouseOverDisplay.ToLower())
                {
                    case "highlight":
                        Control.MouseOverDisplay = NavigationProvider.HoverDisplay.Highlight;
                        break;
                    case "outset":
                        Control.MouseOverDisplay = NavigationProvider.HoverDisplay.Outset;
                        break;
                    case "none":
                        Control.MouseOverDisplay = NavigationProvider.HoverDisplay.None;
                        break;
                }
            }
            if (Convert.ToBoolean(GetValue(m_strMouseOverAction, "True")))
            {
                Control.MouseOverAction = NavigationProvider.HoverAction.Expand;
            }
            else
            {
                Control.MouseOverAction = NavigationProvider.HoverAction.None;
            }
            Control.IndicateChildren = Convert.ToBoolean(GetValue(m_strIndicateChildren, "True"));
            if (!String.IsNullOrEmpty(m_strIndicateChildImageRoot))
            {
                Control.IndicateChildImageRoot = m_strIndicateChildImageRoot;
            }
            if (!String.IsNullOrEmpty(m_strIndicateChildImageSub))
            {
                Control.IndicateChildImageSub = m_strIndicateChildImageSub;
            }
            if (!String.IsNullOrEmpty(m_strIndicateChildImageExpandedRoot))
            {
                Control.IndicateChildImageExpandedRoot = m_strIndicateChildImageExpandedRoot;
            }
            if (!String.IsNullOrEmpty(m_strIndicateChildImageExpandedSub))
            {
                Control.IndicateChildImageExpandedSub = m_strIndicateChildImageExpandedSub;
            }
            if (!String.IsNullOrEmpty(m_strNodeLeftHTMLRoot))
            {
                Control.NodeLeftHTMLRoot = m_strNodeLeftHTMLRoot;
            }
            if (!String.IsNullOrEmpty(m_strNodeRightHTMLRoot))
            {
                Control.NodeRightHTMLRoot = m_strNodeRightHTMLRoot;
            }
            if (!String.IsNullOrEmpty(m_strNodeLeftHTMLSub))
            {
                Control.NodeLeftHTMLSub = m_strNodeLeftHTMLSub;
            }
            if (!String.IsNullOrEmpty(m_strNodeRightHTMLSub))
            {
                Control.NodeRightHTMLSub = m_strNodeRightHTMLSub;
            }
            if (!String.IsNullOrEmpty(m_strNodeLeftHTMLBreadCrumbRoot))
            {
                Control.NodeLeftHTMLBreadCrumbRoot = m_strNodeLeftHTMLBreadCrumbRoot;
            }
            if (!String.IsNullOrEmpty(m_strNodeLeftHTMLBreadCrumbSub))
            {
                Control.NodeLeftHTMLBreadCrumbSub = m_strNodeLeftHTMLBreadCrumbSub;
            }
            if (!String.IsNullOrEmpty(m_strNodeRightHTMLBreadCrumbRoot))
            {
                Control.NodeRightHTMLBreadCrumbRoot = m_strNodeRightHTMLBreadCrumbRoot;
            }
            if (!String.IsNullOrEmpty(m_strNodeRightHTMLBreadCrumbSub))
            {
                Control.NodeRightHTMLBreadCrumbSub = m_strNodeRightHTMLBreadCrumbSub;
            }
            if (!String.IsNullOrEmpty(m_strSeparatorHTML))
            {
                Control.SeparatorHTML = m_strSeparatorHTML;
            }
            if (!String.IsNullOrEmpty(m_strSeparatorLeftHTML))
            {
                Control.SeparatorLeftHTML = m_strSeparatorLeftHTML;
            }
            if (!String.IsNullOrEmpty(m_strSeparatorRightHTML))
            {
                Control.SeparatorRightHTML = m_strSeparatorRightHTML;
            }
            if (!String.IsNullOrEmpty(m_strSeparatorLeftHTMLActive))
            {
                Control.SeparatorLeftHTMLActive = m_strSeparatorLeftHTMLActive;
            }
            if (!String.IsNullOrEmpty(m_strSeparatorRightHTMLActive))
            {
                Control.SeparatorRightHTMLActive = m_strSeparatorRightHTMLActive;
            }
            if (!String.IsNullOrEmpty(m_strSeparatorLeftHTMLBreadCrumb))
            {
                Control.SeparatorLeftHTMLBreadCrumb = m_strSeparatorLeftHTMLBreadCrumb;
            }
            if (!String.IsNullOrEmpty(m_strSeparatorRightHTMLBreadCrumb))
            {
                Control.SeparatorRightHTMLBreadCrumb = m_strSeparatorRightHTMLBreadCrumb;
            }
            if (!String.IsNullOrEmpty(m_strCSSControl))
            {
                Control.CSSControl = m_strCSSControl;
            }
            if (!String.IsNullOrEmpty(m_strCSSContainerRoot))
            {
                Control.CSSContainerRoot = m_strCSSContainerRoot;
            }
            if (!String.IsNullOrEmpty(m_strCSSNode))
            {
                Control.CSSNode = m_strCSSNode;
            }
            if (!String.IsNullOrEmpty(m_strCSSIcon))
            {
                Control.CSSIcon = m_strCSSIcon;
            }
            if (!String.IsNullOrEmpty(m_strCSSContainerSub))
            {
                Control.CSSContainerSub = m_strCSSContainerSub;
            }
            if (!String.IsNullOrEmpty(m_strCSSNodeHover))
            {
                Control.CSSNodeHover = m_strCSSNodeHover;
            }
            if (!String.IsNullOrEmpty(m_strCSSBreak))
            {
                Control.CSSBreak = m_strCSSBreak;
            }
            if (!String.IsNullOrEmpty(m_strCSSIndicateChildSub))
            {
                Control.CSSIndicateChildSub = m_strCSSIndicateChildSub;
            }
            if (!String.IsNullOrEmpty(m_strCSSIndicateChildRoot))
            {
                Control.CSSIndicateChildRoot = m_strCSSIndicateChildRoot;
            }
            if (!String.IsNullOrEmpty(m_strCSSBreadCrumbRoot))
            {
                Control.CSSBreadCrumbRoot = m_strCSSBreadCrumbRoot;
            }
            if (!String.IsNullOrEmpty(m_strCSSBreadCrumbSub))
            {
                Control.CSSBreadCrumbSub = m_strCSSBreadCrumbSub;
            }
            if (!String.IsNullOrEmpty(m_strCSSNodeRoot))
            {
                Control.CSSNodeRoot = m_strCSSNodeRoot;
            }
            if (!String.IsNullOrEmpty(m_strCSSNodeSelectedRoot))
            {
                Control.CSSNodeSelectedRoot = m_strCSSNodeSelectedRoot;
            }
            if (!String.IsNullOrEmpty(m_strCSSNodeSelectedSub))
            {
                Control.CSSNodeSelectedSub = m_strCSSNodeSelectedSub;
            }
            if (!String.IsNullOrEmpty(m_strCSSNodeHoverRoot))
            {
                Control.CSSNodeHoverRoot = m_strCSSNodeHoverRoot;
            }
            if (!String.IsNullOrEmpty(m_strCSSNodeHoverSub))
            {
                Control.CSSNodeHoverSub = m_strCSSNodeHoverSub;
            }
            if (!String.IsNullOrEmpty(m_strCSSSeparator))
            {
                Control.CSSSeparator = m_strCSSSeparator;
            }
            if (!String.IsNullOrEmpty(m_strCSSLeftSeparator))
            {
                Control.CSSLeftSeparator = m_strCSSLeftSeparator;
            }
            if (!String.IsNullOrEmpty(m_strCSSRightSeparator))
            {
                Control.CSSRightSeparator = m_strCSSRightSeparator;
            }
            if (!String.IsNullOrEmpty(m_strCSSLeftSeparatorSelection))
            {
                Control.CSSLeftSeparatorSelection = m_strCSSLeftSeparatorSelection;
            }
            if (!String.IsNullOrEmpty(m_strCSSRightSeparatorSelection))
            {
                Control.CSSRightSeparatorSelection = m_strCSSRightSeparatorSelection;
            }
            if (!String.IsNullOrEmpty(m_strCSSLeftSeparatorBreadCrumb))
            {
                Control.CSSLeftSeparatorBreadCrumb = m_strCSSLeftSeparatorBreadCrumb;
            }
            if (!String.IsNullOrEmpty(m_strCSSRightSeparatorBreadCrumb))
            {
                Control.CSSRightSeparatorBreadCrumb = m_strCSSRightSeparatorBreadCrumb;
            }
            if (!String.IsNullOrEmpty(m_strStyleBackColor))
            {
                Control.StyleBackColor = m_strStyleBackColor;
            }
            if (!String.IsNullOrEmpty(m_strStyleForeColor))
            {
                Control.StyleForeColor = m_strStyleForeColor;
            }
            if (!String.IsNullOrEmpty(m_strStyleHighlightColor))
            {
                Control.StyleHighlightColor = m_strStyleHighlightColor;
            }
            if (!String.IsNullOrEmpty(m_strStyleIconBackColor))
            {
                Control.StyleIconBackColor = m_strStyleIconBackColor;
            }
            if (!String.IsNullOrEmpty(m_strStyleSelectionBorderColor))
            {
                Control.StyleSelectionBorderColor = m_strStyleSelectionBorderColor;
            }
            if (!String.IsNullOrEmpty(m_strStyleSelectionColor))
            {
                Control.StyleSelectionColor = m_strStyleSelectionColor;
            }
            if (!String.IsNullOrEmpty(m_strStyleSelectionForeColor))
            {
                Control.StyleSelectionForeColor = m_strStyleSelectionForeColor;
            }
            if (!String.IsNullOrEmpty(m_strStyleControlHeight))
            {
                Control.StyleControlHeight = Convert.ToDecimal(m_strStyleControlHeight);
            }
            if (!String.IsNullOrEmpty(m_strStyleBorderWidth))
            {
                Control.StyleBorderWidth = Convert.ToDecimal(m_strStyleBorderWidth);
            }
            if (!String.IsNullOrEmpty(m_strStyleNodeHeight))
            {
                Control.StyleNodeHeight = Convert.ToDecimal(m_strStyleNodeHeight);
            }
            if (!String.IsNullOrEmpty(m_strStyleIconWidth))
            {
                Control.StyleIconWidth = Convert.ToDecimal(m_strStyleIconWidth);
            }
            if (!String.IsNullOrEmpty(m_strStyleFontNames))
            {
                Control.StyleFontNames = m_strStyleFontNames;
            }
            if (!String.IsNullOrEmpty(m_strStyleFontSize))
            {
                Control.StyleFontSize = Convert.ToDecimal(m_strStyleFontSize);
            }
            if (!String.IsNullOrEmpty(m_strStyleFontBold))
            {
                Control.StyleFontBold = m_strStyleFontBold;
            }
            if (!String.IsNullOrEmpty(m_strEffectsShadowColor))
            {
                Control.EffectsShadowColor = m_strEffectsShadowColor;
            }
            if (!String.IsNullOrEmpty(m_strEffectsStyle))
            {
                Control.EffectsStyle = m_strEffectsStyle;
            }
            if (!String.IsNullOrEmpty(m_strEffectsShadowStrength))
            {
                Control.EffectsShadowStrength = Convert.ToInt32(m_strEffectsShadowStrength);
            }
            if (!String.IsNullOrEmpty(m_strEffectsTransition))
            {
                Control.EffectsTransition = m_strEffectsTransition;
            }
            if (!String.IsNullOrEmpty(m_strEffectsDuration))
            {
                Control.EffectsDuration = Convert.ToDouble(m_strEffectsDuration);
            }
            if (!String.IsNullOrEmpty(m_strEffectsShadowDirection))
            {
                Control.EffectsShadowDirection = m_strEffectsShadowDirection;
            }
            Control.CustomAttributes = CustomAttributes;
        }

        protected void Bind(DNNNodeCollection objNodes)
        {
            Control.Bind(objNodes);
        }

        private string GetPath(string strPath)
        {
            if (strPath.IndexOf("[SKINPATH]") > -1)
            {
                return strPath.Replace("[SKINPATH]", PortalSettings.ActiveTab.SkinPath);
            }
            else if (strPath.IndexOf("[APPIMAGEPATH]") > -1)
            {
                return strPath.Replace("[APPIMAGEPATH]", Globals.ApplicationPath + "/images/");
            }
            else if (strPath.IndexOf("[HOMEDIRECTORY]") > -1)
            {
                return strPath.Replace("[HOMEDIRECTORY]", PortalSettings.HomeDirectory);
            }
            else
            {
                if (strPath.StartsWith("~"))
                {
                    return ResolveUrl(strPath);
                }
            }
            return strPath;
		}

		#endregion
	}

    public class CustomAttribute
    {
        public string Name;
        public string Value;
    }
}