// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Entities.Portals
{
    public interface IPortalEventHandlers
    {
        void PortalCreated(object sender, PortalCreatedEventArgs args);
    }
}
