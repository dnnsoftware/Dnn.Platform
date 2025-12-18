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
        private readonly IServiceProvider serviceProvider;
        private readonly List<CustomAttribute> objCustomAttributes = new List<CustomAttribute>();
        private bool blnPopulateNodesFromClient = true;
        private int intExpandDepth = -1;
        private int intStartTabId = -1;
        private NavigationProvider objControl;
        private string strCSSBreadCrumbRoot;

        private string strCSSBreadCrumbSub;
        private string strCSSBreak;
        private string strCSSContainerRoot;
        private string strCSSContainerSub;
        private string strCSSControl;
        private string strCSSIcon;
        private string strCSSIndicateChildRoot;
        private string strCSSIndicateChildSub;
        private string strCSSLeftSeparator;
        private string strCSSLeftSeparatorBreadCrumb;
        private string strCSSLeftSeparatorSelection;
        private string strCSSNode;
        private string strCSSNodeHover;
        private string strCSSNodeHoverRoot;
        private string strCSSNodeHoverSub;
        private string strCSSNodeRoot;
        private string strCSSNodeSelectedRoot;
        private string strCSSNodeSelectedSub;
        private string strCSSRightSeparator;
        private string strCSSRightSeparatorBreadCrumb;
        private string strCSSRightSeparatorSelection;
        private string strCSSSeparator;
        private string strControlAlignment;
        private string strControlOrientation;
        private string strEffectsDuration;
        private string strEffectsShadowColor;
        private string strEffectsShadowDirection;
        private string strEffectsShadowStrength;
        private string strEffectsStyle;
        private string strEffectsTransition;
        private string strForceCrawlerDisplay;
        private string strForceDownLevel;
        private string strIndicateChildImageExpandedRoot;
        private string strIndicateChildImageExpandedSub;
        private string strIndicateChildImageRoot;
        private string strIndicateChildImageSub;
        private string strIndicateChildren;
        private string strLevel = string.Empty;
        private string strMouseOutHideDelay;
        private string strMouseOverAction;
        private string strMouseOverDisplay;
        private string strNodeLeftHTMLBreadCrumbRoot;
        private string strNodeLeftHTMLBreadCrumbSub;
        private string strNodeLeftHTMLRoot;
        private string strNodeLeftHTMLSub;
        private string strNodeRightHTMLBreadCrumbRoot;
        private string strNodeRightHTMLBreadCrumbSub;
        private string strNodeRightHTMLRoot;
        private string strNodeRightHTMLSub;
        private string strPathImage;
        private string strPathSystemImage;
        private string strPathSystemScript;
        private string strProviderName = string.Empty;
        private string strSeparatorHTML;
        private string strSeparatorLeftHTML;
        private string strSeparatorLeftHTMLActive;
        private string strSeparatorLeftHTMLBreadCrumb;
        private string strSeparatorRightHTML;
        private string strSeparatorRightHTMLActive;
        private string strSeparatorRightHTMLBreadCrumb;
        private string strStyleBackColor;
        private string strStyleBorderWidth;
        private string strStyleControlHeight;
        private string strStyleFontBold;
        private string strStyleFontNames;
        private string strStyleFontSize;
        private string strStyleForeColor;
        private string strStyleHighlightColor;
        private string strStyleIconBackColor;
        private string strStyleIconWidth;
        private string strStyleNodeHeight;
        private string strStyleSelectionBorderColor;
        private string strStyleSelectionColor;
        private string strStyleSelectionForeColor;
        private string strToolTip = string.Empty;
        private string strWorkImage;

        /// <summary>Initializes a new instance of the <see cref="NavObjectBase"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IServiceProvider. Scheduled removal in v12.0.0.")]
        public NavObjectBase()
            : this(Globals.DependencyProvider)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="NavObjectBase"/> class.</summary>
        /// <param name="serviceProvider">The service provider.</param>
        public NavObjectBase(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        // JH - 2/5/07 - support for custom attributes
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public List<CustomAttribute> CustomAttributes
        {
            get
            {
                return this.objCustomAttributes;
            }
        }

        public bool ShowHiddenTabs { get; set; }

        public string ProviderName
        {
            get
            {
                return this.strProviderName;
            }

            set
            {
                this.strProviderName = value;
            }
        }

        public string Level
        {
            get
            {
                return this.strLevel;
            }

            set
            {
                this.strLevel = value;
            }
        }

        public string ToolTip
        {
            get
            {
                return this.strToolTip;
            }

            set
            {
                this.strToolTip = value;
            }
        }

        public bool PopulateNodesFromClient
        {
            get
            {
                return this.blnPopulateNodesFromClient;
            }

            set
            {
                this.blnPopulateNodesFromClient = value;
            }
        }

        public int ExpandDepth
        {
            get
            {
                return this.intExpandDepth;
            }

            set
            {
                this.intExpandDepth = value;
            }
        }

        public int StartTabId
        {
            get
            {
                return this.intStartTabId;
            }

            set
            {
                this.intStartTabId = value;
            }
        }

        public string PathSystemImage
        {
            get
            {
                if (this.Control == null)
                {
                    return this.strPathSystemImage;
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
                    this.strPathSystemImage = value;
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
                    return this.strPathImage;
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
                    this.strPathImage = value;
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
                    return this.strWorkImage;
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
                    this.strWorkImage = value;
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
                    return this.strPathSystemScript;
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
                    this.strPathSystemScript = value;
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
                    retValue = this.strControlOrientation;
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
                    this.strControlOrientation = value;
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
                    retValue = this.strControlAlignment;
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
                    this.strControlAlignment = value;
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
                    return this.strForceCrawlerDisplay;
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
                    this.strForceCrawlerDisplay = value;
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
                    return this.strForceDownLevel;
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
                    this.strForceDownLevel = value;
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
                    return this.strMouseOutHideDelay;
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
                    this.strMouseOutHideDelay = value;
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
                    retValue = this.strMouseOverDisplay;
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
                    this.strMouseOverDisplay = value;
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
                    retValue = this.strMouseOverAction;
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
                    this.strMouseOverAction = value;
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
                    return this.strIndicateChildren;
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
                    this.strIndicateChildren = value;
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
                    return this.strIndicateChildImageRoot;
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
                    this.strIndicateChildImageRoot = value;
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
                    return this.strIndicateChildImageSub;
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
                    this.strIndicateChildImageSub = value;
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
                    return this.strIndicateChildImageExpandedRoot;
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
                    this.strIndicateChildImageExpandedRoot = value;
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
                    return this.strIndicateChildImageExpandedSub;
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
                    this.strIndicateChildImageExpandedSub = value;
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
                    return this.strNodeLeftHTMLRoot;
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
                    this.strNodeLeftHTMLRoot = value;
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
                    return this.strNodeRightHTMLRoot;
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
                    this.strNodeRightHTMLRoot = value;
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
                    return this.strNodeLeftHTMLSub;
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
                    this.strNodeLeftHTMLSub = value;
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
                    return this.strNodeRightHTMLSub;
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
                    this.strNodeRightHTMLSub = value;
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
                    return this.strNodeLeftHTMLBreadCrumbRoot;
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
                    this.strNodeLeftHTMLBreadCrumbRoot = value;
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
                    return this.strNodeLeftHTMLBreadCrumbSub;
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
                    this.strNodeLeftHTMLBreadCrumbSub = value;
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
                    return this.strNodeRightHTMLBreadCrumbRoot;
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
                    this.strNodeRightHTMLBreadCrumbRoot = value;
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
                    return this.strNodeRightHTMLBreadCrumbSub;
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
                    this.strNodeRightHTMLBreadCrumbSub = value;
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
                    return this.strSeparatorHTML;
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
                    this.strSeparatorHTML = value;
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
                    return this.strSeparatorLeftHTML;
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
                    this.strSeparatorLeftHTML = value;
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
                    return this.strSeparatorRightHTML;
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
                    this.strSeparatorRightHTML = value;
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
                    return this.strSeparatorLeftHTMLActive;
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
                    this.strSeparatorLeftHTMLActive = value;
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
                    return this.strSeparatorRightHTMLActive;
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
                    this.strSeparatorRightHTMLActive = value;
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
                    return this.strSeparatorLeftHTMLBreadCrumb;
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
                    this.strSeparatorLeftHTMLBreadCrumb = value;
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
                    return this.strSeparatorRightHTMLBreadCrumb;
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
                    this.strSeparatorRightHTMLBreadCrumb = value;
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
                    return this.strCSSControl;
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
                    this.strCSSControl = value;
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
                    return this.strCSSContainerRoot;
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
                    this.strCSSContainerRoot = value;
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
                    return this.strCSSNode;
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
                    this.strCSSNode = value;
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
                    return this.strCSSIcon;
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
                    this.strCSSIcon = value;
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
                    return this.strCSSContainerSub;
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
                    this.strCSSContainerSub = value;
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
                    return this.strCSSNodeHover;
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
                    this.strCSSNodeHover = value;
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
                    return this.strCSSBreak;
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
                    this.strCSSBreak = value;
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
                    return this.strCSSIndicateChildSub;
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
                    this.strCSSIndicateChildSub = value;
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
                    return this.strCSSIndicateChildRoot;
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
                    this.strCSSIndicateChildRoot = value;
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
                    return this.strCSSBreadCrumbRoot;
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
                    this.strCSSBreadCrumbRoot = value;
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
                    return this.strCSSBreadCrumbSub;
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
                    this.strCSSBreadCrumbSub = value;
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
                    return this.strCSSNodeRoot;
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
                    this.strCSSNodeRoot = value;
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
                    return this.strCSSNodeSelectedRoot;
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
                    this.strCSSNodeSelectedRoot = value;
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
                    return this.strCSSNodeSelectedSub;
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
                    this.strCSSNodeSelectedSub = value;
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
                    return this.strCSSNodeHoverRoot;
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
                    this.strCSSNodeHoverRoot = value;
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
                    return this.strCSSNodeHoverSub;
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
                    this.strCSSNodeHoverSub = value;
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
                    return this.strCSSSeparator;
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
                    this.strCSSSeparator = value;
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
                    return this.strCSSLeftSeparator;
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
                    this.strCSSLeftSeparator = value;
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
                    return this.strCSSRightSeparator;
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
                    this.strCSSRightSeparator = value;
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
                    return this.strCSSLeftSeparatorSelection;
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
                    this.strCSSLeftSeparatorSelection = value;
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
                    return this.strCSSRightSeparatorSelection;
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
                    this.strCSSRightSeparatorSelection = value;
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
                    return this.strCSSLeftSeparatorBreadCrumb;
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
                    this.strCSSLeftSeparatorBreadCrumb = value;
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
                    return this.strCSSRightSeparatorBreadCrumb;
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
                    this.strCSSRightSeparatorBreadCrumb = value;
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
                    return this.strStyleBackColor;
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
                    this.strStyleBackColor = value;
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
                    return this.strStyleForeColor;
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
                    this.strStyleForeColor = value;
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
                    return this.strStyleHighlightColor;
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
                    this.strStyleHighlightColor = value;
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
                    return this.strStyleIconBackColor;
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
                    this.strStyleIconBackColor = value;
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
                    return this.strStyleSelectionBorderColor;
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
                    this.strStyleSelectionBorderColor = value;
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
                    return this.strStyleSelectionColor;
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
                    this.strStyleSelectionColor = value;
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
                    return this.strStyleSelectionForeColor;
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
                    this.strStyleSelectionForeColor = value;
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
                    return this.strStyleControlHeight;
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
                    this.strStyleControlHeight = value;
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
                    return this.strStyleBorderWidth;
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
                    this.strStyleBorderWidth = value;
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
                    return this.strStyleNodeHeight;
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
                    this.strStyleNodeHeight = value;
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
                    return this.strStyleIconWidth;
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
                    this.strStyleIconWidth = value;
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
                    return this.strStyleFontNames;
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
                    this.strStyleFontNames = value;
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
                    return this.strStyleFontSize;
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
                    this.strStyleFontSize = value;
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
                    return this.strStyleFontBold;
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
                    this.strStyleFontBold = value;
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
                    return this.strEffectsShadowColor;
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
                    this.strEffectsShadowColor = value;
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
                    return this.strEffectsStyle;
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
                    this.strEffectsStyle = value;
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
                    return this.strEffectsShadowStrength;
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
                    this.strEffectsShadowStrength = value;
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
                    return this.strEffectsTransition;
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
                    this.strEffectsTransition = value;
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
                    return this.strEffectsDuration;
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
                    this.strEffectsDuration = value;
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
                    return this.strEffectsShadowDirection;
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
                    this.strEffectsShadowDirection = value;
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
                return this.objControl;
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

            this.objControl = NavigationProvider.Instance(this.serviceProvider, this.ProviderName);
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
            if (!string.IsNullOrEmpty(this.strPathSystemImage))
            {
                this.Control.PathSystemImage = this.strPathSystemImage;
            }

            if (!string.IsNullOrEmpty(this.strPathImage))
            {
                this.Control.PathImage = this.strPathImage;
            }

            if (!string.IsNullOrEmpty(this.strPathSystemScript))
            {
                this.Control.PathSystemScript = this.strPathSystemScript;
            }

            if (!string.IsNullOrEmpty(this.strWorkImage))
            {
                this.Control.WorkImage = this.strWorkImage;
            }

            if (!string.IsNullOrEmpty(this.strControlOrientation))
            {
                switch (this.strControlOrientation.ToLowerInvariant())
                {
                    case "horizontal":
                        this.Control.ControlOrientation = NavigationProvider.Orientation.Horizontal;
                        break;
                    case "vertical":
                        this.Control.ControlOrientation = NavigationProvider.Orientation.Vertical;
                        break;
                }
            }

            if (!string.IsNullOrEmpty(this.strControlAlignment))
            {
                switch (this.strControlAlignment.ToLowerInvariant())
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

            this.Control.ForceCrawlerDisplay = this.GetValue(this.strForceCrawlerDisplay, "False");
            this.Control.ForceDownLevel = this.GetValue(this.strForceDownLevel, "False");
            if (!string.IsNullOrEmpty(this.strMouseOutHideDelay))
            {
                this.Control.MouseOutHideDelay = Convert.ToDecimal(this.strMouseOutHideDelay);
            }

            if (!string.IsNullOrEmpty(this.strMouseOverDisplay))
            {
                switch (this.strMouseOverDisplay.ToLowerInvariant())
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

            if (Convert.ToBoolean(this.GetValue(this.strMouseOverAction, "True")))
            {
                this.Control.MouseOverAction = NavigationProvider.HoverAction.Expand;
            }
            else
            {
                this.Control.MouseOverAction = NavigationProvider.HoverAction.None;
            }

            this.Control.IndicateChildren = Convert.ToBoolean(this.GetValue(this.strIndicateChildren, "True"));
            if (!string.IsNullOrEmpty(this.strIndicateChildImageRoot))
            {
                this.Control.IndicateChildImageRoot = this.strIndicateChildImageRoot;
            }

            if (!string.IsNullOrEmpty(this.strIndicateChildImageSub))
            {
                this.Control.IndicateChildImageSub = this.strIndicateChildImageSub;
            }

            if (!string.IsNullOrEmpty(this.strIndicateChildImageExpandedRoot))
            {
                this.Control.IndicateChildImageExpandedRoot = this.strIndicateChildImageExpandedRoot;
            }

            if (!string.IsNullOrEmpty(this.strIndicateChildImageExpandedSub))
            {
                this.Control.IndicateChildImageExpandedSub = this.strIndicateChildImageExpandedSub;
            }

            if (!string.IsNullOrEmpty(this.strNodeLeftHTMLRoot))
            {
                this.Control.NodeLeftHTMLRoot = this.strNodeLeftHTMLRoot;
            }

            if (!string.IsNullOrEmpty(this.strNodeRightHTMLRoot))
            {
                this.Control.NodeRightHTMLRoot = this.strNodeRightHTMLRoot;
            }

            if (!string.IsNullOrEmpty(this.strNodeLeftHTMLSub))
            {
                this.Control.NodeLeftHTMLSub = this.strNodeLeftHTMLSub;
            }

            if (!string.IsNullOrEmpty(this.strNodeRightHTMLSub))
            {
                this.Control.NodeRightHTMLSub = this.strNodeRightHTMLSub;
            }

            if (!string.IsNullOrEmpty(this.strNodeLeftHTMLBreadCrumbRoot))
            {
                this.Control.NodeLeftHTMLBreadCrumbRoot = this.strNodeLeftHTMLBreadCrumbRoot;
            }

            if (!string.IsNullOrEmpty(this.strNodeLeftHTMLBreadCrumbSub))
            {
                this.Control.NodeLeftHTMLBreadCrumbSub = this.strNodeLeftHTMLBreadCrumbSub;
            }

            if (!string.IsNullOrEmpty(this.strNodeRightHTMLBreadCrumbRoot))
            {
                this.Control.NodeRightHTMLBreadCrumbRoot = this.strNodeRightHTMLBreadCrumbRoot;
            }

            if (!string.IsNullOrEmpty(this.strNodeRightHTMLBreadCrumbSub))
            {
                this.Control.NodeRightHTMLBreadCrumbSub = this.strNodeRightHTMLBreadCrumbSub;
            }

            if (!string.IsNullOrEmpty(this.strSeparatorHTML))
            {
                this.Control.SeparatorHTML = this.strSeparatorHTML;
            }

            if (!string.IsNullOrEmpty(this.strSeparatorLeftHTML))
            {
                this.Control.SeparatorLeftHTML = this.strSeparatorLeftHTML;
            }

            if (!string.IsNullOrEmpty(this.strSeparatorRightHTML))
            {
                this.Control.SeparatorRightHTML = this.strSeparatorRightHTML;
            }

            if (!string.IsNullOrEmpty(this.strSeparatorLeftHTMLActive))
            {
                this.Control.SeparatorLeftHTMLActive = this.strSeparatorLeftHTMLActive;
            }

            if (!string.IsNullOrEmpty(this.strSeparatorRightHTMLActive))
            {
                this.Control.SeparatorRightHTMLActive = this.strSeparatorRightHTMLActive;
            }

            if (!string.IsNullOrEmpty(this.strSeparatorLeftHTMLBreadCrumb))
            {
                this.Control.SeparatorLeftHTMLBreadCrumb = this.strSeparatorLeftHTMLBreadCrumb;
            }

            if (!string.IsNullOrEmpty(this.strSeparatorRightHTMLBreadCrumb))
            {
                this.Control.SeparatorRightHTMLBreadCrumb = this.strSeparatorRightHTMLBreadCrumb;
            }

            if (!string.IsNullOrEmpty(this.strCSSControl))
            {
                this.Control.CSSControl = this.strCSSControl;
            }

            if (!string.IsNullOrEmpty(this.strCSSContainerRoot))
            {
                this.Control.CSSContainerRoot = this.strCSSContainerRoot;
            }

            if (!string.IsNullOrEmpty(this.strCSSNode))
            {
                this.Control.CSSNode = this.strCSSNode;
            }

            if (!string.IsNullOrEmpty(this.strCSSIcon))
            {
                this.Control.CSSIcon = this.strCSSIcon;
            }

            if (!string.IsNullOrEmpty(this.strCSSContainerSub))
            {
                this.Control.CSSContainerSub = this.strCSSContainerSub;
            }

            if (!string.IsNullOrEmpty(this.strCSSNodeHover))
            {
                this.Control.CSSNodeHover = this.strCSSNodeHover;
            }

            if (!string.IsNullOrEmpty(this.strCSSBreak))
            {
                this.Control.CSSBreak = this.strCSSBreak;
            }

            if (!string.IsNullOrEmpty(this.strCSSIndicateChildSub))
            {
                this.Control.CSSIndicateChildSub = this.strCSSIndicateChildSub;
            }

            if (!string.IsNullOrEmpty(this.strCSSIndicateChildRoot))
            {
                this.Control.CSSIndicateChildRoot = this.strCSSIndicateChildRoot;
            }

            if (!string.IsNullOrEmpty(this.strCSSBreadCrumbRoot))
            {
                this.Control.CSSBreadCrumbRoot = this.strCSSBreadCrumbRoot;
            }

            if (!string.IsNullOrEmpty(this.strCSSBreadCrumbSub))
            {
                this.Control.CSSBreadCrumbSub = this.strCSSBreadCrumbSub;
            }

            if (!string.IsNullOrEmpty(this.strCSSNodeRoot))
            {
                this.Control.CSSNodeRoot = this.strCSSNodeRoot;
            }

            if (!string.IsNullOrEmpty(this.strCSSNodeSelectedRoot))
            {
                this.Control.CSSNodeSelectedRoot = this.strCSSNodeSelectedRoot;
            }

            if (!string.IsNullOrEmpty(this.strCSSNodeSelectedSub))
            {
                this.Control.CSSNodeSelectedSub = this.strCSSNodeSelectedSub;
            }

            if (!string.IsNullOrEmpty(this.strCSSNodeHoverRoot))
            {
                this.Control.CSSNodeHoverRoot = this.strCSSNodeHoverRoot;
            }

            if (!string.IsNullOrEmpty(this.strCSSNodeHoverSub))
            {
                this.Control.CSSNodeHoverSub = this.strCSSNodeHoverSub;
            }

            if (!string.IsNullOrEmpty(this.strCSSSeparator))
            {
                this.Control.CSSSeparator = this.strCSSSeparator;
            }

            if (!string.IsNullOrEmpty(this.strCSSLeftSeparator))
            {
                this.Control.CSSLeftSeparator = this.strCSSLeftSeparator;
            }

            if (!string.IsNullOrEmpty(this.strCSSRightSeparator))
            {
                this.Control.CSSRightSeparator = this.strCSSRightSeparator;
            }

            if (!string.IsNullOrEmpty(this.strCSSLeftSeparatorSelection))
            {
                this.Control.CSSLeftSeparatorSelection = this.strCSSLeftSeparatorSelection;
            }

            if (!string.IsNullOrEmpty(this.strCSSRightSeparatorSelection))
            {
                this.Control.CSSRightSeparatorSelection = this.strCSSRightSeparatorSelection;
            }

            if (!string.IsNullOrEmpty(this.strCSSLeftSeparatorBreadCrumb))
            {
                this.Control.CSSLeftSeparatorBreadCrumb = this.strCSSLeftSeparatorBreadCrumb;
            }

            if (!string.IsNullOrEmpty(this.strCSSRightSeparatorBreadCrumb))
            {
                this.Control.CSSRightSeparatorBreadCrumb = this.strCSSRightSeparatorBreadCrumb;
            }

            if (!string.IsNullOrEmpty(this.strStyleBackColor))
            {
                this.Control.StyleBackColor = this.strStyleBackColor;
            }

            if (!string.IsNullOrEmpty(this.strStyleForeColor))
            {
                this.Control.StyleForeColor = this.strStyleForeColor;
            }

            if (!string.IsNullOrEmpty(this.strStyleHighlightColor))
            {
                this.Control.StyleHighlightColor = this.strStyleHighlightColor;
            }

            if (!string.IsNullOrEmpty(this.strStyleIconBackColor))
            {
                this.Control.StyleIconBackColor = this.strStyleIconBackColor;
            }

            if (!string.IsNullOrEmpty(this.strStyleSelectionBorderColor))
            {
                this.Control.StyleSelectionBorderColor = this.strStyleSelectionBorderColor;
            }

            if (!string.IsNullOrEmpty(this.strStyleSelectionColor))
            {
                this.Control.StyleSelectionColor = this.strStyleSelectionColor;
            }

            if (!string.IsNullOrEmpty(this.strStyleSelectionForeColor))
            {
                this.Control.StyleSelectionForeColor = this.strStyleSelectionForeColor;
            }

            if (!string.IsNullOrEmpty(this.strStyleControlHeight))
            {
                this.Control.StyleControlHeight = Convert.ToDecimal(this.strStyleControlHeight);
            }

            if (!string.IsNullOrEmpty(this.strStyleBorderWidth))
            {
                this.Control.StyleBorderWidth = Convert.ToDecimal(this.strStyleBorderWidth);
            }

            if (!string.IsNullOrEmpty(this.strStyleNodeHeight))
            {
                this.Control.StyleNodeHeight = Convert.ToDecimal(this.strStyleNodeHeight);
            }

            if (!string.IsNullOrEmpty(this.strStyleIconWidth))
            {
                this.Control.StyleIconWidth = Convert.ToDecimal(this.strStyleIconWidth);
            }

            if (!string.IsNullOrEmpty(this.strStyleFontNames))
            {
                this.Control.StyleFontNames = this.strStyleFontNames;
            }

            if (!string.IsNullOrEmpty(this.strStyleFontSize))
            {
                this.Control.StyleFontSize = Convert.ToDecimal(this.strStyleFontSize);
            }

            if (!string.IsNullOrEmpty(this.strStyleFontBold))
            {
                this.Control.StyleFontBold = this.strStyleFontBold;
            }

            if (!string.IsNullOrEmpty(this.strEffectsShadowColor))
            {
                this.Control.EffectsShadowColor = this.strEffectsShadowColor;
            }

            if (!string.IsNullOrEmpty(this.strEffectsStyle))
            {
                this.Control.EffectsStyle = this.strEffectsStyle;
            }

            if (!string.IsNullOrEmpty(this.strEffectsShadowStrength))
            {
                this.Control.EffectsShadowStrength = Convert.ToInt32(this.strEffectsShadowStrength);
            }

            if (!string.IsNullOrEmpty(this.strEffectsTransition))
            {
                this.Control.EffectsTransition = this.strEffectsTransition;
            }

            if (!string.IsNullOrEmpty(this.strEffectsDuration))
            {
                this.Control.EffectsDuration = Convert.ToDouble(this.strEffectsDuration);
            }

            if (!string.IsNullOrEmpty(this.strEffectsShadowDirection))
            {
                this.Control.EffectsShadowDirection = this.strEffectsShadowDirection;
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
}
