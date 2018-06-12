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
