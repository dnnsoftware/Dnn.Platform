// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Portals.Internal
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    /// Do not implement.  This interface is only implemented by the DotNetNuke core framework. Outside the framework it should used as a type and for unit test purposes only.
    /// There is no guarantee that this interface will not change.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("This class has been obsoleted in 7.3.0 - please use version in DotNetNuke.Entities.Portals instead. Scheduled removal in v10.0.0.")]
    public interface IPortalAliasController
    {
        /// <summary>
        /// Add a new Portal Alias.
        /// </summary>
        /// <param name="portalAlias">The portal alias to add.</param>
        /// <returns>The Id of the newly added portal alias.</returns>
        int AddPortalAlias(PortalAliasInfo portalAlias);

        /// <summary>
        /// Delete a Portal Alias.
        /// </summary>
        /// <param name="portalAlias">The portal alias to remove.</param>
        void DeletePortalAlias(PortalAliasInfo portalAlias);

        /// <summary>
        /// Gets the portal alias info.
        /// </summary>
        /// <param name="alias">The portal alias.</param>
        /// <returns>Portal alias info.</returns>
        PortalAliasInfo GetPortalAlias(string alias);

        /// <summary>
        /// Gets the portal alias by portal ID.
        /// </summary>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>Portal alias collection.</returns>
        IEnumerable<PortalAliasInfo> GetPortalAliasesByPortalId(int portalId);

        /// <summary>
        /// Gets all the portal aliases defined.
        /// </summary>
        /// <returns>A dictionary keyed by the HTTP Alias.</returns>
        IDictionary<string, PortalAliasInfo> GetPortalAliases();

        /// <summary>
        /// Updates the portal alias info.
        /// </summary>
        /// <param name="portalAlias">The obj portal alias info.</param>
        void UpdatePortalAlias(PortalAliasInfo portalAlias);
    }
}
