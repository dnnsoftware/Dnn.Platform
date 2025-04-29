// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu;

using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Web.DDRMenu.DNNCommon;

/// <summary>DDR Menu WebControl.</summary>
internal class DDRMenuControl : WebControl, IPostBackEventHandler
{
    private MenuBase menu;

    /// <summary>Handles a click on the menu.</summary>
    /// <param name="id">The id of the menu item beeing clicked.</param>
    public delegate void MenuClickEventHandler(string id);

    /// <summary>Handles a click on a node of the menu.</summary>
    public event MenuClickEventHandler NodeClick;

    /// <inheritdoc/>
    public override bool EnableViewState
    {
        get { return false; }
        set { }
    }

    /// <summary>Gets or sets the menu root node.</summary>
    internal MenuNode RootNode { get; set; }

    /// <summary>Gets or sets a value indicating whether to skip the localization.</summary>
    internal bool SkipLocalisation { get; set; }

    /// <summary>Gets or sets the menu settings.</summary>
    internal Settings MenuSettings { get; set; }

    /// <inheritdoc/>
    public void RaisePostBackEvent(string eventArgument)
    {
        using (new DNNContext(this))
        {
            if (this.NodeClick != null)
            {
                this.NodeClick(eventArgument);
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnPreRender(EventArgs e)
    {
        using (new DNNContext(this))
        {
            base.OnPreRender(e);

            this.MenuSettings.MenuStyle = this.MenuSettings.MenuStyle ?? "DNNMenu";
            this.menu = MenuBase.Instantiate(this.MenuSettings.MenuStyle);
            this.menu.RootNode = this.RootNode ?? new MenuNode();
            this.menu.SkipLocalisation = this.SkipLocalisation;
            this.menu.ApplySettings(this.MenuSettings);

            this.menu.PreRender();
        }
    }

    /// <inheritdoc/>
    protected override void Render(HtmlTextWriter htmlWriter)
    {
        using (new DNNContext(this))
        {
            this.menu.Render(htmlWriter);
        }
    }
}
