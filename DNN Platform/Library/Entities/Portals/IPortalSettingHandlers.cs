// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Entities.Portals
{
    public interface IPortalSettingHandlers
    {
        void PortalSettingUpdated(object sender, PortalSettingUpdatedEventArgs args);
    }
}
