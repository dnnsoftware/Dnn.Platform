﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Portals.Data
{
    using System.Data;

    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;

    public class DataService : ComponentBase<IDataService, DataService>, IDataService
    {
        private readonly DataProvider _provider = DataProvider.Instance();

        /// <inheritdoc/>
        public int AddPortalGroup(PortalGroupInfo portalGroup, int createdByUserId)
        {
            return this._provider.ExecuteScalar<int>(
                "AddPortalGroup",
                portalGroup.PortalGroupName,
                portalGroup.PortalGroupDescription,
                portalGroup.MasterPortalId,
                portalGroup.AuthenticationDomain,
                createdByUserId);
        }

        /// <inheritdoc/>
        public void DeletePortalGroup(PortalGroupInfo portalGroup)
        {
            this._provider.ExecuteNonQuery("DeletePortalGroup", portalGroup.PortalGroupId);
        }

        /// <inheritdoc/>
        public IDataReader GetPortalGroups()
        {
            return this._provider.ExecuteReader("GetPortalGroups");
        }

        /// <inheritdoc/>
        public void UpdatePortalGroup(PortalGroupInfo portalGroup, int lastModifiedByUserId)
        {
            this._provider.ExecuteNonQuery(
                "UpdatePortalGroup",
                portalGroup.PortalGroupId,
                portalGroup.PortalGroupName,
                portalGroup.PortalGroupDescription,
                portalGroup.AuthenticationDomain,
                lastModifiedByUserId);
        }

        /// <inheritdoc/>
        public IDataReader GetSharedModulesWithPortal(PortalInfo portal)
        {
            return this._provider.ExecuteReader("GetSharedModulesWithPortal", portal.PortalID);
        }

        /// <inheritdoc/>
        public IDataReader GetSharedModulesByPortal(PortalInfo portal)
        {
            return this._provider.ExecuteReader("GetSharedModulesByPortal", portal.PortalID);
        }
    }
}
