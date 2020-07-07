// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules.Actions
{
    using System;

    using DotNetNuke.Security;

    /// -----------------------------------------------------------------------------
    /// Project     : DotNetNuke
    /// Class       : ModuleAction
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// Each Module Action represents a separate functional action as defined by the
    /// associated module.
    /// </summary>
    /// <remarks>A module action is used to define a specific function for a given module.
    /// Each module can define one or more actions which the portal will present to the
    /// user.  These actions may be presented as a menu, a dropdown list or even a group
    /// of linkbuttons.
    /// <seealso cref="T:DotNetNuke.ModuleActionCollection" /></remarks>
    /// -----------------------------------------------------------------------------
    public class ModuleAction
    {
        public ModuleAction(int id)
            : this(id, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, false, SecurityAccessLevel.Anonymous, true, false)
        {
        }

        public ModuleAction(int id, string title, string cmdName)
            : this(id, title, cmdName, string.Empty, string.Empty, string.Empty, string.Empty, false, SecurityAccessLevel.Anonymous, true, false)
        {
        }

        public ModuleAction(int id, string title, string cmdName, string cmdArg)
            : this(id, title, cmdName, cmdArg, string.Empty, string.Empty, string.Empty, false, SecurityAccessLevel.Anonymous, true, false)
        {
        }

        public ModuleAction(int id, string title, string cmdName, string cmdArg, string icon)
            : this(id, title, cmdName, cmdArg, icon, string.Empty, string.Empty, false, SecurityAccessLevel.Anonymous, true, false)
        {
        }

        public ModuleAction(int id, string title, string cmdName, string cmdArg, string icon, string url)
            : this(id, title, cmdName, cmdArg, icon, url, string.Empty, false, SecurityAccessLevel.Anonymous, true, false)
        {
        }

        public ModuleAction(int id, string title, string cmdName, string cmdArg, string icon, string url, string clientScript)
            : this(id, title, cmdName, cmdArg, icon, url, clientScript, false, SecurityAccessLevel.Anonymous, true, false)
        {
        }

        public ModuleAction(int id, string title, string cmdName, string cmdArg, string icon, string url, string clientScript, bool useActionEvent)
            : this(id, title, cmdName, cmdArg, icon, url, clientScript, useActionEvent, SecurityAccessLevel.Anonymous, true, false)
        {
        }

        public ModuleAction(int id, string title, string cmdName, string cmdArg, string icon, string url, string clientScript, bool useActionEvent, SecurityAccessLevel secure)
            : this(id, title, cmdName, cmdArg, icon, url, clientScript, useActionEvent, secure, true, false)
        {
        }

        public ModuleAction(int id, string title, string cmdName, string cmdArg, string icon, string url, string clientScript, bool useActionEvent, SecurityAccessLevel secure, bool visible)
            : this(id, title, cmdName, cmdArg, icon, url, clientScript, useActionEvent, secure, visible, false)
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleAction"/> class
        /// using the specified parameters.
        /// </summary>
        /// <param name="id">This is the identifier to use for this action.</param>
        /// <param name="title">This is the title that will be displayed for this action.</param>
        /// <param name="cmdName">The command name passed to the client when this action is
        /// clicked.</param>
        /// <param name="cmdArg">The command argument passed to the client when this action is
        /// clicked.</param>
        /// <param name="icon">The URL of the Icon to place next to this action.</param>
        /// <param name="url">The destination URL to redirect the client browser when this action is clicked.</param>
        /// <param name="clientScript"></param>
        /// <param name="useActionEvent">Determines whether client will receive an event notification.</param>
        /// <param name="secure">The security access level required for access to this action.</param>
        /// <param name="visible">Whether this action will be displayed.</param>
        /// <param name="newWindow"></param>
        /// <remarks>The moduleaction constructor is used to set the various properties of
        /// the <see cref="T:DotNetNuke.Entities.Modules.Actions.ModuleAction" /> class at the time the instance is created.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public ModuleAction(int id, string title, string cmdName, string cmdArg, string icon, string url, string clientScript, bool useActionEvent, SecurityAccessLevel secure, bool visible,
                            bool newWindow)
        {
            this.ID = id;
            this.Title = title;
            this.CommandName = cmdName;
            this.CommandArgument = cmdArg;
            this.Icon = icon;
            this.Url = url;
            this.ClientScript = clientScript;
            this.UseActionEvent = useActionEvent;
            this.Secure = secure;
            this.Visible = visible;
            this.NewWindow = newWindow;
            this.Actions = new ModuleActionCollection();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Actions property allows the user to create a hierarchy of actions, with
        /// each action having sub-actions.
        /// </summary>
        /// <value>Returns a collection of ModuleActions.</value>
        /// <remarks>Each action may contain one or more child actions.  When displayed via
        /// the <see cref="T:DotNetNuke.Containers.Actions"/> control, these subactions are
        /// shown as sub-menus.  If other Action controls are implemented, then
        /// sub-actions may or may not be supported for that control type.</remarks>
        /// -----------------------------------------------------------------------------
        public ModuleActionCollection Actions { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a Module Action ID is a identifier that can be used in a Module Action Collection
        /// to find a specific Action.
        /// </summary>
        /// <value>The integer ID of the current <see cref="T:DotNetNuke.ModuleAction"/>.</value>
        /// <remarks>When building a hierarchy of <see cref="T:DotNetNuke.ModuleAction">ModuleActions</see>,
        /// the ID is used to link the child and parent actions.</remarks>
        /// -----------------------------------------------------------------------------
        public int ID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets or sets whether the current action should be displayed.
        /// </summary>
        /// <value>A boolean value that determines if the current action should be displayed.</value>
        /// <remarks>If Visible is false, then the action is always hidden.  If Visible
        /// is true then the action may be visible depending on the security access rights
        /// specified by the <see cref="P:DotNetNuke.ModuleAction.Secure"/> property.  By
        /// utilizing a custom method in your module, you can encapsulate specific business
        /// rules to determine if the Action should be visible.</remarks>
        /// -----------------------------------------------------------------------------
        public bool Visible { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the value indicating the <see cref="T:DotnetNuke.SecurityAccessLevel" /> that is required
        /// to access this <see cref="T:DotNetNuke.ModuleAction" />.
        /// </summary>
        /// <value>The value indicating the <see cref="T:DotnetNuke.SecurityAccessLevel" /> that is required
        /// to access this <see cref="T:DotNetNuke.ModuleAction" />.</value>
        /// <remarks>The security access level determines the roles required by the current user in
        /// order to access this module action.</remarks>
        /// -----------------------------------------------------------------------------
        public SecurityAccessLevel Secure { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a Module Action CommandName represents a string used by the ModuleTitle to notify
        /// the parent module that a given Module Action was selected in the Module Menu.
        /// </summary>
        /// <value>The name of the command to perform.</value>
        /// <remarks>
        /// Use the CommandName property to determine the command to perform. The CommandName
        /// property can contain any string set by the programmer. The programmer can then
        /// identify the command name in code and perform the appropriate tasks.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public string CommandName { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a Module Action CommandArgument provides additional information and
        /// complements the CommandName.
        /// </summary>
        /// <value>A string that contains the argument for the command.</value>
        /// <remarks>
        /// The CommandArgument can contain any string set by the programmer. The
        /// CommandArgument property complements the <see cref="P:DotNetNuke.ModuleAction.CommandName" />
        ///  property by allowing you to provide any additional information for the command.
        /// For example, you can set the CommandName property to "Sort" and set the
        /// CommandArgument property to "Ascending" to specify a command to sort in ascending
        /// order.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public string CommandArgument { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the string that is displayed in the Module Menu
        /// that represents a given menu action.
        /// </summary>
        /// <value>The string value that is displayed to represent the module action.</value>
        /// <remarks>The title property is displayed by the Actions control for each module
        /// action.</remarks>
        /// -----------------------------------------------------------------------------
        public string Title { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the URL for the icon file that is displayed for the given
        /// <see cref="T:DotNetNuke.ModuleAction" />.
        /// </summary>
        /// <value>The URL for the icon that is displayed with the module action.</value>
        /// <remarks>The URL for the icon is a simple string and is not checked for formatting.</remarks>
        /// -----------------------------------------------------------------------------
        public string Icon { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the URL to which the user is redirected when the
        /// associated Module Menu Action is selected.
        /// </summary>
        /// <value>The URL to which the user is redirected when the
        /// associated Module Menu Action is selected.</value>
        /// <remarks>If the URL is present then the Module Action Event is not fired.
        /// If the URL is empty then the Action Event is fired and is passed the value
        /// of the associated Command property.</remarks>
        /// -----------------------------------------------------------------------------
        public string Url { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets javascript which will be run in the clients browser
        /// when the associated Module menu Action is selected. prior to a postback.
        /// </summary>
        /// <value>The Javascript which will be run during the menuClick event.</value>
        /// <remarks>If the ClientScript property is present then it is called prior
        /// to the postback occuring. If the ClientScript returns false then the postback
        /// is canceled.  If the ClientScript is empty then the Action Event is fired and
        /// is passed the value of the associated Command property.</remarks>
        /// -----------------------------------------------------------------------------
        public string ClientScript { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets a value that determines if a local ActionEvent is fired when the
        /// <see cref="T:DotNetNuke.ModuleAction" /> contains a URL.
        /// </summary>
        /// <value>A boolean indicating whether to fire the ActionEvent.</value>
        /// <remarks>When a MenuAction is clicked, an event is fired within the Actions
        /// control.  If the UseActionEvent is true then the Actions control will forward
        /// the event to the parent skin which will then attempt to raise the event to
        /// the appropriate module.  If the UseActionEvent is false, and the URL property
        /// is set, then the Actions control will redirect the response to the URL.  In
        /// all cases, an ActionEvent is raised if the URL is not set.</remarks>
        /// -----------------------------------------------------------------------------
        public bool UseActionEvent { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets or sets a value that determines if a new window is opened when the
        /// DoAction() method is called.
        /// </summary>
        /// <value>A boolean indicating whether to open a new window.</value>
        /// <remarks></remarks>
        /// -----------------------------------------------------------------------------
        public bool NewWindow { get; set; }

        internal string ControlKey
        {
            get
            {
                string controlKey = string.Empty;
                if (!string.IsNullOrEmpty(this.Url))
                {
                    int startIndex = this.Url.IndexOf("/ctl/");
                    int endIndex = -1;
                    if (startIndex > -1)
                    {
                        startIndex += 4;
                        endIndex = this.Url.IndexOf("/", startIndex + 1);
                    }
                    else
                    {
                        startIndex = this.Url.IndexOf("ctl=");
                        if (startIndex > -1)
                        {
                            startIndex += 4;
                            endIndex = this.Url.IndexOf("&", startIndex + 1);
                        }
                    }

                    if (startIndex > -1)
                    {
                        controlKey = endIndex > -1 ? this.Url.Substring(startIndex + 1, endIndex - startIndex - 1) : this.Url.Substring(startIndex + 1);
                    }
                }

                return controlKey;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Determines whether the action node contains any child actions.
        /// </summary>
        /// <returns>True if child actions exist, false if child actions do not exist.</returns>
        /// <remarks>Each action may contain one or more child actions in the
        /// <see cref="P:DotNetNuke.ModuleAction.Actions"/> property.  When displayed via
        /// the <see cref="T:DotNetNuke.Containers.Actions"/> control, these subactions are
        /// shown as sub-menus.</remarks>
        /// -----------------------------------------------------------------------------
        public bool HasChildren()
        {
            return this.Actions.Count > 0;
        }
    }
}
