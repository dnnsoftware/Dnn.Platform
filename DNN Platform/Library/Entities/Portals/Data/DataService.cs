// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Data;

using DotNetNuke.ComponentModel;
using DotNetNuke.Data;
#endregion

namespace DotNetNuke.Entities.Portals.Data
{
    public class DataService : ComponentBase<IDataService, DataService>, IDataService
    {
        private readonly DataProvider _provider = DataProvider.Instance();

        public int AddPortalGroup(PortalGroupInfo portalGroup, int createdByUserId)
        {
            return _provider.ExecuteScalar<int>("AddPortalGroup",
                                               portalGroup.PortalGroupName,
                                               portalGroup.PortalGroupDescription,
                                               portalGroup.MasterPortalId,
                                               portalGroup.AuthenticationDomain,
                                               createdByUserId);
        }

        public void DeletePortalGroup(PortalGroupInfo portalGroup)
        {
            _provider.ExecuteNonQuery("DeletePortalGroup", portalGroup.PortalGroupId);
        }

        public IDataReader GetPortalGroups()
        {
            return _provider.ExecuteReader("GetPortalGroups");
        }

        public void UpdatePortalGroup(PortalGroupInfo portalGroup, int lastModifiedByUserId)
        {
            _provider.ExecuteNonQuery("UpdatePortalGroup",
                                            portalGroup.PortalGroupId,
                                            portalGroup.PortalGroupName,
                                            portalGroup.PortalGroupDescription,
                                            portalGroup.AuthenticationDomain,
                                            lastModifiedByUserId);
        }

        public IDataReader GetSharedModulesWithPortal(PortalInfo portal)
        {
            return _provider.ExecuteReader("GetSharedModulesWithPortal", portal.PortalID);
        }

        public IDataReader GetSharedModulesByPortal(PortalInfo portal)
        {
            return _provider.ExecuteReader("GetSharedModulesByPortal", portal.PortalID);
        }
    }
}
