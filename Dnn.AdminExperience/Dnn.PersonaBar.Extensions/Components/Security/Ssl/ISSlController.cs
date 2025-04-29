// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Extensions.Components.Security.Ssl;

public interface ISSlController
{
    /// <summary>Get statistics on the specified portal.</summary>
    /// <param name="portalId">Portal id of the portal.</param>
    /// <returns>PortalStats object.</returns>
    PortalStats GetPortalStats(int portalId);
}
