// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Connections;

using System.Collections.Generic;

public interface IConnectionsController
{
    /// <summary>Get Connection Settings for a Site.</summary>
    /// <returns>A list of <see cref="IConnector"/> instances.</returns>
    IList<IConnector> GetConnections(int portalId);

    /// <summary>Get Connection Configuration Value.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <param name="name">The connector name.</param>
    /// <returns>A dictionary of configuration values for the connector, or <see langword="null"/>.</returns>
    IDictionary<string, string> GetConnectionConfigs(int portalId, string name);
}
