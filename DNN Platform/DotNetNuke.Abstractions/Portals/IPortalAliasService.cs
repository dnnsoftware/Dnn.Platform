// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Portals
{
    using System.Collections.Generic;

    /// <summary>Portal Alias APIs for managing the different Portal Alias.</summary>
    public interface IPortalAliasService
    {
        /// <summary>Gets the portal alias by portal.</summary>
        /// <param name="portalId">The portal id.</param>
        /// <param name="portalAlias">The portal alias.</param>
        /// <returns>Portal alias.</returns>
        string GetPortalAliasByPortal(int portalId, string portalAlias);

        /// <summary>Gets the portal alias by tab.</summary>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="portalAlias">The portal alias.</param>
        /// <returns>Portal alias.</returns>
        string GetPortalAliasByTab(int tabId, string portalAlias);

        /// <summary>Validates the alias.</summary>
        /// <param name="portalAlias">The portal alias.</param>
        /// <param name="ischild">if set to <see langword="true"/>, validate the alias as a child alias (i.e. as a subdirectory of another alias).</param>
        /// <returns><see langword="true"/> if the alias is a valid url format; otherwise return <see langword="false"/>.</returns>
        bool ValidateAlias(string portalAlias, bool ischild);

        /// <summary>Adds a new Portal Alias.</summary>
        /// <param name="portalAlias">The portal alias.</param>
        /// <returns>The new portal alias ID.</returns>
        int AddPortalAlias(IPortalAliasInfo portalAlias);

        /// <summary>Deletes an existing portal alias.</summary>
        /// <param name="portalAlias">The portal alias to delete.</param>
        void DeletePortalAlias(IPortalAliasInfo portalAlias);

        /// <summary>Get a portal alias by name.</summary>
        /// <param name="alias">The name of the portal alias.</param>
        /// <returns>The portal alias.</returns>
        IPortalAliasInfo GetPortalAlias(string alias);

        /// <summary>Gets the portal alias.</summary>
        /// <param name="alias">The portal alias.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>Portal Alias Info.</returns>
        IPortalAliasInfo GetPortalAlias(string alias, int portalId);

        /// <summary>Gets the portal alias by portal alias ID.</summary>
        /// <param name="portalAliasId">The portal alias ID.</param>
        /// <returns>Portal alias info.</returns>
        IPortalAliasInfo GetPortalAliasByPortalAliasId(int portalAliasId);

        /// <summary>Get all portal aliases.</summary>
        /// <returns>A collection of portal aliases.</returns>
        IDictionary<string, IPortalAliasInfo> GetPortalAliases();

        /// <summary>Get all the portal aliases for a specific portal.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>An IEnumerable of portal aliases.</returns>
        IEnumerable<IPortalAliasInfo> GetPortalAliasesByPortalId(int portalId);

        /// <summary>Gets the portal by portal alias ID.</summary>
        /// <param name="portalAliasId">The portal alias id.</param>
        /// <returns>Portal info.</returns>
        IPortalInfo GetPortalByPortalAliasId(int portalAliasId);

        /// <summary>Update a specified portal alias.</summary>
        /// <param name="portalAlias">The portal alias to update.</param>
        void UpdatePortalAlias(IPortalAliasInfo portalAlias);
    }
}
