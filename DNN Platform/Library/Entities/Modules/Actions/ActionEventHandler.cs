// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Modules.Actions
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>The ActionEventHandler delegate defines a custom event handler for an Action Event.</summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    [SuppressMessage("Microsoft.Design", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Breaking change")]
    public delegate void ActionEventHandler(object sender, ActionEventArgs e);
}
