// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules.Actions
{
    using DotNetNuke.Collections;
    using DotNetNuke.Security;

    /// <summary>Represents a collection of <see cref="ModuleAction" /> objects.</summary>
    /// <remarks>The ModuleActionCollection is a custom collection of ModuleActions.
    /// Each ModuleAction in the collection has its own <see cref="DotNetNuke.ModuleAction.Actions" />
    ///  collection which provides the ability to create a hierarchy of ModuleActions.</remarks>
    public class ModuleActionCollection : GenericCollectionBase<ModuleAction>
    {
        /// <summary>Initializes a new instance of the <see cref="ModuleActionCollection"/> class.</summary>
        /// <remarks>The default constructor creates an empty collection of <see cref="ModuleAction" /> objects.</remarks>
        public ModuleActionCollection()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ModuleActionCollection"/> class containing the elements of the specified source collection.</summary>
        /// <param name="value">A <see cref="ModuleActionCollection" /> with which to initialize the collection.</param>
        /// <remarks>This overloaded constructor copies the <see cref="ModuleAction" />s from the indicated collection.</remarks>
        public ModuleActionCollection(ModuleActionCollection value)
        {
            this.AddRange(value);
        }

        /// <summary>Initializes a new instance of the <see cref="ModuleActionCollection"/> class containing the specified array of <see cref="ModuleAction" /> objects.</summary>
        /// <param name="value">An array of <see cref="ModuleAction" /> objects with which to initialize the collection. </param>
        /// <remarks>This overloaded constructor copies the <see cref="ModuleAction" />s from the indicated array.</remarks>
        public ModuleActionCollection(ModuleAction[] value)
        {
            this.AddRange(value);
        }

        /// <summary>Add an element of the specified <see cref="ModuleAction" /> to the end of the collection.</summary>
        /// <param name="iD">This is the identifier to use for this action.</param>
        /// <param name="title">This is the title that will be displayed for this action.</param>
        /// <param name="cmdName">The command name passed to the client when this action is clicked.</param>
        /// <returns>The index of the newly added <see cref="ModuleAction" />.</returns>
        /// <remarks>This method creates a new <see cref="ModuleAction" /> with the specified values, adds it to the collection and returns the index of the newly created ModuleAction.</remarks>
        public ModuleAction Add(int iD, string title, string cmdName)
        {
            return this.Add(iD, title, cmdName, string.Empty, string.Empty, string.Empty, false, SecurityAccessLevel.Anonymous, true, false);
        }

        /// <summary>Add an element of the specified <see cref="ModuleAction" /> to the end of the collection.</summary>
        /// <param name="iD">This is the identifier to use for this action.</param>
        /// <param name="title">This is the title that will be displayed for this action.</param>
        /// <param name="cmdName">The command name passed to the client when this action is clicked.</param>
        /// <param name="cmdArg">The command argument passed to the client when this action is clicked.</param>
        /// <param name="icon">The URL of the Icon to place next to this action.</param>
        /// <param name="url">The destination URL to redirect the client browser when this action is clicked.</param>
        /// <param name="useActionEvent">Determines whether client will receive an event notification.</param>
        /// <param name="secure">The security access level required for access to this action.</param>
        /// <param name="visible">Whether this action will be displayed.</param>
        /// <param name="newWindow">Whether open in new window.</param>
        /// <returns>The index of the newly added <see cref="ModuleAction" />.</returns>
        /// <remarks>This method creates a new <see cref="ModuleAction" /> with the specified values, adds it to the collection and returns the index of the newly created ModuleAction.</remarks>
        public ModuleAction Add(int iD, string title, string cmdName, string cmdArg, string icon, string url, bool useActionEvent, SecurityAccessLevel secure, bool visible, bool newWindow)
        {
            return this.Add(iD, title, cmdName, cmdArg, icon, url, string.Empty, useActionEvent, secure, visible, newWindow);
        }

        /// <summary>Add an element of the specified <see cref="ModuleAction" /> to the end of the collection.</summary>
        /// <param name="iD">This is the identifier to use for this action.</param>
        /// <param name="title">This is the title that will be displayed for this action.</param>
        /// <param name="cmdName">The command name passed to the client when this action is clicked.</param>
        /// <param name="cmdArg">The command argument passed to the client when this action is clicked.</param>
        /// <param name="icon">The URL of the Icon to place next to this action.</param>
        /// <param name="url">The destination URL to redirect the client browser when this action is clicked.</param>
        /// <param name="clientScript">Client side script to be run when this action is clicked.</param>
        /// <param name="useActionEvent">Determines whether client will receive an event notification.</param>
        /// <param name="secure">The security access level required for access to this action.</param>
        /// <param name="visible">Whether this action will be displayed.</param>
        /// <param name="newWindow">Whether open in new window.</param>
        /// <returns>The index of the newly added <see cref="ModuleAction" />.</returns>
        /// <remarks>This method creates a new <see cref="ModuleAction" /> with the specified values, adds it to the collection and returns the index of the newly created ModuleAction.</remarks>
        public ModuleAction Add(int iD, string title, string cmdName, string cmdArg, string icon, string url, string clientScript, bool useActionEvent, SecurityAccessLevel secure, bool visible, bool newWindow)
        {
            var modAction = new ModuleAction(iD, title, cmdName, cmdArg, icon, url, clientScript, useActionEvent, secure, visible, newWindow);
            this.Add(modAction);
            return modAction;
        }

        /// <summary>Copies the elements of the specified <see cref="ModuleAction" /> array to the end of the collection.</summary>
        /// <param name="value">An array of type <see cref="ModuleAction" /> containing the objects to add to the collection.</param>
        public void AddRange(ModuleAction[] value)
        {
            int i;
            for (i = 0; i <= value.Length - 1; i++)
            {
                this.Add(value[i]);
            }
        }

        /// <summary>Adds the contents of another <see cref="ModuleActionCollection" /> to the end of the collection.</summary>
        /// <param name="value">A <see cref="ModuleActionCollection" /> containing the objects to add to the collection. </param>
        public void AddRange(ModuleActionCollection value)
        {
            foreach (ModuleAction mA in value)
            {
                this.Add(mA);
            }
        }

        public ModuleAction GetActionByCommandName(string name)
        {
            ModuleAction retAction = null;

            // Check each action in the List
            foreach (ModuleAction modAction in this.List)
            {
                if (modAction.CommandName == name)
                {
                    retAction = modAction;
                    break;
                }

                // If action has children check them
                if (modAction.HasChildren())
                {
                    ModuleAction childAction = modAction.Actions.GetActionByCommandName(name);
                    if (childAction != null)
                    {
                        retAction = childAction;
                        break;
                    }
                }
            }

            return retAction;
        }

        public ModuleActionCollection GetActionsByCommandName(string name)
        {
            var retActions = new ModuleActionCollection();

            // Check each action in the List
            foreach (ModuleAction modAction in this.List)
            {
                if (modAction.CommandName == name)
                {
                    retActions.Add(modAction);
                }

                // If action has children check them
                if (modAction.HasChildren())
                {
                    retActions.AddRange(modAction.Actions.GetActionsByCommandName(name));
                }
            }

            return retActions;
        }

        public ModuleAction GetActionByID(int id)
        {
            ModuleAction retAction = null;

            // Check each action in the List
            foreach (ModuleAction modAction in this.List)
            {
                if (modAction.ID == id)
                {
                    retAction = modAction;
                    break;
                }

                // If action has children check them
                if (modAction.HasChildren())
                {
                    ModuleAction childAction = modAction.Actions.GetActionByID(id);
                    if (childAction != null)
                    {
                        retAction = childAction;
                        break;
                    }
                }
            }

            return retAction;
        }
    }
}
