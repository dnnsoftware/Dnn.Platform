// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;

namespace DotNetNuke.Services.Connections
{
    public class ConnectionsController : ServiceLocator<IConnectionsController, ConnectionsController>, IConnectionsController
    {
        #region Overrides of ServiceLocator

        protected override Func<IConnectionsController> GetFactory()
        {
            return () => new ConnectionsController();
        }

        #endregion

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

    }
}
