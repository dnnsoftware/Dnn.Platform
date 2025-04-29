// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.UI.Components.EventHandlers;

using System.ComponentModel.Composition;

using Dnn.PersonaBar.UI.Components.Controllers;
using DotNetNuke.Entities.Tabs.Actions;

[Export(typeof(ITabEventHandler))]
public class TabEventsHandler : ITabEventHandler
{
    /// <inheritdoc/>
    public void TabCreated(object sender, TabEventArgs args)
    {
        AdminMenuController.Instance.CreateLinkMenu(args.Tab);
    }

    /// <inheritdoc/>
    public void TabUpdated(object sender, TabEventArgs args)
    {
        AdminMenuController.Instance.CreateLinkMenu(args.Tab);
    }

    /// <inheritdoc/>
    public void TabRemoved(object sender, TabEventArgs args)
    {
    }

    /// <inheritdoc/>
    public void TabDeleted(object sender, TabEventArgs args)
    {
        AdminMenuController.Instance.DeleteLinkMenu(args.Tab);
    }

    /// <inheritdoc/>
    public void TabRestored(object sender, TabEventArgs args)
    {
        AdminMenuController.Instance.CreateLinkMenu(args.Tab);
    }

    /// <inheritdoc/>
    public void TabMarkedAsPublished(object sender, TabEventArgs args)
    {
    }
}
