// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Connections;

using System;
using System.Collections.Generic;
using System.Linq;

using DotNetNuke.Framework;

public class ConnectionsController : ServiceLocator<IConnectionsController, ConnectionsController>, IConnectionsController
{
    /// <inheritdoc/>
    public IList<IConnector> GetConnections(int portalId)
    {
        var connectors = ConnectionsManager.Instance.GetConnectors().Where(c => c.HasConfig(portalId)).ToList();
        var allConnectors = new List<IConnector>();
        foreach (var con in connectors)
        {
            allConnectors.AddRange(con.GetConnectors(portalId));
        }

        return allConnectors;
    }

    /// <inheritdoc/>
    public IDictionary<string, string> GetConnectionConfigs(int portalId, string name)
    {
        var connector = ConnectionsManager.Instance.GetConnectors()
            .FirstOrDefault(c => c.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        if (connector != null)
        {
            return connector.GetConfig(portalId);
        }

        return null;
    }

    /// <inheritdoc/>
    protected override Func<IConnectionsController> GetFactory()
    {
        return () => new ConnectionsController();
    }
}
