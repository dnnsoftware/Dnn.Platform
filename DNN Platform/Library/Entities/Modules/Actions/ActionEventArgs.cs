// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules.Actions;

using System;

/// Project     : DotNetNuke
/// Namespace   : DotNetNuke.Entities.Modules.Actions
/// Class       : ActionEventArgs
/// <summary>ActionEventArgs provides a custom EventARgs class for Action Events.</summary>
public class ActionEventArgs : EventArgs
{
    private readonly ModuleAction action;
    private readonly ModuleInfo moduleConfiguration;

    /// <summary>Initializes a new instance of the <see cref="ActionEventArgs"/> class.</summary>
    /// <param name="action"></param>
    /// <param name="moduleConfiguration"></param>
    public ActionEventArgs(ModuleAction action, ModuleInfo moduleConfiguration)
    {
        this.action = action;
        this.moduleConfiguration = moduleConfiguration;
    }

    public ModuleAction Action
    {
        get
        {
            return this.action;
        }
    }

    public ModuleInfo ModuleConfiguration
    {
        get
        {
            return this.moduleConfiguration;
        }
    }
}
