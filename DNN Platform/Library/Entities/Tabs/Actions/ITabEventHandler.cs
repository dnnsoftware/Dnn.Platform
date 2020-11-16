// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
