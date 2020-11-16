// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Modules.Actions
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Modules.Actions
    /// Class:      ActionEventHandler
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ActionEventHandler delegate defines a custom event handler for an Action
    /// Event.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public delegate void ActionEventHandler(object sender, ActionEventArgs e);
}
