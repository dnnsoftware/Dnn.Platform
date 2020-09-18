// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Web.UI;

    using DotNetNuke.Common;
    using DotNetNuke.Modules.NavigationProvider;
    using DotNetNuke.UI.WebControls;

    public class NavObjectBase : SkinObjectBase
    {
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
        private string m_strLevel = string.Empty;
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
        private string m_strProviderName = string.Empty;
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
        private string m_strToolTip = string.Empty;
        private string m_strWorkImage;

        // JH - 2/5/07 - support for custom attributes
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public List<CustomAttribute> CustomAttributes
        {
            get
            {
                return this.m_objCustomAttributes;
            }
        }

        public bool ShowHiddenTabs { get; set; }

        public string ProviderName
        {
            get
            {
                return this.m_strProviderName;
            }

            set
            {
                this.m_strProviderName = value;
            }
        }

        public string Level
        {
            get
            {
                return this.m_strLevel;
            }

            set
            {
                this.m_strLevel = value;
            }
        }

        public string ToolTip
        {
            get
            {
                return this.m_strToolTip;
            }

            set
            {
                this.m_strToolTip = value;
            }
        }

        public bool PopulateNodesFromClient
        {
            get
            {
                return this.m_blnPopulateNodesFromClient;
            }

            set
            {
                this.m_blnPopulateNodesFromClient = value;
            }
        }

        public int ExpandDepth
        {
            get
            {
                return this.m_intExpandDepth;
            }

            set
            {
                this.m_intExpandDepth = value;
            }
        }

        public int StartTabId
        {
            get
            {
                return this.m_intStartTabId;
            }

            set
            {
                this.m_intStartTabId = value;
            }
        }

        public string PathSystemImage
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strPathSystemImage;
                }
                else
                {
                    return this.Control.PathSystemImage;
                }
            }

            set
            {
                value = this.GetPath(value);
                if (this.Control == null)
                {
                    this.m_strPathSystemImage = value;
                }
                else
                {
                    this.Control.PathSystemImage = value;
                }
            }
        }

        public string PathImage
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strPathImage;
                }
                else
                {
                    return this.Control.PathImage;
                }
            }

            set
            {
                value = this.GetPath(value);
                if (this.Control == null)
                {
                    this.m_strPathImage = value;
                }
                else
                {
                    this.Control.PathImage = value;
                }
            }
        }

        public string WorkImage
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strWorkImage;
                }
                else
                {
                    return this.Control.WorkImage;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strWorkImage = value;
                }
                else
                {
                    this.Control.WorkImage = value;
                }
            }
        }

        public string PathSystemScript
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strPathSystemScript;
                }
                else
                {
                    return this.Control.PathSystemScript;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strPathSystemScript = value;
                }
                else
                {
                    this.Control.PathSystemScript = value;
                }
            }
        }

        public string ControlOrientation
        {
            get
            {
                string retValue = string.Empty;
                if (this.Control == null)
                {
                    retValue = this.m_strControlOrientation;
                }
                else
                {
                    switch (this.Control.ControlOrientation)
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
                if (this.Control == null)
                {
                    this.m_strControlOrientation = value;
                }
                else
                {
                    switch (value.ToLowerInvariant())
                    {
                        case "horizontal":
                            this.Control.ControlOrientation = NavigationProvider.Orientation.Horizontal;
                            break;
                        case "vertical":
                            this.Control.ControlOrientation = NavigationProvider.Orientation.Vertical;
                            break;
                    }
                }
            }
        }

        public string ControlAlignment
        {
            get
            {
                string retValue = string.Empty;
                if (this.Control == null)
                {
                    retValue = this.m_strControlAlignment;
                }
                else
                {
                    switch (this.Control.ControlAlignment)
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
                if (this.Control == null)
                {
                    this.m_strControlAlignment = value;
                }
                else
                {
                    switch (value.ToLowerInvariant())
                    {
                        case "left":
                            this.Control.ControlAlignment = NavigationProvider.Alignment.Left;
                            break;
                        case "right":
                            this.Control.ControlAlignment = NavigationProvider.Alignment.Right;
                            break;
                        case "center":
                            this.Control.ControlAlignment = NavigationProvider.Alignment.Center;
                            break;
                        case "justify":
                            this.Control.ControlAlignment = NavigationProvider.Alignment.Justify;
                            break;
                    }
                }
            }
        }

        public string ForceCrawlerDisplay
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strForceCrawlerDisplay;
                }
                else
                {
                    return this.Control.ForceCrawlerDisplay;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strForceCrawlerDisplay = value;
                }
                else
                {
                    this.Control.ForceCrawlerDisplay = value;
                }
            }
        }

        public string ForceDownLevel
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strForceDownLevel;
                }
                else
                {
                    return this.Control.ForceDownLevel;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strForceDownLevel = value;
                }
                else
                {
                    this.Control.ForceDownLevel = value;
                }
            }
        }

        public string MouseOutHideDelay
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strMouseOutHideDelay;
                }
                else
                {
                    return this.Control.MouseOutHideDelay.ToString();
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strMouseOutHideDelay = value;
                }
                else
                {
                    this.Control.MouseOutHideDelay = Convert.ToDecimal(value);
                }
            }
        }

        public string MouseOverDisplay
        {
            get
            {
                string retValue = string.Empty;
                if (this.Control == null)
                {
                    retValue = this.m_strMouseOverDisplay;
                }
                else
                {
                    switch (this.Control.MouseOverDisplay)
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
                if (this.Control == null)
                {
                    this.m_strMouseOverDisplay = value;
                }
                else
                {
                    switch (value.ToLowerInvariant())
                    {
                        case "highlight":
                            this.Control.MouseOverDisplay = NavigationProvider.HoverDisplay.Highlight;
                            break;
                        case "outset":
                            this.Control.MouseOverDisplay = NavigationProvider.HoverDisplay.Outset;
                            break;
                        case "none":
                            this.Control.MouseOverDisplay = NavigationProvider.HoverDisplay.None;
                            break;
                    }
                }
            }
        }

        public string MouseOverAction
        {
            get
            {
                string retValue = string.Empty;
                if (this.Control == null)
                {
                    retValue = this.m_strMouseOverAction;
                }
                else
                {
                    switch (this.Control.MouseOverAction)
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
                if (this.Control == null)
                {
                    this.m_strMouseOverAction = value;
                }
                else
                {
                    if (Convert.ToBoolean(this.GetValue(value, "True")))
                    {
                        this.Control.MouseOverAction = NavigationProvider.HoverAction.Expand;
                    }
                    else
                    {
                        this.Control.MouseOverAction = NavigationProvider.HoverAction.None;
                    }
                }
            }
        }

        public string IndicateChildren
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strIndicateChildren;
                }
                else
                {
                    return this.Control.IndicateChildren.ToString();
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strIndicateChildren = value;
                }
                else
                {
                    this.Control.IndicateChildren = Convert.ToBoolean(value);
                }
            }
        }

        public string IndicateChildImageRoot
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strIndicateChildImageRoot;
                }
                else
                {
                    return this.Control.IndicateChildImageRoot;
                }
            }

            set
            {
                value = this.GetPath(value);
                if (this.Control == null)
                {
                    this.m_strIndicateChildImageRoot = value;
                }
                else
                {
                    this.Control.IndicateChildImageRoot = value;
                }
            }
        }

        public string IndicateChildImageSub
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strIndicateChildImageSub;
                }
                else
                {
                    return this.Control.IndicateChildImageSub;
                }
            }

            set
            {
                value = this.GetPath(value);
                if (this.Control == null)
                {
                    this.m_strIndicateChildImageSub = value;
                }
                else
                {
                    this.Control.IndicateChildImageSub = value;
                }
            }
        }

        public string IndicateChildImageExpandedRoot
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strIndicateChildImageExpandedRoot;
                }
                else
                {
                    return this.Control.IndicateChildImageExpandedRoot;
                }
            }

            set
            {
                value = this.GetPath(value);
                if (this.Control == null)
                {
                    this.m_strIndicateChildImageExpandedRoot = value;
                }
                else
                {
                    this.Control.IndicateChildImageExpandedRoot = value;
                }
            }
        }

        public string IndicateChildImageExpandedSub
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strIndicateChildImageExpandedSub;
                }
                else
                {
                    return this.Control.IndicateChildImageExpandedSub;
                }
            }

            set
            {
                value = this.GetPath(value);
                if (this.Control == null)
                {
                    this.m_strIndicateChildImageExpandedSub = value;
                }
                else
                {
                    this.Control.IndicateChildImageExpandedSub = value;
                }
            }
        }

        public string NodeLeftHTMLRoot
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strNodeLeftHTMLRoot;
                }
                else
                {
                    return this.Control.NodeLeftHTMLRoot;
                }
            }

            set
            {
                value = this.GetPath(value);
                if (this.Control == null)
                {
                    this.m_strNodeLeftHTMLRoot = value;
                }
                else
                {
                    this.Control.NodeLeftHTMLRoot = value;
                }
            }
        }

        public string NodeRightHTMLRoot
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strNodeRightHTMLRoot;
                }
                else
                {
                    return this.Control.NodeRightHTMLRoot;
                }
            }

            set
            {
                value = this.GetPath(value);
                if (this.Control == null)
                {
                    this.m_strNodeRightHTMLRoot = value;
                }
                else
                {
                    this.Control.NodeRightHTMLRoot = value;
                }
            }
        }

        public string NodeLeftHTMLSub
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strNodeLeftHTMLSub;
                }
                else
                {
                    return this.Control.NodeLeftHTMLSub;
                }
            }

            set
            {
                value = this.GetPath(value);
                if (this.Control == null)
                {
                    this.m_strNodeLeftHTMLSub = value;
                }
                else
                {
                    this.Control.NodeLeftHTMLSub = value;
                }
            }
        }

        public string NodeRightHTMLSub
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strNodeRightHTMLSub;
                }
                else
                {
                    return this.Control.NodeRightHTMLSub;
                }
            }

            set
            {
                value = this.GetPath(value);
                if (this.Control == null)
                {
                    this.m_strNodeRightHTMLSub = value;
                }
                else
                {
                    this.Control.NodeRightHTMLSub = value;
                }
            }
        }

        public string NodeLeftHTMLBreadCrumbRoot
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strNodeLeftHTMLBreadCrumbRoot;
                }
                else
                {
                    return this.Control.NodeLeftHTMLBreadCrumbRoot;
                }
            }

            set
            {
                value = this.GetPath(value);
                if (this.Control == null)
                {
                    this.m_strNodeLeftHTMLBreadCrumbRoot = value;
                }
                else
                {
                    this.Control.NodeLeftHTMLBreadCrumbRoot = value;
                }
            }
        }

        public string NodeLeftHTMLBreadCrumbSub
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strNodeLeftHTMLBreadCrumbSub;
                }
                else
                {
                    return this.Control.NodeLeftHTMLBreadCrumbSub;
                }
            }

            set
            {
                value = this.GetPath(value);
                if (this.Control == null)
                {
                    this.m_strNodeLeftHTMLBreadCrumbSub = value;
                }
                else
                {
                    this.Control.NodeLeftHTMLBreadCrumbSub = value;
                }
            }
        }

        public string NodeRightHTMLBreadCrumbRoot
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strNodeRightHTMLBreadCrumbRoot;
                }
                else
                {
                    return this.Control.NodeRightHTMLBreadCrumbRoot;
                }
            }

            set
            {
                value = this.GetPath(value);
                if (this.Control == null)
                {
                    this.m_strNodeRightHTMLBreadCrumbRoot = value;
                }
                else
                {
                    this.Control.NodeRightHTMLBreadCrumbRoot = value;
                }
            }
        }

        public string NodeRightHTMLBreadCrumbSub
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strNodeRightHTMLBreadCrumbSub;
                }
                else
                {
                    return this.Control.NodeRightHTMLBreadCrumbSub;
                }
            }

            set
            {
                value = this.GetPath(value);
                if (this.Control == null)
                {
                    this.m_strNodeRightHTMLBreadCrumbSub = value;
                }
                else
                {
                    this.Control.NodeRightHTMLBreadCrumbSub = value;
                }
            }
        }

        public string SeparatorHTML
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strSeparatorHTML;
                }
                else
                {
                    return this.Control.SeparatorHTML;
                }
            }

            set
            {
                value = this.GetPath(value);
                if (this.Control == null)
                {
                    this.m_strSeparatorHTML = value;
                }
                else
                {
                    this.Control.SeparatorHTML = value;
                }
            }
        }

        public string SeparatorLeftHTML
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strSeparatorLeftHTML;
                }
                else
                {
                    return this.Control.SeparatorLeftHTML;
                }
            }

            set
            {
                value = this.GetPath(value);
                if (this.Control == null)
                {
                    this.m_strSeparatorLeftHTML = value;
                }
                else
                {
                    this.Control.SeparatorLeftHTML = value;
                }
            }
        }

        public string SeparatorRightHTML
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strSeparatorRightHTML;
                }
                else
                {
                    return this.Control.SeparatorRightHTML;
                }
            }

            set
            {
                value = this.GetPath(value);
                if (this.Control == null)
                {
                    this.m_strSeparatorRightHTML = value;
                }
                else
                {
                    this.Control.SeparatorRightHTML = value;
                }
            }
        }

        public string SeparatorLeftHTMLActive
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strSeparatorLeftHTMLActive;
                }
                else
                {
                    return this.Control.SeparatorLeftHTMLActive;
                }
            }

            set
            {
                value = this.GetPath(value);
                if (this.Control == null)
                {
                    this.m_strSeparatorLeftHTMLActive = value;
                }
                else
                {
                    this.Control.SeparatorLeftHTMLActive = value;
                }
            }
        }

        public string SeparatorRightHTMLActive
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strSeparatorRightHTMLActive;
                }
                else
                {
                    return this.Control.SeparatorRightHTMLActive;
                }
            }

            set
            {
                value = this.GetPath(value);
                if (this.Control == null)
                {
                    this.m_strSeparatorRightHTMLActive = value;
                }
                else
                {
                    this.Control.SeparatorRightHTMLActive = value;
                }
            }
        }

        public string SeparatorLeftHTMLBreadCrumb
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strSeparatorLeftHTMLBreadCrumb;
                }
                else
                {
                    return this.Control.SeparatorLeftHTMLBreadCrumb;
                }
            }

            set
            {
                value = this.GetPath(value);
                if (this.Control == null)
                {
                    this.m_strSeparatorLeftHTMLBreadCrumb = value;
                }
                else
                {
                    this.Control.SeparatorLeftHTMLBreadCrumb = value;
                }
            }
        }

        public string SeparatorRightHTMLBreadCrumb
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strSeparatorRightHTMLBreadCrumb;
                }
                else
                {
                    return this.Control.SeparatorRightHTMLBreadCrumb;
                }
            }

            set
            {
                value = this.GetPath(value);
                if (this.Control == null)
                {
                    this.m_strSeparatorRightHTMLBreadCrumb = value;
                }
                else
                {
                    this.Control.SeparatorRightHTMLBreadCrumb = value;
                }
            }
        }

        public string CSSControl
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strCSSControl;
                }
                else
                {
                    return this.Control.CSSControl;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strCSSControl = value;
                }
                else
                {
                    this.Control.CSSControl = value;
                }
            }
        }

        public string CSSContainerRoot
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strCSSContainerRoot;
                }
                else
                {
                    return this.Control.CSSContainerRoot;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strCSSContainerRoot = value;
                }
                else
                {
                    this.Control.CSSContainerRoot = value;
                }
            }
        }

        public string CSSNode
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strCSSNode;
                }
                else
                {
                    return this.Control.CSSNode;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strCSSNode = value;
                }
                else
                {
                    this.Control.CSSNode = value;
                }
            }
        }

        public string CSSIcon
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strCSSIcon;
                }
                else
                {
                    return this.Control.CSSIcon;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strCSSIcon = value;
                }
                else
                {
                    this.Control.CSSIcon = value;
                }
            }
        }

        public string CSSContainerSub
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strCSSContainerSub;
                }
                else
                {
                    return this.Control.CSSContainerSub;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strCSSContainerSub = value;
                }
                else
                {
                    this.Control.CSSContainerSub = value;
                }
            }
        }

        public string CSSNodeHover
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strCSSNodeHover;
                }
                else
                {
                    return this.Control.CSSNodeHover;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strCSSNodeHover = value;
                }
                else
                {
                    this.Control.CSSNodeHover = value;
                }
            }
        }

        public string CSSBreak
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strCSSBreak;
                }
                else
                {
                    return this.Control.CSSBreak;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strCSSBreak = value;
                }
                else
                {
                    this.Control.CSSBreak = value;
                }
            }
        }

        public string CSSIndicateChildSub
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strCSSIndicateChildSub;
                }
                else
                {
                    return this.Control.CSSIndicateChildSub;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strCSSIndicateChildSub = value;
                }
                else
                {
                    this.Control.CSSIndicateChildSub = value;
                }
            }
        }

        public string CSSIndicateChildRoot
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strCSSIndicateChildRoot;
                }
                else
                {
                    return this.Control.CSSIndicateChildRoot;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strCSSIndicateChildRoot = value;
                }
                else
                {
                    this.Control.CSSIndicateChildRoot = value;
                }
            }
        }

        public string CSSBreadCrumbRoot
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strCSSBreadCrumbRoot;
                }
                else
                {
                    return this.Control.CSSBreadCrumbRoot;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strCSSBreadCrumbRoot = value;
                }
                else
                {
                    this.Control.CSSBreadCrumbRoot = value;
                }
            }
        }

        public string CSSBreadCrumbSub
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strCSSBreadCrumbSub;
                }
                else
                {
                    return this.Control.CSSBreadCrumbSub;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strCSSBreadCrumbSub = value;
                }
                else
                {
                    this.Control.CSSBreadCrumbSub = value;
                }
            }
        }

        public string CSSNodeRoot
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strCSSNodeRoot;
                }
                else
                {
                    return this.Control.CSSNodeRoot;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strCSSNodeRoot = value;
                }
                else
                {
                    this.Control.CSSNodeRoot = value;
                }
            }
        }

        public string CSSNodeSelectedRoot
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strCSSNodeSelectedRoot;
                }
                else
                {
                    return this.Control.CSSNodeSelectedRoot;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strCSSNodeSelectedRoot = value;
                }
                else
                {
                    this.Control.CSSNodeSelectedRoot = value;
                }
            }
        }

        public string CSSNodeSelectedSub
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strCSSNodeSelectedSub;
                }
                else
                {
                    return this.Control.CSSNodeSelectedSub;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strCSSNodeSelectedSub = value;
                }
                else
                {
                    this.Control.CSSNodeSelectedSub = value;
                }
            }
        }

        public string CSSNodeHoverRoot
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strCSSNodeHoverRoot;
                }
                else
                {
                    return this.Control.CSSNodeHoverRoot;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strCSSNodeHoverRoot = value;
                }
                else
                {
                    this.Control.CSSNodeHoverRoot = value;
                }
            }
        }

        public string CSSNodeHoverSub
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strCSSNodeHoverSub;
                }
                else
                {
                    return this.Control.CSSNodeHoverSub;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strCSSNodeHoverSub = value;
                }
                else
                {
                    this.Control.CSSNodeHoverSub = value;
                }
            }
        }

        public string CSSSeparator
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strCSSSeparator;
                }
                else
                {
                    return this.Control.CSSSeparator;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strCSSSeparator = value;
                }
                else
                {
                    this.Control.CSSSeparator = value;
                }
            }
        }

        public string CSSLeftSeparator
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strCSSLeftSeparator;
                }
                else
                {
                    return this.Control.CSSLeftSeparator;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strCSSLeftSeparator = value;
                }
                else
                {
                    this.Control.CSSLeftSeparator = value;
                }
            }
        }

        public string CSSRightSeparator
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strCSSRightSeparator;
                }
                else
                {
                    return this.Control.CSSRightSeparator;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strCSSRightSeparator = value;
                }
                else
                {
                    this.Control.CSSRightSeparator = value;
                }
            }
        }

        public string CSSLeftSeparatorSelection
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strCSSLeftSeparatorSelection;
                }
                else
                {
                    return this.Control.CSSLeftSeparatorSelection;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strCSSLeftSeparatorSelection = value;
                }
                else
                {
                    this.Control.CSSLeftSeparatorSelection = value;
                }
            }
        }

        public string CSSRightSeparatorSelection
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strCSSRightSeparatorSelection;
                }
                else
                {
                    return this.Control.CSSRightSeparatorSelection;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strCSSRightSeparatorSelection = value;
                }
                else
                {
                    this.Control.CSSRightSeparatorSelection = value;
                }
            }
        }

        public string CSSLeftSeparatorBreadCrumb
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strCSSLeftSeparatorBreadCrumb;
                }
                else
                {
                    return this.Control.CSSLeftSeparatorBreadCrumb;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strCSSLeftSeparatorBreadCrumb = value;
                }
                else
                {
                    this.Control.CSSLeftSeparatorBreadCrumb = value;
                }
            }
        }

        public string CSSRightSeparatorBreadCrumb
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strCSSRightSeparatorBreadCrumb;
                }
                else
                {
                    return this.Control.CSSRightSeparatorBreadCrumb;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strCSSRightSeparatorBreadCrumb = value;
                }
                else
                {
                    this.Control.CSSRightSeparatorBreadCrumb = value;
                }
            }
        }

        public string StyleBackColor
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strStyleBackColor;
                }
                else
                {
                    return this.Control.StyleBackColor;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strStyleBackColor = value;
                }
                else
                {
                    this.Control.StyleBackColor = value;
                }
            }
        }

        public string StyleForeColor
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strStyleForeColor;
                }
                else
                {
                    return this.Control.StyleForeColor;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strStyleForeColor = value;
                }
                else
                {
                    this.Control.StyleForeColor = value;
                }
            }
        }

        public string StyleHighlightColor
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strStyleHighlightColor;
                }
                else
                {
                    return this.Control.StyleHighlightColor;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strStyleHighlightColor = value;
                }
                else
                {
                    this.Control.StyleHighlightColor = value;
                }
            }
        }

        public string StyleIconBackColor
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strStyleIconBackColor;
                }
                else
                {
                    return this.Control.StyleIconBackColor;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strStyleIconBackColor = value;
                }
                else
                {
                    this.Control.StyleIconBackColor = value;
                }
            }
        }

        public string StyleSelectionBorderColor
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strStyleSelectionBorderColor;
                }
                else
                {
                    return this.Control.StyleSelectionBorderColor;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strStyleSelectionBorderColor = value;
                }
                else
                {
                    this.Control.StyleSelectionBorderColor = value;
                }
            }
        }

        public string StyleSelectionColor
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strStyleSelectionColor;
                }
                else
                {
                    return this.Control.StyleSelectionColor;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strStyleSelectionColor = value;
                }
                else
                {
                    this.Control.StyleSelectionColor = value;
                }
            }
        }

        public string StyleSelectionForeColor
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strStyleSelectionForeColor;
                }
                else
                {
                    return this.Control.StyleSelectionForeColor;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strStyleSelectionForeColor = value;
                }
                else
                {
                    this.Control.StyleSelectionForeColor = value;
                }
            }
        }

        public string StyleControlHeight
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strStyleControlHeight;
                }
                else
                {
                    return this.Control.StyleControlHeight.ToString();
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strStyleControlHeight = value;
                }
                else
                {
                    this.Control.StyleControlHeight = Convert.ToDecimal(value);
                }
            }
        }

        public string StyleBorderWidth
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strStyleBorderWidth;
                }
                else
                {
                    return this.Control.StyleBorderWidth.ToString();
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strStyleBorderWidth = value;
                }
                else
                {
                    this.Control.StyleBorderWidth = Convert.ToDecimal(value);
                }
            }
        }

        public string StyleNodeHeight
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strStyleNodeHeight;
                }
                else
                {
                    return this.Control.StyleNodeHeight.ToString();
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strStyleNodeHeight = value;
                }
                else
                {
                    this.Control.StyleNodeHeight = Convert.ToDecimal(value);
                }
            }
        }

        public string StyleIconWidth
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strStyleIconWidth;
                }
                else
                {
                    return this.Control.StyleIconWidth.ToString();
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strStyleIconWidth = value;
                }
                else
                {
                    this.Control.StyleIconWidth = Convert.ToDecimal(value);
                }
            }
        }

        public string StyleFontNames
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strStyleFontNames;
                }
                else
                {
                    return this.Control.StyleFontNames;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strStyleFontNames = value;
                }
                else
                {
                    this.Control.StyleFontNames = value;
                }
            }
        }

        public string StyleFontSize
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strStyleFontSize;
                }
                else
                {
                    return this.Control.StyleFontSize.ToString();
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strStyleFontSize = value;
                }
                else
                {
                    this.Control.StyleFontSize = Convert.ToDecimal(value);
                }
            }
        }

        public string StyleFontBold
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strStyleFontBold;
                }
                else
                {
                    return this.Control.StyleFontBold;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strStyleFontBold = value;
                }
                else
                {
                    this.Control.StyleFontBold = value;
                }
            }
        }

        public string EffectsShadowColor
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strEffectsShadowColor;
                }
                else
                {
                    return this.Control.EffectsShadowColor;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strEffectsShadowColor = value;
                }
                else
                {
                    this.Control.EffectsShadowColor = value;
                }
            }
        }

        public string EffectsStyle
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strEffectsStyle;
                }
                else
                {
                    return this.Control.EffectsStyle;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strEffectsStyle = value;
                }
                else
                {
                    this.Control.EffectsStyle = value;
                }
            }
        }

        public string EffectsShadowStrength
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strEffectsShadowStrength;
                }
                else
                {
                    return this.Control.EffectsShadowStrength.ToString();
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strEffectsShadowStrength = value;
                }
                else
                {
                    this.Control.EffectsShadowStrength = Convert.ToInt32(value);
                }
            }
        }

        public string EffectsTransition
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strEffectsTransition;
                }
                else
                {
                    return this.Control.EffectsTransition;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strEffectsTransition = value;
                }
                else
                {
                    this.Control.EffectsTransition = value;
                }
            }
        }

        public string EffectsDuration
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strEffectsDuration;
                }
                else
                {
                    return this.Control.EffectsDuration.ToString();
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strEffectsDuration = value;
                }
                else
                {
                    this.Control.EffectsDuration = Convert.ToDouble(value);
                }
            }
        }

        public string EffectsShadowDirection
        {
            get
            {
                if (this.Control == null)
                {
                    return this.m_strEffectsShadowDirection;
                }
                else
                {
                    return this.Control.EffectsShadowDirection;
                }
            }

            set
            {
                if (this.Control == null)
                {
                    this.m_strEffectsShadowDirection = value;
                }
                else
                {
                    this.Control.EffectsShadowDirection = value;
                }
            }
        }

        protected NavigationProvider Control
        {
            get
            {
                return this.m_objControl;
            }
        }

        public DNNNodeCollection GetNavigationNodes(DNNNode objNode)
        {
            int intRootParent = this.PortalSettings.ActiveTab.TabID;
            DNNNodeCollection objNodes = null;
            Navigation.ToolTipSource eToolTips;
            int intNavNodeOptions = 0;
            int intDepth = this.ExpandDepth;
            switch (this.Level.ToLowerInvariant())
            {
                case "child":
                    break;
                case "parent":
                    intNavNodeOptions = (int)Navigation.NavNodeOptions.IncludeParent + (int)Navigation.NavNodeOptions.IncludeSelf;
                    break;
                case "same":
                    intNavNodeOptions = (int)Navigation.NavNodeOptions.IncludeSiblings + (int)Navigation.NavNodeOptions.IncludeSelf;
                    break;
                default:
                    intRootParent = -1;
                    intNavNodeOptions = (int)Navigation.NavNodeOptions.IncludeSiblings + (int)Navigation.NavNodeOptions.IncludeSelf;
                    break;
            }

            if (this.ShowHiddenTabs)
            {
                intNavNodeOptions += (int)Navigation.NavNodeOptions.IncludeHiddenNodes;
            }

            switch (this.ToolTip.ToLowerInvariant())
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

            if (this.PopulateNodesFromClient && this.Control.SupportsPopulateOnDemand)
            {
                intNavNodeOptions += (int)Navigation.NavNodeOptions.MarkPendingNodes;
            }

            if (this.PopulateNodesFromClient && this.Control.SupportsPopulateOnDemand == false)
            {
                this.ExpandDepth = -1;
            }

            if (this.StartTabId != -1)
            {
                intRootParent = this.StartTabId;
            }

            if (objNode != null)
            {
                intRootParent = Convert.ToInt32(objNode.ID);
                intNavNodeOptions = (int)Navigation.NavNodeOptions.MarkPendingNodes;
                objNodes = Navigation.GetNavigationNodes(objNode, eToolTips, intRootParent, intDepth, intNavNodeOptions);
            }
            else
            {
                objNodes = Navigation.GetNavigationNodes(this.Control.ClientID, eToolTips, intRootParent, intDepth, intNavNodeOptions);
            }

            return objNodes;
        }

        protected string GetValue(string strVal, string strDefault)
        {
            if (string.IsNullOrEmpty(strVal))
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
            if (string.IsNullOrEmpty(this.ProviderName))
            {
                this.ProviderName = strDefaultProvider;
            }

            this.m_objControl = NavigationProvider.Instance(this.ProviderName);
            this.Control.ControlID = "ctl" + this.ID;
            this.Control.Initialize();
            this.AssignControlProperties();
            objParent.Controls.Add(this.Control.NavigationControl);
        }

        protected void Bind(DNNNodeCollection objNodes)
        {
            this.Control.Bind(objNodes);
        }

        private void AssignControlProperties()
        {
            if (!string.IsNullOrEmpty(this.m_strPathSystemImage))
            {
                this.Control.PathSystemImage = this.m_strPathSystemImage;
            }

            if (!string.IsNullOrEmpty(this.m_strPathImage))
            {
                this.Control.PathImage = this.m_strPathImage;
            }

            if (!string.IsNullOrEmpty(this.m_strPathSystemScript))
            {
                this.Control.PathSystemScript = this.m_strPathSystemScript;
            }

            if (!string.IsNullOrEmpty(this.m_strWorkImage))
            {
                this.Control.WorkImage = this.m_strWorkImage;
            }

            if (!string.IsNullOrEmpty(this.m_strControlOrientation))
            {
                switch (this.m_strControlOrientation.ToLowerInvariant())
                {
                    case "horizontal":
                        this.Control.ControlOrientation = NavigationProvider.Orientation.Horizontal;
                        break;
                    case "vertical":
                        this.Control.ControlOrientation = NavigationProvider.Orientation.Vertical;
                        break;
                }
            }

            if (!string.IsNullOrEmpty(this.m_strControlAlignment))
            {
                switch (this.m_strControlAlignment.ToLowerInvariant())
                {
                    case "left":
                        this.Control.ControlAlignment = NavigationProvider.Alignment.Left;
                        break;
                    case "right":
                        this.Control.ControlAlignment = NavigationProvider.Alignment.Right;
                        break;
                    case "center":
                        this.Control.ControlAlignment = NavigationProvider.Alignment.Center;
                        break;
                    case "justify":
                        this.Control.ControlAlignment = NavigationProvider.Alignment.Justify;
                        break;
                }
            }

            this.Control.ForceCrawlerDisplay = this.GetValue(this.m_strForceCrawlerDisplay, "False");
            this.Control.ForceDownLevel = this.GetValue(this.m_strForceDownLevel, "False");
            if (!string.IsNullOrEmpty(this.m_strMouseOutHideDelay))
            {
                this.Control.MouseOutHideDelay = Convert.ToDecimal(this.m_strMouseOutHideDelay);
            }

            if (!string.IsNullOrEmpty(this.m_strMouseOverDisplay))
            {
                switch (this.m_strMouseOverDisplay.ToLowerInvariant())
                {
                    case "highlight":
                        this.Control.MouseOverDisplay = NavigationProvider.HoverDisplay.Highlight;
                        break;
                    case "outset":
                        this.Control.MouseOverDisplay = NavigationProvider.HoverDisplay.Outset;
                        break;
                    case "none":
                        this.Control.MouseOverDisplay = NavigationProvider.HoverDisplay.None;
                        break;
                }
            }

            if (Convert.ToBoolean(this.GetValue(this.m_strMouseOverAction, "True")))
            {
                this.Control.MouseOverAction = NavigationProvider.HoverAction.Expand;
            }
            else
            {
                this.Control.MouseOverAction = NavigationProvider.HoverAction.None;
            }

            this.Control.IndicateChildren = Convert.ToBoolean(this.GetValue(this.m_strIndicateChildren, "True"));
            if (!string.IsNullOrEmpty(this.m_strIndicateChildImageRoot))
            {
                this.Control.IndicateChildImageRoot = this.m_strIndicateChildImageRoot;
            }

            if (!string.IsNullOrEmpty(this.m_strIndicateChildImageSub))
            {
                this.Control.IndicateChildImageSub = this.m_strIndicateChildImageSub;
            }

            if (!string.IsNullOrEmpty(this.m_strIndicateChildImageExpandedRoot))
            {
                this.Control.IndicateChildImageExpandedRoot = this.m_strIndicateChildImageExpandedRoot;
            }

            if (!string.IsNullOrEmpty(this.m_strIndicateChildImageExpandedSub))
            {
                this.Control.IndicateChildImageExpandedSub = this.m_strIndicateChildImageExpandedSub;
            }

            if (!string.IsNullOrEmpty(this.m_strNodeLeftHTMLRoot))
            {
                this.Control.NodeLeftHTMLRoot = this.m_strNodeLeftHTMLRoot;
            }

            if (!string.IsNullOrEmpty(this.m_strNodeRightHTMLRoot))
            {
                this.Control.NodeRightHTMLRoot = this.m_strNodeRightHTMLRoot;
            }

            if (!string.IsNullOrEmpty(this.m_strNodeLeftHTMLSub))
            {
                this.Control.NodeLeftHTMLSub = this.m_strNodeLeftHTMLSub;
            }

            if (!string.IsNullOrEmpty(this.m_strNodeRightHTMLSub))
            {
                this.Control.NodeRightHTMLSub = this.m_strNodeRightHTMLSub;
            }

            if (!string.IsNullOrEmpty(this.m_strNodeLeftHTMLBreadCrumbRoot))
            {
                this.Control.NodeLeftHTMLBreadCrumbRoot = this.m_strNodeLeftHTMLBreadCrumbRoot;
            }

            if (!string.IsNullOrEmpty(this.m_strNodeLeftHTMLBreadCrumbSub))
            {
                this.Control.NodeLeftHTMLBreadCrumbSub = this.m_strNodeLeftHTMLBreadCrumbSub;
            }

            if (!string.IsNullOrEmpty(this.m_strNodeRightHTMLBreadCrumbRoot))
            {
                this.Control.NodeRightHTMLBreadCrumbRoot = this.m_strNodeRightHTMLBreadCrumbRoot;
            }

            if (!string.IsNullOrEmpty(this.m_strNodeRightHTMLBreadCrumbSub))
            {
                this.Control.NodeRightHTMLBreadCrumbSub = this.m_strNodeRightHTMLBreadCrumbSub;
            }

            if (!string.IsNullOrEmpty(this.m_strSeparatorHTML))
            {
                this.Control.SeparatorHTML = this.m_strSeparatorHTML;
            }

            if (!string.IsNullOrEmpty(this.m_strSeparatorLeftHTML))
            {
                this.Control.SeparatorLeftHTML = this.m_strSeparatorLeftHTML;
            }

            if (!string.IsNullOrEmpty(this.m_strSeparatorRightHTML))
            {
                this.Control.SeparatorRightHTML = this.m_strSeparatorRightHTML;
            }

            if (!string.IsNullOrEmpty(this.m_strSeparatorLeftHTMLActive))
            {
                this.Control.SeparatorLeftHTMLActive = this.m_strSeparatorLeftHTMLActive;
            }

            if (!string.IsNullOrEmpty(this.m_strSeparatorRightHTMLActive))
            {
                this.Control.SeparatorRightHTMLActive = this.m_strSeparatorRightHTMLActive;
            }

            if (!string.IsNullOrEmpty(this.m_strSeparatorLeftHTMLBreadCrumb))
            {
                this.Control.SeparatorLeftHTMLBreadCrumb = this.m_strSeparatorLeftHTMLBreadCrumb;
            }

            if (!string.IsNullOrEmpty(this.m_strSeparatorRightHTMLBreadCrumb))
            {
                this.Control.SeparatorRightHTMLBreadCrumb = this.m_strSeparatorRightHTMLBreadCrumb;
            }

            if (!string.IsNullOrEmpty(this.m_strCSSControl))
            {
                this.Control.CSSControl = this.m_strCSSControl;
            }

            if (!string.IsNullOrEmpty(this.m_strCSSContainerRoot))
            {
                this.Control.CSSContainerRoot = this.m_strCSSContainerRoot;
            }

            if (!string.IsNullOrEmpty(this.m_strCSSNode))
            {
                this.Control.CSSNode = this.m_strCSSNode;
            }

            if (!string.IsNullOrEmpty(this.m_strCSSIcon))
            {
                this.Control.CSSIcon = this.m_strCSSIcon;
            }

            if (!string.IsNullOrEmpty(this.m_strCSSContainerSub))
            {
                this.Control.CSSContainerSub = this.m_strCSSContainerSub;
            }

            if (!string.IsNullOrEmpty(this.m_strCSSNodeHover))
            {
                this.Control.CSSNodeHover = this.m_strCSSNodeHover;
            }

            if (!string.IsNullOrEmpty(this.m_strCSSBreak))
            {
                this.Control.CSSBreak = this.m_strCSSBreak;
            }

            if (!string.IsNullOrEmpty(this.m_strCSSIndicateChildSub))
            {
                this.Control.CSSIndicateChildSub = this.m_strCSSIndicateChildSub;
            }

            if (!string.IsNullOrEmpty(this.m_strCSSIndicateChildRoot))
            {
                this.Control.CSSIndicateChildRoot = this.m_strCSSIndicateChildRoot;
            }

            if (!string.IsNullOrEmpty(this.m_strCSSBreadCrumbRoot))
            {
                this.Control.CSSBreadCrumbRoot = this.m_strCSSBreadCrumbRoot;
            }

            if (!string.IsNullOrEmpty(this.m_strCSSBreadCrumbSub))
            {
                this.Control.CSSBreadCrumbSub = this.m_strCSSBreadCrumbSub;
            }

            if (!string.IsNullOrEmpty(this.m_strCSSNodeRoot))
            {
                this.Control.CSSNodeRoot = this.m_strCSSNodeRoot;
            }

            if (!string.IsNullOrEmpty(this.m_strCSSNodeSelectedRoot))
            {
                this.Control.CSSNodeSelectedRoot = this.m_strCSSNodeSelectedRoot;
            }

            if (!string.IsNullOrEmpty(this.m_strCSSNodeSelectedSub))
            {
                this.Control.CSSNodeSelectedSub = this.m_strCSSNodeSelectedSub;
            }

            if (!string.IsNullOrEmpty(this.m_strCSSNodeHoverRoot))
            {
                this.Control.CSSNodeHoverRoot = this.m_strCSSNodeHoverRoot;
            }

            if (!string.IsNullOrEmpty(this.m_strCSSNodeHoverSub))
            {
                this.Control.CSSNodeHoverSub = this.m_strCSSNodeHoverSub;
            }

            if (!string.IsNullOrEmpty(this.m_strCSSSeparator))
            {
                this.Control.CSSSeparator = this.m_strCSSSeparator;
            }

            if (!string.IsNullOrEmpty(this.m_strCSSLeftSeparator))
            {
                this.Control.CSSLeftSeparator = this.m_strCSSLeftSeparator;
            }

            if (!string.IsNullOrEmpty(this.m_strCSSRightSeparator))
            {
                this.Control.CSSRightSeparator = this.m_strCSSRightSeparator;
            }

            if (!string.IsNullOrEmpty(this.m_strCSSLeftSeparatorSelection))
            {
                this.Control.CSSLeftSeparatorSelection = this.m_strCSSLeftSeparatorSelection;
            }

            if (!string.IsNullOrEmpty(this.m_strCSSRightSeparatorSelection))
            {
                this.Control.CSSRightSeparatorSelection = this.m_strCSSRightSeparatorSelection;
            }

            if (!string.IsNullOrEmpty(this.m_strCSSLeftSeparatorBreadCrumb))
            {
                this.Control.CSSLeftSeparatorBreadCrumb = this.m_strCSSLeftSeparatorBreadCrumb;
            }

            if (!string.IsNullOrEmpty(this.m_strCSSRightSeparatorBreadCrumb))
            {
                this.Control.CSSRightSeparatorBreadCrumb = this.m_strCSSRightSeparatorBreadCrumb;
            }

            if (!string.IsNullOrEmpty(this.m_strStyleBackColor))
            {
                this.Control.StyleBackColor = this.m_strStyleBackColor;
            }

            if (!string.IsNullOrEmpty(this.m_strStyleForeColor))
            {
                this.Control.StyleForeColor = this.m_strStyleForeColor;
            }

            if (!string.IsNullOrEmpty(this.m_strStyleHighlightColor))
            {
                this.Control.StyleHighlightColor = this.m_strStyleHighlightColor;
            }

            if (!string.IsNullOrEmpty(this.m_strStyleIconBackColor))
            {
                this.Control.StyleIconBackColor = this.m_strStyleIconBackColor;
            }

            if (!string.IsNullOrEmpty(this.m_strStyleSelectionBorderColor))
            {
                this.Control.StyleSelectionBorderColor = this.m_strStyleSelectionBorderColor;
            }

            if (!string.IsNullOrEmpty(this.m_strStyleSelectionColor))
            {
                this.Control.StyleSelectionColor = this.m_strStyleSelectionColor;
            }

            if (!string.IsNullOrEmpty(this.m_strStyleSelectionForeColor))
            {
                this.Control.StyleSelectionForeColor = this.m_strStyleSelectionForeColor;
            }

            if (!string.IsNullOrEmpty(this.m_strStyleControlHeight))
            {
                this.Control.StyleControlHeight = Convert.ToDecimal(this.m_strStyleControlHeight);
            }

            if (!string.IsNullOrEmpty(this.m_strStyleBorderWidth))
            {
                this.Control.StyleBorderWidth = Convert.ToDecimal(this.m_strStyleBorderWidth);
            }

            if (!string.IsNullOrEmpty(this.m_strStyleNodeHeight))
            {
                this.Control.StyleNodeHeight = Convert.ToDecimal(this.m_strStyleNodeHeight);
            }

            if (!string.IsNullOrEmpty(this.m_strStyleIconWidth))
            {
                this.Control.StyleIconWidth = Convert.ToDecimal(this.m_strStyleIconWidth);
            }

            if (!string.IsNullOrEmpty(this.m_strStyleFontNames))
            {
                this.Control.StyleFontNames = this.m_strStyleFontNames;
            }

            if (!string.IsNullOrEmpty(this.m_strStyleFontSize))
            {
                this.Control.StyleFontSize = Convert.ToDecimal(this.m_strStyleFontSize);
            }

            if (!string.IsNullOrEmpty(this.m_strStyleFontBold))
            {
                this.Control.StyleFontBold = this.m_strStyleFontBold;
            }

            if (!string.IsNullOrEmpty(this.m_strEffectsShadowColor))
            {
                this.Control.EffectsShadowColor = this.m_strEffectsShadowColor;
            }

            if (!string.IsNullOrEmpty(this.m_strEffectsStyle))
            {
                this.Control.EffectsStyle = this.m_strEffectsStyle;
            }

            if (!string.IsNullOrEmpty(this.m_strEffectsShadowStrength))
            {
                this.Control.EffectsShadowStrength = Convert.ToInt32(this.m_strEffectsShadowStrength);
            }

            if (!string.IsNullOrEmpty(this.m_strEffectsTransition))
            {
                this.Control.EffectsTransition = this.m_strEffectsTransition;
            }

            if (!string.IsNullOrEmpty(this.m_strEffectsDuration))
            {
                this.Control.EffectsDuration = Convert.ToDouble(this.m_strEffectsDuration);
            }

            if (!string.IsNullOrEmpty(this.m_strEffectsShadowDirection))
            {
                this.Control.EffectsShadowDirection = this.m_strEffectsShadowDirection;
            }

            this.Control.CustomAttributes = this.CustomAttributes;
        }

        private string GetPath(string strPath)
        {
            if (strPath.IndexOf("[SKINPATH]") > -1)
            {
                return strPath.Replace("[SKINPATH]", this.PortalSettings.ActiveTab.SkinPath);
            }
            else if (strPath.IndexOf("[APPIMAGEPATH]") > -1)
            {
                return strPath.Replace("[APPIMAGEPATH]", Globals.ApplicationPath + "/images/");
            }
            else if (strPath.IndexOf("[HOMEDIRECTORY]") > -1)
            {
                return strPath.Replace("[HOMEDIRECTORY]", this.PortalSettings.HomeDirectory);
            }
            else
            {
                if (strPath.StartsWith("~"))
                {
                    return this.ResolveUrl(strPath);
                }
            }

            return strPath;
        }
    }

    public class CustomAttribute
    {
        public string Name;
        public string Value;
    }
}
