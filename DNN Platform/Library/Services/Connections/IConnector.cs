// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Connections
{
    using System.Collections.Generic;

    public interface IConnector
    {
        /// <summary>
        /// Gets unique name of the connector. It is used to distinguish between different types of connectors.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets category of the connector. It can be used to sort similar type resources.
        /// </summary>
        ConnectorCategories Type { get; }

        /// <summary>
        /// Gets a value indicating whether determines whether this connector supports multiple connections or not.
        /// </summary>
        bool SupportsMultiple { get; }

        /// <summary>
        /// Gets icon url of the connector.
        /// </summary>
        string IconUrl { get; }

        /// <summary>
        /// Gets plugins folder for the connector.
        /// </summary>
        string PluginFolder { get; }

        /// <summary>
        /// Gets a value indicating whether determines if it is engage connector or not.
        /// </summary>
        bool IsEngageConnector { get; }

        /// <summary>
        /// Gets or sets id of the connector. It is required if SupportsMultiple is true.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Gets or sets display name of the connector.
        /// </summary>
        string DisplayName { get; set; }

        /// <summary>
        /// Checks if the connector has been configured or not.
        /// </summary>
        /// <param name="portalId"></param>
        /// <returns></returns>
        bool HasConfig(int portalId);

        /// <summary>
        /// Get the connector configuration.
        /// </summary>
        /// <param name="portalId"></param>
        /// <returns></returns>
        IDictionary<string, string> GetConfig(int portalId);

        /// <summary>
        /// Save the connector configuration. This will work as both edit and new  if SupportsMultiple is true.
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="values"></param>
        /// <param name="validated"></param>
        /// <param name="customErrorMessage"></param>
        /// <returns></returns>
        bool SaveConfig(int portalId, IDictionary<string, string> values, ref bool validated, out string customErrorMessage);

        /// <summary>
        /// Get all the connectors of a particular type.
        /// </summary>
        /// <param name="portalId"></param>
        /// <returns></returns>
        IEnumerable<IConnector> GetConnectors(int portalId);

        /// <summary>
        /// Delete a connector. This is used only if SupportsMultiple is true.
        /// </summary>
        /// <param name="portalId"></param>
        void DeleteConnector(int portalId);
    }
}
