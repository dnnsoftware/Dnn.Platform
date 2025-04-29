// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules;

using DotNetNuke.Entities.Modules.Actions;

/// <summary>Modules that implement this interface can define their module actions to show in the actions menu.</summary>
public interface IActionable
{
    /// <summary>
    /// Gets the collection of module actions, <see cref="ModuleActionCollection"/>.
    /// <seealso cref="ModuleAction"/>.
    /// </summary>
    ModuleActionCollection ModuleActions { get; }
}
