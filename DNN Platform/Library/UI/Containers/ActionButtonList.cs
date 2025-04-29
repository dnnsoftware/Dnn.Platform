// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Containers;

using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.UI.Modules;

/// <summary>ActionButtonList provides a list of buttons for a group of actions of the same type.</summary>
/// <remarks>
/// ActionButtonList inherits from CompositeControl, and implements the IActionControl
/// Interface.  It uses a single ActionCommandButton for each Action.
/// </remarks>
public class ActionButtonList : CompositeControl, IActionControl
{
    private ActionManager actionManager;
    private ModuleActionCollection moduleActions;
    private string buttonSeparator = "&nbsp;&nbsp;";
    private string commandName = string.Empty;
    private bool displayLink = true;

    /// <inheritdoc/>
    public event ActionEventHandler Action;

    /// <summary>Gets the ActionManager instance for this Action control.</summary>
    /// <returns>An ActionManager object.</returns>
    public ActionManager ActionManager
    {
        get
        {
            if (this.actionManager == null)
            {
                this.actionManager = new ActionManager(this);
            }

            return this.actionManager;
        }
    }

    /// <summary>Gets or sets the Separator between Buttons.</summary>
    /// <remarks>Defaults to 2 non-breaking spaces.</remarks>
    /// <value>A String.</value>
    public string ButtonSeparator
    {
        get
        {
            return this.buttonSeparator;
        }

        set
        {
            this.buttonSeparator = value;
        }
    }

    /// <summary>Gets or sets the Command Name.</summary>
    /// <remarks>Maps to ModuleActionType in DotNetNuke.Entities.Modules.Actions.</remarks>
    /// <value>A String.</value>
    public string CommandName
    {
        get
        {
            return this.commandName;
        }

        set
        {
            this.commandName = value;
        }
    }

    /// <summary>Gets or sets a value indicating whether gets or sets whether the icon is displayed.</summary>
    /// <remarks>Defaults to False.</remarks>
    /// <value>A Boolean.</value>
    public bool DisplayIcon { get; set; }

    /// <summary>Gets or sets a value indicating whether gets or sets whether the link is displayed.</summary>
    /// <remarks>Defaults to True.</remarks>
    /// <value>A Boolean.</value>
    public bool DisplayLink
    {
        get
        {
            return this.displayLink;
        }

        set
        {
            this.displayLink = value;
        }
    }

    /// <summary>Gets or sets the Icon used.</summary>
    /// <remarks>Defaults to the icon defined in Action.</remarks>
    /// <value>A String.</value>
    public string ImageURL { get; set; }

    /// <summary>Gets or sets the ModuleControl instance for this Action control.</summary>
    /// <returns>An IModuleControl object.</returns>
    public IModuleControl ModuleControl { get; set; }

    /// <summary>Gets the ModuleActionCollection to bind to the list.</summary>
    /// <value>A ModuleActionCollection.</value>
    protected ModuleActionCollection ModuleActions
    {
        get
        {
            if (this.moduleActions == null)
            {
                this.moduleActions = this.ModuleControl.ModuleContext.Actions.GetActionsByCommandName(this.CommandName);
            }

            return this.moduleActions;
        }
    }

    /// <summary>OnAction raises the Action Event.</summary>
    protected virtual void OnAction(ActionEventArgs e)
    {
        if (this.Action != null)
        {
            this.Action(this, e);
        }
    }

    /// <summary>OnLoad runs when the control is loaded into the Control Tree.</summary>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (this.ModuleControl == null)
        {
            return;
        }

        foreach (ModuleAction action in this.ModuleActions)
        {
            if (action != null && this.ActionManager.IsVisible(action))
            {
                // Create a new ActionCommandButton
                var actionButton = new ActionCommandButton();

                // Set all the properties
                actionButton.ModuleAction = action;
                actionButton.ModuleControl = this.ModuleControl;
                actionButton.CommandName = this.CommandName;
                actionButton.CssClass = this.CssClass;
                actionButton.DisplayLink = this.DisplayLink;
                actionButton.DisplayIcon = this.DisplayIcon;
                actionButton.ImageUrl = this.ImageURL;

                // Add a handler for the Action Event
                actionButton.Action += this.ActionButtonClick;

                this.Controls.Add(actionButton);

                this.Controls.Add(new LiteralControl(this.ButtonSeparator));
            }
        }

        this.Visible = this.Controls.Count > 0;
    }

    /// <summary>ActionButtonClick handles the Action event of the contained ActionCommandButton(s).</summary>
    private void ActionButtonClick(object sender, ActionEventArgs e)
    {
        this.OnAction(e);
    }
}
