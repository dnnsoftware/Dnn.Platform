// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.ClientResources;

/// <summary>
/// Specifies the type of client resource.
/// </summary>
public enum ResourceType
{
    /// <summary>
    /// Represents all resource types.
    /// </summary>
    All,

    /// <summary>
    /// Represents a script resource type.
    /// </summary>
    Script,

    /// <summary>
    /// Represents a link resource type such as a CSS file.
    /// </summary>
    Link,
}
