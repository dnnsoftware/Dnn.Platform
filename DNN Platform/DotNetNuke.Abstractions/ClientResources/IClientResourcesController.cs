// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.ClientResources;

/// <summary>Provides an interface for managing client resources.</summary>
public interface IClientResourcesController
{
    /// <summary>
    /// Adds a link resource to the client resources controller.
    /// </summary>
    /// <param name="link">The <see cref="ILinkResource"/> to add.</param>
    void AddLink(ILinkResource link);

    /// <summary>
    /// Adds a script resource to the client resources controller.
    /// </summary>
    /// <param name="script">The <see cref="IScriptResource"/> to add.</param>
    void AddScript(IScriptResource script);

    /// <summary>
    /// Creates a new link resource.
    /// </summary>
    /// <returns>An <see cref="ILinkResource"/> instance representing the created link resource.</returns>
    ILinkResource CreateLink();

    /// <summary>
    /// Create a new script resource.
    /// </summary>
    /// <returns>An <see cref="IScriptResource"/> instance representing the created script resource.</returns>
    IScriptResource CreateScript();

    /// <summary>
    /// Registers a link resource by its path.
    /// </summary>
    /// <param name="linkPath">The path to the link resource to register.</param>
    void RegisterLink(string linkPath);

    /// <summary>
    /// Registers a script resource by its path.
    /// </summary>
    /// <param name="scriptPath">The path to the script resource to register.</param>
    void RegisterScript(string scriptPath);

    /// <summary>
    /// Removes a link resource by its name.
    /// </summary>
    /// <param name="linkName">The name of the link resource to remove.</param>
    void RemoveLinkByName(string linkName);

    /// <summary>
    /// Removes a link resource by its path.
    /// </summary>
    /// <param name="linkPath">The path of the link resource to remove.</param>
    void RemoveLinkByPath(string linkPath);

    /// <summary>
    /// Removes a script resource by its name.
    /// </summary>
    /// <param name="scriptName">The name of the script resource to remove.</param>
    void RemoveScriptByName(string scriptName);

    /// <summary>
    /// Removes a script resource by its path.
    /// </summary>
    /// <param name="scriptPath">The path of the script resource to remove.</param>
    void RemoveScriptByPath(string scriptPath);
}
