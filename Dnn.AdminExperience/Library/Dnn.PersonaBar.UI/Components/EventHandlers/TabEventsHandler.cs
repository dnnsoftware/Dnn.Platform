﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.ComponentModel.Composition;
using Dnn.PersonaBar.UI.Components.Controllers;
using DotNetNuke.Entities.Tabs.Actions;

namespace Dnn.PersonaBar.UI.Components.EventHandlers
{
    [Export(typeof(ITabEventHandler))]
    public class TabEventsHandler : ITabEventHandler
    {
        public void TabCreated(object sender, TabEventArgs args)
        {
            AdminMenuController.Instance.CreateLinkMenu(args.Tab);
        }

        public void TabUpdated(object sender, TabEventArgs args)
        {
            AdminMenuController.Instance.CreateLinkMenu(args.Tab);
        }

        public void TabRemoved(object sender, TabEventArgs args)
        {
            
        }

        public void TabDeleted(object sender, TabEventArgs args)
        {
            AdminMenuController.Instance.DeleteLinkMenu(args.Tab);
        }

        public void TabRestored(object sender, TabEventArgs args)
        {
            AdminMenuController.Instance.CreateLinkMenu(args.Tab);
        }

        public void TabMarkedAsPublished(object sender, TabEventArgs args)
        {
        }
    }
}
