// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.UI;

using DotNetNuke.Modules.NavigationProvider;
using DotNetNuke.UI.Skins;
using DotNetNuke.UI.WebControls;
using DotNetNuke.Web.DDRMenu.Localisation;
using DotNetNuke.Web.DDRMenu.TemplateEngine;

/// <summary>Implementes the DDR Menu navigation profider.</summary>
public class DDRMenuNavigationProvider : NavigationProvider
{
    private DDRMenuControl menuControl;

    /// <inheritdoc/>
    public override Control NavigationControl
    {
        get { return this.menuControl; }
    }

    /// <inheritdoc/>
    public override bool SupportsPopulateOnDemand
    {
        get { return false; }
    }

    /// <inheritdoc/>
    public override Alignment ControlAlignment { get; set; }

    /// <inheritdoc/>
    public override bool IndicateChildren { get; set; }

    /// <inheritdoc/>
    public override bool PopulateNodesFromClient { get; set; }

    /// <inheritdoc/>
    public override decimal MouseOutHideDelay { get; set; }

    /// <inheritdoc/>
    public override decimal StyleBorderWidth { get; set; }

    /// <inheritdoc/>
    public override decimal StyleControlHeight { get; set; }

    /// <inheritdoc/>
    public override decimal StyleFontSize { get; set; }

    /// <inheritdoc/>
    public override decimal StyleIconWidth { get; set; }

    /// <inheritdoc/>
    public override decimal StyleNodeHeight { get; set; }

    /// <inheritdoc/>
    public override double EffectsDuration { get; set; }

    /// <inheritdoc/>
    public override HoverAction MouseOverAction { get; set; }

    /// <inheritdoc/>
    public override HoverDisplay MouseOverDisplay { get; set; }

    /// <inheritdoc/>
    public override int EffectsShadowStrength { get; set; }

    /// <inheritdoc/>
    public override Orientation ControlOrientation { get; set; }

    /// <inheritdoc/>
    public override string CSSBreadCrumbRoot { get; set; }

    /// <inheritdoc/>
    public override string CSSBreadCrumbSub { get; set; }

    /// <inheritdoc/>
    public override string CSSBreak { get; set; }

    /// <inheritdoc/>
    public override string CSSContainerRoot { get; set; }

    /// <inheritdoc/>
    public override string CSSContainerSub { get; set; }

    /// <inheritdoc/>
    public override string CSSIcon { get; set; }

    /// <inheritdoc/>
    public override string CSSIndicateChildRoot { get; set; }

    /// <inheritdoc/>
    public override string CSSIndicateChildSub { get; set; }

    /// <inheritdoc/>
    public override string CSSLeftSeparator { get; set; }

    /// <inheritdoc/>
    public override string CSSLeftSeparatorBreadCrumb { get; set; }

    /// <inheritdoc/>
    public override string CSSLeftSeparatorSelection { get; set; }

    /// <inheritdoc/>
    public override string CSSNode { get; set; }

    /// <inheritdoc/>
    public override string CSSNodeHover { get; set; }

    /// <inheritdoc/>
    public override string CSSNodeHoverRoot { get; set; }

    /// <inheritdoc/>
    public override string CSSNodeHoverSub { get; set; }

    /// <inheritdoc/>
    public override string CSSNodeRoot { get; set; }

    /// <inheritdoc/>
    public override string CSSNodeSelectedRoot { get; set; }

    /// <inheritdoc/>
    public override string CSSNodeSelectedSub { get; set; }

    /// <inheritdoc/>
    public override string CSSRightSeparator { get; set; }

    /// <inheritdoc/>
    public override string CSSRightSeparatorBreadCrumb { get; set; }

    /// <inheritdoc/>
    public override string CSSRightSeparatorSelection { get; set; }

    /// <inheritdoc/>
    public override string CSSSeparator { get; set; }

    /// <inheritdoc/>
    public override string EffectsShadowColor { get; set; }

    /// <inheritdoc/>
    public override string EffectsShadowDirection { get; set; }

    /// <inheritdoc/>
    public override string EffectsStyle { get; set; }

    /// <inheritdoc/>
    public override string EffectsTransition { get; set; }

    /// <inheritdoc/>
    public override string ForceCrawlerDisplay { get; set; }

    /// <inheritdoc/>
    public override string ForceDownLevel { get; set; }

    /// <inheritdoc/>
    public override string IndicateChildImageExpandedRoot { get; set; }

    /// <inheritdoc/>
    public override string IndicateChildImageExpandedSub { get; set; }

    /// <inheritdoc/>
    public override string IndicateChildImageRoot { get; set; }

    /// <inheritdoc/>
    public override string IndicateChildImageSub { get; set; }

    /// <inheritdoc/>
    public override string NodeLeftHTMLBreadCrumbRoot { get; set; }

    /// <inheritdoc/>
    public override string NodeLeftHTMLBreadCrumbSub { get; set; }

    /// <inheritdoc/>
    public override string NodeLeftHTMLRoot { get; set; }

    /// <inheritdoc/>
    public override string NodeLeftHTMLSub { get; set; }

    /// <inheritdoc/>
    public override string NodeRightHTMLBreadCrumbRoot { get; set; }

    /// <inheritdoc/>
    public override string NodeRightHTMLBreadCrumbSub { get; set; }

    /// <inheritdoc/>
    public override string NodeRightHTMLRoot { get; set; }

    /// <inheritdoc/>
    public override string NodeRightHTMLSub { get; set; }

    /// <inheritdoc/>
    public override string PathImage { get; set; }

    /// <inheritdoc/>
    public override string PathSystemImage { get; set; }

    /// <inheritdoc/>
    public override string PathSystemScript { get; set; }

    /// <inheritdoc/>
    public override string SeparatorHTML { get; set; }

    /// <inheritdoc/>
    public override string SeparatorLeftHTML { get; set; }

    /// <inheritdoc/>
    public override string SeparatorLeftHTMLActive { get; set; }

    /// <inheritdoc/>
    public override string SeparatorLeftHTMLBreadCrumb { get; set; }

    /// <inheritdoc/>
    public override string SeparatorRightHTML { get; set; }

    /// <inheritdoc/>
    public override string SeparatorRightHTMLActive { get; set; }

    /// <inheritdoc/>
    public override string SeparatorRightHTMLBreadCrumb { get; set; }

    /// <inheritdoc/>
    public override string StyleBackColor { get; set; }

    /// <inheritdoc/>
    public override string StyleFontBold { get; set; }

    /// <inheritdoc/>
    public override string StyleFontNames { get; set; }

    /// <inheritdoc/>
    public override string StyleForeColor { get; set; }

    /// <inheritdoc/>
    public override string StyleHighlightColor { get; set; }

    /// <inheritdoc/>
    public override string StyleIconBackColor { get; set; }

    /// <inheritdoc/>
    public override string StyleRoot { get; set; }

    /// <inheritdoc/>
    public override string StyleSelectionBorderColor { get; set; }

    /// <inheritdoc/>
    public override string StyleSelectionColor { get; set; }

    /// <inheritdoc/>
    public override string StyleSelectionForeColor { get; set; }

    /// <inheritdoc/>
    public override string StyleSub { get; set; }

    /// <inheritdoc/>
    public override string WorkImage { get; set; }

    /// <inheritdoc/>
    public override List<CustomAttribute> CustomAttributes { get; set; }

    /// <inheritdoc/>
    public override string ControlID { get; set; }

    /// <inheritdoc/>
    public override string CSSControl { get; set; }

    /// <summary>Gets or sets the style of the menu.</summary>
    public string MenuStyle { get; set; }

    /// <summary>Gets or sets the template arguments, <see cref="TemplateArgument"/>.</summary>
    public List<TemplateArgument> TemplateArguments { get; set; }

    /// <inheritdoc/>
    public override void Initialize()
    {
        this.menuControl = new DDRMenuControl { ID = this.ControlID, EnableViewState = false };
        this.menuControl.NodeClick += this.RaiseEvent_NodeClick;
    }

    /// <inheritdoc/>
    public override void Bind(DNNNodeCollection objNodes)
    {
        this.Bind(objNodes, true);
    }

    /// <summary>Binds the menu nodes.</summary>
    /// <param name="objNodes">The collection of menu nodes.</param>
    /// <param name="localise">A value indicating whether the menu should be localized.</param>
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
