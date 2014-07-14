#region Copyright
// 
// DotNetNuke� - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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

using System.Collections.Generic;

namespace DotNetNuke.Entities.Portals
{
    /// <summary>
    /// Do not implement.  This interface is only implemented by the DotNetNuke core framework. Outside the framework it should used as a type and for unit test purposes only.
    /// There is no guarantee that this interface will not change.
    /// </summary>
    public interface IPortalAliasController
    {
        /// <summary>
        /// Add a new Portal Alias
        /// </summary>
        /// <param name="portalAlias">The portal alias to add</param>
        /// <returns>The Id of the newly added portal alias</returns>
        int AddPortalAlias(PortalAliasInfo portalAlias);

        /// <summary>
        /// Delete a Portal Alias
        /// </summary>
        /// <param name="portalAlias">The portal alias to remove</param>
        void DeletePortalAlias(PortalAliasInfo portalAlias);

        /// <summary>
        /// Gets the portal alias info.
        /// </summary>
        /// <param name="alias">The portal alias.</param>
        /// <returns>Portal alias info</returns>
        PortalAliasInfo GetPortalAlias(string alias);

        /// <summary>
        /// Gets the portal alias info.
        /// </summary>
        /// <param name="alias">The portal alias.</param>
        /// <param name="portalId">The Id of the portal in question</param>
        /// <returns>Portal alias info</returns>
        PortalAliasInfo GetPortalAlias(string alias, int portalId);

        PortalAliasInfo GetPortalAliasByPortalAliasID(int portalAliasId);

        /// <summary>
        /// Gets all the portal aliases defined
        /// </summary>
        /// <returns>A dictionary keyed by the HTTP Alias</returns>
        PortalAliasCollection GetPortalAliases();

        /// <summary>
        /// Gets the portal alias by portal ID.
        /// </summary>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>Portal alias collection.</returns>
        IEnumerable<PortalAliasInfo> GetPortalAliasesByPortalId(int portalId);

        PortalInfo GetPortalByPortalAliasID(int PortalAliasId);

        /// <summary>
        /// Updates the portal alias info.
        /// </summary>
        /// <param name="portalAlias">The obj portal alias info.</param>
        void UpdatePortalAlias(PortalAliasInfo portalAlias);
    }
}