// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Portals
{
    using System;
    using System.Collections.Generic;

    public interface IPortalGroupController
    {
        int AddPortalGroup(PortalGroupInfo portalGroup);

        void AddPortalToGroup(PortalInfo portal, PortalGroupInfo portalGroup, UserCopiedCallback callback);

        void DeletePortalGroup(PortalGroupInfo portalGroup);

        IEnumerable<PortalGroupInfo> GetPortalGroups();

        IEnumerable<PortalInfo> GetPortalsByGroup(int portalGroupId);

        void RemovePortalFromGroup(PortalInfo portal, PortalGroupInfo portalGroup, bool copyUsers, UserCopiedCallback callback);

        void UpdatePortalGroup(PortalGroupInfo portalGroup);

        bool IsModuleShared(int moduleId, PortalInfo portal);
    }
}
