// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules.Actions;

using System.Collections;

using DotNetNuke.Security;

/// Project     : DotNetNuke
/// Class       : ModuleActionCollection
/// <summary>Represents a collection of <see cref="T:DotNetNuke.Entities.Modules.Actions.ModuleAction" /> objects.</summary>
/// <remarks>The ModuleActionCollection is a custom collection of ModuleActions.
/// Each ModuleAction in the collection has it's own <see cref="P:DotNetNuke.ModuleAction.Actions" />
///  collection which provides the ability to create a hierarchy of ModuleActions.</remarks>
public class ModuleActionCollection : CollectionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModuleActionCollection"/> class.
    /// Initializes a new, empty instance of the <see cref="T:DotNetNuke.Entities.Modules.Actions.ModuleActionCollection" /> class.
    /// </summary>
    /// <remarks>The default constructor creates an empty collection of <see cref="T:DotNetNuke.Entities.Modules.Actions.ModuleAction" />
    ///  objects.</remarks>
    public ModuleActionCollection()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModuleActionCollection"/>
    ///  class containing the elements of the specified source collection.
    /// </summary>
    /// <param name="value">A <see cref="T:DotNetNuke.Entities.Modules.Actions.ModuleActionCollection" /> with which to initialize the collection.</param>
    /// <remarks>This overloaded constructor copies the <see cref="T:DotNetNuke.Entities.Modules.Actions.ModuleAction" />s
    ///  from the indicated collection.</remarks>
    public ModuleActionCollection(ModuleActionCollection value)
    {
        this.AddRange(value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModuleActionCollection"/>
    ///  class containing the specified array of <see cref="T:DotNetNuke.Entities.Modules.Actions.ModuleAction" /> objects.
    /// </summary>
    /// <param name="value">An array of <see cref="T:DotNetNuke.Entities.Modules.Actions.ModuleAction" /> objects
    /// with which to initialize the collection. </param>
    /// <remarks>This overloaded constructor copies the <see cref="T:DotNetNuke.Entities.Modules.Actions.ModuleAction" />s
    ///  from the indicated array.</remarks>
    public ModuleActionCollection(ModuleAction[] value)
    {
        this.AddRange(value);
    }

    /// <summary>
    /// Gets or sets the <see cref="T:DotNetNuke.Entities.Modules.Actions.ModuleActionCollection" /> at the
    /// specified index in the collection.
    /// <para>
    /// In VB.Net, this property is the indexer for the <see cref="T:DotNetNuke.Entities.Modules.Actions.ModuleActionCollection" /> class.
    /// </para>
    /// </summary>
    /// <param name="index">The index of the collection to access.</param>
    /// <value>A <see cref="T:DotNetNuke.Entities.Modules.Actions.ModuleAction" /> at each valid index.</value>
    /// <remarks>This method is an indexer that can be used to access the collection.</remarks>
    public ModuleAction this[int index]
    {
        get
        {
            return (ModuleAction)this.List[index];
        }

        set
        {
            this.List[index] = value;
        }
    }

    /// <summary>Add an element of the specified <see cref="T:DotNetNuke.Entities.Modules.Actions.ModuleAction" /> to the end of the collection.</summary>
    /// <param name="value">An object of type <see cref="T:DotNetNuke.Entities.Modules.Actions.ModuleAction" /> to add to the collection.</param>
    /// <returns>The index of the newly added <see cref="T:DotNetNuke.Entities.Modules.Actions.ModuleAction" />.</returns>
    public int Add(ModuleAction value)
    {
        return this.List.Add(value);
    }

    /// <summary>Add an element of the specified <see cref="T:DotNetNuke.Entities.Modules.Actions.ModuleAction" /> to the end of the collection.</summary>
    /// <param name="iD">This is the identifier to use for this action.</param>
    /// <param name="title">This is the title that will be displayed for this action.</param>
    /// <param name="cmdName">The command name passed to the client when this action is
    /// clicked.</param>
    /// <returns>The index of the newly added <see cref="T:DotNetNuke.Entities.Modules.Actions.ModuleAction" />.</returns>
    /// <remarks>This method creates a new <see cref="T:DotNetNuke.Entities.Modules.Actions.ModuleAction" /> with the specified
    /// values, adds it to the collection and returns the index of the newly created ModuleAction.</remarks>
    public ModuleAction Add(int iD, string title, string cmdName)
    {
        return this.Add(iD, title, cmdName, string.Empty, string.Empty, string.Empty, false, SecurityAccessLevel.Anonymous, true, false);
    }

    /// <summary>Add an element of the specified <see cref="T:DotNetNuke.Entities.Modules.Actions.ModuleAction" /> to the end of the collection.</summary>
    /// <param name="iD">This is the identifier to use for this action.</param>
    /// <param name="title">This is the title that will be displayed for this action.</param>
    /// <param name="cmdName">The command name passed to the client when this action is
    /// clicked.</param>
    /// <param name="cmdArg">The command argument passed to the client when this action is
    /// clicked.</param>
    /// <param name="icon">The URL of the Icon to place next to this action.</param>
    /// <param name="url">The destination URL to redirect the client browser when this
    /// action is clicked.</param>
    /// <param name="useActionEvent">Determines whether client will receive an event
    /// notification.</param>
    /// <param name="secure">The security access level required for access to this action.</param>
    /// <param name="visible">Whether this action will be displayed.</param>
    /// <param name="newWindow">Whether open in new window.</param>
    /// <returns>The index of the newly added <see cref="T:DotNetNuke.Entities.Modules.Actions.ModuleAction" />.</returns>
    /// <remarks>This method creates a new <see cref="T:DotNetNuke.Entities.Modules.Actions.ModuleAction" /> with the specified
    /// values, adds it to the collection and returns the index of the newly created ModuleAction.</remarks>
    public ModuleAction Add(int iD, string title, string cmdName, string cmdArg, string icon, string url, bool useActionEvent, SecurityAccessLevel secure, bool visible, bool newWindow)
    {
        return this.Add(iD, title, cmdName, cmdArg, icon, url, string.Empty, useActionEvent, secure, visible, newWindow);
    }

    /// <summary>Add an element of the specified <see cref="T:DotNetNuke.Entities.Modules.Actions.ModuleAction" /> to the end of the collection.</summary>
    /// <param name="iD">This is the identifier to use for this action.</param>
    /// <param name="title">This is the title that will be displayed for this action.</param>
    /// <param name="cmdName">The command name passed to the client when this action is
    /// clicked.</param>
    /// <param name="cmdArg">The command argument passed to the client when this action is
    /// clicked.</param>
    /// <param name="icon">The URL of the Icon to place next to this action.</param>
    /// <param name="url">The destination URL to redirect the client browser when this
    /// action is clicked.</param>
    /// <param name="clientScript">Client side script to be run when the this action is
    /// clicked.</param>
    /// <param name="useActionEvent">Determines whether client will receive an event
    /// notification.</param>
    /// <param name="secure">The security access level required for access to this action.</param>
    /// <param name="visible">Whether this action will be displayed.</param>
    /// <param name="newWindow">Whether open in new window.</param>
    /// <returns>The index of the newly added <see cref="T:DotNetNuke.Entities.Modules.Actions.ModuleAction" />.</returns>
    /// <remarks>This method creates a new <see cref="T:DotNetNuke.Entities.Modules.Actions.ModuleAction" /> with the specified
    /// values, adds it to the collection and returns the index of the newly created ModuleAction.</remarks>
    ///
    public ModuleAction Add(int iD, string title, string cmdName, string cmdArg, string icon, string url, string clientScript, bool useActionEvent, SecurityAccessLevel secure, bool visible, bool newWindow)
    {
        var modAction = new ModuleAction(iD, title, cmdName, cmdArg, icon, url, clientScript, useActionEvent, secure, visible, newWindow);
        this.Add(modAction);
        return modAction;
    }

    /// <summary>
    /// Copies the elements of the specified <see cref="T:DotNetNuke.Entities.Modules.Actions.ModuleAction" />
    ///  array to the end of the collection.
    /// </summary>
    /// <param name="value">An array of type <see cref="T:DotNetNuke.Entities.Modules.Actions.ModuleAction" />
    ///  containing the objects to add to the collection.</param>
    public void AddRange(ModuleAction[] value)
    {
        int i;
        for (i = 0; i <= value.Length - 1; i++)
        {
            this.Add(value[i]);
        }
    }

    /// <summary>
    /// Adds the contents of another <see cref="T:DotNetNuke.Entities.Modules.Actions.ModuleActionCollection" />
    ///  to the end of the collection.
    /// </summary>
    /// <param name="value">A <see cref="T:DotNetNuke.Entities.Modules.Actions.ModuleActionCollection" /> containing
    /// the objects to add to the collection. </param>
    public void AddRange(ModuleActionCollection value)
    {
        foreach (ModuleAction mA in value)
        {
            this.Add(mA);
        }
    }

    /// <summary>Gets a value indicating whether the collection contains the specified <see cref="T:DotNetNuke.Entities.Modules.Actions.ModuleAction" />.</summary>
    /// <param name="value">The <see cref="T:DotNetNuke.Entities.Modules.Actions.ModuleAction" /> to search for in the collection.</param>
    /// <returns><b>true</b> if the collection contains the specified object; otherwise, <b>false</b>.</returns>
    /// <example>
    /// <code>
    /// ' Tests for the presence of a ModuleAction in the
    /// ' collection, and retrieves its index if it is found.
    /// Dim testModuleAction = New ModuleAction(5, "Edit Action", "Edit")
    /// Dim itemIndex As Integer = -1
    /// If collection.Contains(testModuleAction) Then
    ///    itemIndex = collection.IndexOf(testModuleAction)
    /// End If
    /// </code>
    /// </example>
    public bool Contains(ModuleAction value)
    {
        // If value is not of type ModuleAction, this will return false.
        return this.List.Contains(value);
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

    /// <summary>
    /// Gets the index in the collection of the specified <see cref="T:DotNetNuke.Entities.Modules.Actions.ModuleActionCollection" />,
    /// if it exists in the collection.
    /// </summary>
    /// <param name="value">The <see cref="T:DotNetNuke.Entities.Modules.Actions.ModuleAction" /> to locate in the collection.</param>
    /// <returns>The index in the collection of the specified object, if found; otherwise, -1.</returns>
    /// <example> This example tests for the presense of a ModuleAction in the
    /// collection, and retrieves its index if it is found.
    /// <code>
    ///   Dim testModuleAction = New ModuleAction(5, "Edit Action", "Edit")
    ///   Dim itemIndex As Integer = -1
    ///   If collection.Contains(testModuleAction) Then
    ///     itemIndex = collection.IndexOf(testModuleAction)
    ///   End If
    /// </code>
    /// </example>
    public int IndexOf(ModuleAction value)
    {
        return this.List.IndexOf(value);
    }

    /// <summary>
    /// Add an element of the specified <see cref="T:DotNetNuke.Entities.Modules.Actions.ModuleAction" /> to the
    /// collection at the designated index.
    /// </summary>
    /// <param name="index">An <see cref="T:system.int32">Integer</see> to indicate the location to add the object to the collection.</param>
    /// <param name="value">An object of type <see cref="T:DotNetNuke.Entities.Modules.Actions.ModuleAction" /> to add to the collection.</param>
    /// <example>
    /// <code>
    /// ' Inserts a ModuleAction at index 0 of the collection.
    /// collection.Insert(0, New ModuleAction(5, "Edit Action", "Edit"))
    /// </code>
    /// </example>
    public void Insert(int index, ModuleAction value)
    {
        this.List.Insert(index, value);
    }

    /// <summary>Remove the specified object of type <see cref="T:DotNetNuke.Entities.Modules.Actions.ModuleAction" /> from the collection.</summary>
    /// <param name="value">An object of type <see cref="T:DotNetNuke.Entities.Modules.Actions.ModuleAction" /> to remove from the collection.</param>
    /// <example>
    /// <code>
    /// ' Removes the specified ModuleAction from the collection.
    /// Dim testModuleAction = New ModuleAction(5, "Edit Action", "Edit")
    /// collection.Remove(testModuleAction)
    /// </code>
    /// </example>
    public void Remove(ModuleAction value)
    {
        this.List.Remove(value);
    }
}
