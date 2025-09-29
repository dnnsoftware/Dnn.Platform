// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.ClientResources;

/// <summary>Marker interface for resources that resolve to a link in html.</summary>
public interface ILinkResource : IResource
{
    /// <summary>
    /// Gets or sets a value indicating whether the client resource should be preloaded.
    /// </summary>
    bool Preload { get; set; }
}
