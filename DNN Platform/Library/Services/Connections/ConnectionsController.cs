// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Connections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Framework;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The <see cref="IConnectionsController"/> implementation.</summary>
    public class ConnectionsController : ServiceLocator<IConnectionsController, ConnectionsController>, IConnectionsController
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IConnectionsManager connectionsManager;

        /// <summary>Initializes a new instance of the <see cref="ConnectionsController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IConnectionsManager. Scheduled removal in v12.0.0.")]
        public ConnectionsController()
            : this(Globals.GetCurrentServiceProvider(), null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ConnectionsController"/> class.</summary>
        /// <param name="serviceProvider">The DI container.</param>
        /// <param name="connectionsManager">The connections manager.</param>
        public ConnectionsController(IServiceProvider serviceProvider, IConnectionsManager connectionsManager)
        {
            this.serviceProvider = serviceProvider;
            this.connectionsManager = connectionsManager ?? serviceProvider.GetRequiredService<IConnectionsManager>();
        }

        /// <inheritdoc/>
        public IList<IConnector> GetConnections(int portalId)
        {
            return
                this.connectionsManager.GetConnectors(this.serviceProvider)
                    .Where(c => c.HasConfig(portalId))
                    .SelectMany(con => con.GetConnectors(portalId))
                    .ToList();
        }

        /// <inheritdoc/>
        public IDictionary<string, string> GetConnectionConfigs(int portalId, string name)
        {
            return
                this.connectionsManager.GetConnectors(this.serviceProvider)
                    .Where(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    .Select(c => c.GetConfig(portalId))
                    .FirstOrDefault();
        }

        /// <inheritdoc/>
        protected override Func<IConnectionsController> GetFactory()
        {
            return Globals.DependencyProvider.GetRequiredService<IConnectionsController>;
        }
    }
}
