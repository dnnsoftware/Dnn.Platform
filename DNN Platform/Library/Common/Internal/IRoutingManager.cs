// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Internal;

/// <summary>A contract specifying the ability for a routing manager to register its routes.</summary>
public interface IRoutingManager
{
    /// <summary>Register the routes.</summary>
    void RegisterRoutes();
}
