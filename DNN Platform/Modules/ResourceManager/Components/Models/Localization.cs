// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Components.Models;

/// <summary>Represents a localization caching information.</summary>
public class Localization
{
    /// <summary>Gets or sets the resource file localization cache key.</summary>
    public string ResxDataCacheKey { get; set; }

    /// <summary>Gets or sets the modified date cache key.</summary>
    public string ResxModifiedDateCacheKey { get; set; }
}
