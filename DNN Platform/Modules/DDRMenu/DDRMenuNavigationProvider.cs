// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Web.UI;

    using DotNetNuke.Modules.NavigationProvider;
    using DotNetNuke.UI.Skins;
    using DotNetNuke.UI.WebControls;
    using DotNetNuke.Web.DDRMenu.Localisation;
    using DotNetNuke.Web.DDRMenu.TemplateEngine;

    public class DDRMenuNavigationProvider : NavigationProvider
    {
        private DDRMenuControl menuControl;

        public override Control NavigationControl
        {
            get { return this.menuControl; }
        }

        public override bool SupportsPopulateOnDemand
        {
            get { return false; }
        }

        public override Alignment ControlAlignment { get; set; }

        public override bool IndicateChildren { get; set; }

        public override bool PopulateNodesFromClient { get; set; }

        public override decimal MouseOutHideDelay { get; set; }

        public override decimal StyleBorderWidth { get; set; }

        public override decimal StyleControlHeight { get; set; }

        public override decimal StyleFontSize { get; set; }

        public override decimal StyleIconWidth { get; set; }

        public override decimal StyleNodeHeight { get; set; }

        public override double EffectsDuration { get; set; }

        public override HoverAction MouseOverAction { get; set; }

        public override HoverDisplay MouseOverDisplay { get; set; }

        public override int EffectsShadowStrength { get; set; }

        public override Orientation ControlOrientation { get; set; }

        public override string CSSBreadCrumbRoot { get; set; }

        public override string CSSBreadCrumbSub { get; set; }

        public override string CSSBreak { get; set; }

        public override string CSSContainerRoot { get; set; }

        public override string CSSContainerSub { get; set; }

        public override string CSSIcon { get; set; }

        public override string CSSIndicateChildRoot { get; set; }

        public override string CSSIndicateChildSub { get; set; }

        public override string CSSLeftSeparator { get; set; }

        public override string CSSLeftSeparatorBreadCrumb { get; set; }

        public override string CSSLeftSeparatorSelection { get; set; }

        public override string CSSNode { get; set; }

        public override string CSSNodeHover { get; set; }

        public override string CSSNodeHoverRoot { get; set; }

        public override string CSSNodeHoverSub { get; set; }

        public override string CSSNodeRoot { get; set; }

        public override string CSSNodeSelectedRoot { get; set; }

        public override string CSSNodeSelectedSub { get; set; }

        public override string CSSRightSeparator { get; set; }

        public override string CSSRightSeparatorBreadCrumb { get; set; }

        public override string CSSRightSeparatorSelection { get; set; }

        public override string CSSSeparator { get; set; }

        public override string EffectsShadowColor { get; set; }

        public override string EffectsShadowDirection { get; set; }

        public override string EffectsStyle { get; set; }

        public override string EffectsTransition { get; set; }

        public override string ForceCrawlerDisplay { get; set; }

        public override string ForceDownLevel { get; set; }

        public override string IndicateChildImageExpandedRoot { get; set; }

        public override string IndicateChildImageExpandedSub { get; set; }

        public override string IndicateChildImageRoot { get; set; }

        public override string IndicateChildImageSub { get; set; }

        public override string NodeLeftHTMLBreadCrumbRoot { get; set; }

        public override string NodeLeftHTMLBreadCrumbSub { get; set; }

        public override string NodeLeftHTMLRoot { get; set; }

        public override string NodeLeftHTMLSub { get; set; }

        public override string NodeRightHTMLBreadCrumbRoot { get; set; }

        public override string NodeRightHTMLBreadCrumbSub { get; set; }

        public override string NodeRightHTMLRoot { get; set; }

        public override string NodeRightHTMLSub { get; set; }

        public override string PathImage { get; set; }

        public override string PathSystemImage { get; set; }

        public override string PathSystemScript { get; set; }

        public override string SeparatorHTML { get; set; }

        public override string SeparatorLeftHTML { get; set; }

        public override string SeparatorLeftHTMLActive { get; set; }

        public override string SeparatorLeftHTMLBreadCrumb { get; set; }

        public override string SeparatorRightHTML { get; set; }

        public override string SeparatorRightHTMLActive { get; set; }

        public override string SeparatorRightHTMLBreadCrumb { get; set; }

        public override string StyleBackColor { get; set; }

        public override string StyleFontBold { get; set; }

        public override string StyleFontNames { get; set; }

        public override string StyleForeColor { get; set; }

        public override string StyleHighlightColor { get; set; }

        public override string StyleIconBackColor { get; set; }

        public override string StyleRoot { get; set; }

        public override string StyleSelectionBorderColor { get; set; }

        public override string StyleSelectionColor { get; set; }

        public override string StyleSelectionForeColor { get; set; }

        public override string StyleSub { get; set; }

        public override string WorkImage { get; set; }

        public override List<CustomAttribute> CustomAttributes { get; set; }

        public override string ControlID { get; set; }

        public override string CSSControl { get; set; }

        public string MenuStyle { get; set; }

        public List<TemplateArgument> TemplateArguments { get; set; }

        public override void Initialize()
        {
            this.menuControl = new DDRMenuControl { ID = this.ControlID, EnableViewState = false };
            this.menuControl.NodeClick += this.RaiseEvent_NodeClick;
        }

        public override void Bind(DNNNodeCollection objNodes)
        {
            this.Bind(objNodes, true);
        }

        public void Bind(DNNNodeCollection objNodes, bool localise)
        {
            var clientOptions = new List<ClientOption>();

            var ignoreProperties = new List<string>
                                   {
                                    "CustomAttributes",
                                    "NavigationControl",
                                    "SupportsPopulateOnDemand",
                                    "NodeXmlPath",
                                    "NodeSelector",
                                    "IncludeContext",
                                    "IncludeHidden",
                                    "IncludeNodes",
                                    "ExcludeNodes",
                                    "NodeManipulator",
                                   };

            foreach (var prop in
                typeof(NavigationProvider).GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance))
            {
                if (!string.IsNullOrEmpty(prop.Name) && !ignoreProperties.Contains(prop.Name))
                {
                    var propValue = prop.GetValue(this, null);
                    if (propValue != null)
                    {
                        if (propValue is bool)
                        {
                            clientOptions.Add(new ClientBoolean(prop.Name, propValue.ToString()));
                        }
                        else if (propValue is int || propValue is decimal || propValue is double)
                        {
                            clientOptions.Add(new ClientNumber(prop.Name, propValue.ToString()));
                        }
                        else
                        {
                            clientOptions.Add(new ClientString(prop.Name, propValue.ToString()));
                        }
                    }
                }
            }

            if (this.CustomAttributes != null)
            {
                foreach (var attr in this.CustomAttributes)
                {
                    if (!string.IsNullOrEmpty(attr.Name))
                    {
                        clientOptions.Add(new ClientString(attr.Name, attr.Value));
                    }
                }
            }

            if (localise)
            {
                objNodes = Localiser.LocaliseDNNNodeCollection(objNodes);
            }

            this.menuControl.RootNode = new MenuNode(objNodes);
            this.menuControl.SkipLocalisation = !localise;
            this.menuControl.MenuSettings = new Settings
            {
                MenuStyle = this.GetCustomAttribute("MenuStyle") ?? this.MenuStyle ?? "DNNMenu",
                NodeXmlPath = this.GetCustomAttribute("NodeXmlPath"),
                NodeSelector = this.GetCustomAttribute("NodeSelector"),
                IncludeContext = Convert.ToBoolean(this.GetCustomAttribute("IncludeContext") ?? "false"),
                IncludeHidden = Convert.ToBoolean(this.GetCustomAttribute("IncludeHidden") ?? "false"),
                IncludeNodes = this.GetCustomAttribute("IncludeNodes"),
                ExcludeNodes = this.GetCustomAttribute("ExcludeNodes"),
                NodeManipulator = this.GetCustomAttribute("NodeManipulator"),
                ClientOptions = clientOptions,
                TemplateArguments = this.TemplateArguments,
            };
        }

        private string GetCustomAttribute(string attributeName)
        {
            string xmlValue = null;
            if (this.CustomAttributes != null)
            {
                var xmlAttr = this.CustomAttributes.Find(a => a.Name.Equals(attributeName, StringComparison.InvariantCultureIgnoreCase));
                if (xmlAttr != null)
                {
                    xmlValue = xmlAttr.Value;
                }
            }

            return xmlValue;
        }
    }
}
