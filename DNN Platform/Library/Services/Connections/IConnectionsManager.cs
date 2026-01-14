// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Connections
{
    using System;
    using System.Collections.Generic;

    /// <summary>A contract specifying the ability to get the <see cref="IConnector"/> instances.</summary>
    public interface IConnectionsManager
    {
        /// <summary>Gets all of the registered connectors.</summary>
        /// <param name="serviceProvider">The DI container.</param>
        /// <returns>A list of <see cref="IConnector"/> instances.</returns>
        IList<IConnector> GetConnectors(IServiceProvider serviceProvider);
    }
}
