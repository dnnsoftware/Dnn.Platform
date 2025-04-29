// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Containers;

using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.UI.Modules;

/// <summary>IActionControl provides a common Interface for Action Controls.</summary>
public interface IActionControl
{
    /// <summary>The eventhandler for the action.</summary>
    event ActionEventHandler Action;

    /// <summary>Gets a reference to the aciton manager.</summary>
    ActionManager ActionManager { get; }

    /// <summary>Gets or sets the module control.</summary>
    IModuleControl ModuleControl { get; set; }
}
