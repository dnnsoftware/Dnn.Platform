// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Portals.Data
{
    using System.Data;

    public interface IDataService
    {
        int AddPortalGroup(PortalGroupInfo portalGroup, int createdByUserId);

        void DeletePortalGroup(PortalGroupInfo portalGroup);

        IDataReader GetPortalGroups();

        void UpdatePortalGroup(PortalGroupInfo portalGroup, int lastModifiedByUserId);

        /// <summary>
        /// Gets all shared modules with the specified Portal  by another owner portals.
        /// </summary>
        /// <param name="portal">The Portal.</param>
        /// <returns>A list of ModuleInfo objects.</returns>
        IDataReader GetSharedModulesWithPortal(PortalInfo portal);

        /// <summary>
        /// Gets all shared modules by the specified Portal with another portals.
        /// </summary>
        /// <param name="portal">The owner Portal.</param>
        /// <returns>A list of ModuleInfo objects.</returns>
        IDataReader GetSharedModulesByPortal(PortalInfo portal);
    }
}
