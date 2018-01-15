#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
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