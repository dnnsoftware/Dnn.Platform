// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Entities.Tabs.Actions
{
    public interface ITabEventHandler
    {
        void TabCreated(object sender, TabEventArgs args);
        void TabUpdated(object sender, TabEventArgs args);
        void TabRemoved(object sender, TabEventArgs args);
        void TabDeleted(object sender, TabEventArgs args);
        void TabRestored(object sender, TabEventArgs args);
        void TabMarkedAsPublished(object sender, TabEventArgs args);
    }
}
