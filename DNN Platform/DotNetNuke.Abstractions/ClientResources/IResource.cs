// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.ClientResources;

using System.Collections.Generic;

/// <summary>
/// Represents a client resource that can be registered with the application.
/// </summary>
public interface IResource
{
    /// <summary>
    /// Gets or sets the file path of the client resource.
    /// </summary>
    string FilePath { get; set; }

    /// <summary>
    /// Gets or sets the path name alias for the client resource.
    /// </summary>
    string PathNameAlias { get; set; }

    /// <summary>
    /// Gets or sets the priority of the client resource.
    /// </summary>
    int Priority { get; set; }

    /// <summary>
    /// Gets or sets the provider of the client resource.
    /// </summary>
    string Provider { get; set; }

    /// <summary>
    /// Gets or sets the name of the client resource.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Gets or sets the version of the client resource.
    /// </summary>
    string Version { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to force the version of the client resource.
    /// </summary>
    bool ForceVersion { get; set; }

    /// <summary>
    /// Gets or sets the cross-origin policy for the client resource.
    /// </summary>
    CrossOrigin CrossOrigin { get; set; }

    /// <summary>
    /// Gets or sets the fetch priority for the client resource.
    /// </summary>
    FetchPriority FetchPriority { get; set; }

    /// <summary>
    /// Gets or sets the referrer policy for the client resource.
    /// </summary>
    ReferrerPolicy ReferrerPolicy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the client resource should be preloaded.
    /// </summary>
    bool Preload { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the client resource is blocking.
    /// </summary>
    bool Blocking { get; set; }

    /// <summary>
    /// Gets or sets the integrity attribute for the client resource.
    /// Contains inline metadata — a base64-encoded cryptographic hash of the resource (file) you're telling the browser to fetch. 
    /// The browser can use this to verify that the fetched resource has been delivered without unexpected manipulation.
    /// </summary>
    string Integrity { get; set; }

    /// <summary>
    /// Gets or sets additional attributes for the client resource.
    /// </summary>
    Dictionary<string, string> Attributes { get; set; }

    /// <summary>
    /// Registers the client resource.
    /// </summary>
    void Register();
}
