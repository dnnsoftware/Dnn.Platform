﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System.Collections;
using System.Collections.Generic;

namespace DotNetNuke.Services.Connections
{
    public interface IConnectionsController
    {
        /// <summary>
        /// Get Connection Settings for a Site
        /// </summary>
        IList<IConnector> GetConnections(int portalId);

        /// <summary>
        /// Get Connection Configuration Value.
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        IDictionary<string, string> GetConnectionConfigs(int portalId, string name);
    }
}
