// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Entities.Tabs.Actions
{
    public interface ITabSyncEventHandler
    {
        void TabSerialize(object sender, TabSyncEventArgs args);
        void TabDeserialize(object sender, TabSyncEventArgs args);
    }
}
