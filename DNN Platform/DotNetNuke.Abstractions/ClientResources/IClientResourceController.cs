// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.ClientResources;

/// <summary>Provides an interface for managing client resources.</summary>
public interface IClientResourceController
{
    /// <summary>
    /// Adds a font resource to the client resources controller.
    /// </summary>
    /// <param name="font">The <see cref="IFontResource"/> to add.</param>
    void AddFont(IFontResource font);

    /// <summary>
    /// Adds a script resource to the client resources controller.
    /// </summary>
    /// <param name="script">The <see cref="IScriptResource"/> to add.</param>
    void AddScript(IScriptResource script);

    /// <summary>
    /// Adds a stylesheet resource to the client resources controller.
    /// </summary>
    /// <param name="stylesheet">The <see cref="IStylesheetResource"/> to add.</param>
    void AddStylesheet(IStylesheetResource stylesheet);

    /// <summary>
    /// Create a new font resource.
    /// </summary>
    /// <param name="sourcePath">The source URL to set.</param>
    /// <returns>An <see cref="IFontResource"/> instance representing the created font resource.</returns>
    IFontResource CreateFont(string sourcePath);

    /// <summary>
    /// Create a new font resource.
    /// </summary>
    /// <param name="sourcePath">The source URL to set.</param>
    /// <param name="pathNameAlias">The path alias to set.</param>
    /// <returns>An <see cref="IFontResource"/> instance representing the created font resource.</returns>
    IFontResource CreateFont(string sourcePath, string pathNameAlias);

    /// <summary>
    /// Create a new font resource.
    /// </summary>
    /// <param name="sourcePath">The source URL to set.</param>
    /// <param name="pathNameAlias">The path alias to set.</param>
    /// <param name="mimeType">The MIME type of the resource.</param>
    /// <returns>An <see cref="IFontResource"/> instance representing the created font resource.</returns>
    IFontResource CreateFont(string sourcePath, string pathNameAlias, string mimeType);

    /// <summary>
    /// Create a new script resource.
    /// </summary>
    /// <param name="sourcePath">The source URL to set.</param>
    /// <returns>An <see cref="IScriptResource"/> instance representing the created script resource.</returns>
    IScriptResource CreateScript(string sourcePath);

    /// <summary>
    /// Create a new script resource.
    /// </summary>
    /// <param name="sourcePath">The source URL to set.</param>
    /// <param name="pathNameAlias">The path alias to set.</param>
    /// <returns>An <see cref="IScriptResource"/> instance representing the created script resource.</returns>
    IScriptResource CreateScript(string sourcePath, string pathNameAlias);

    /// <summary>
    /// Creates a new stylesheet resource.
    /// </summary>
    /// <param name="sourcePath">The source URL to set.</param>
    /// <returns>An <see cref="IStylesheetResource"/> instance representing the created stylesheet resource.</returns>
    IStylesheetResource CreateStylesheet(string sourcePath);

    /// <summary>
    /// Creates a new stylesheet resource.
    /// </summary>
    /// <param name="sourcePath">The source URL to set.</param>
    /// <param name="pathNameAlias">The path alias to set.</param>
    /// <returns>An <see cref="IStylesheetResource"/> instance representing the created stylesheet resource.</returns>
    IStylesheetResource CreateStylesheet(string sourcePath, string pathNameAlias);

    /// <summary>
    /// Registers a path name alias for resolving resource paths.
    /// </summary>
    /// <param name="pathNameAlias">The path name alias.</param>
    /// <param name="resolvedPath">The resolved path corresponding to the alias.</param>
    void RegisterPathNameAlias(string pathNameAlias, string resolvedPath);

    /// <summary>
    /// Removes a font resource by its name.
    /// </summary>
    /// <param name="fontName">The name of the font resource to remove.</param>
    void RemoveFontByName(string fontName);

    /// <summary>
    /// Removes a font resource by its path.
    /// </summary>
    /// <param name="fontPath">The path of the font resource to remove.</param>
    /// <param name="pathNameAlias">The name alias for the path to remove.</param>
    void RemoveFontByPath(string fontPath, string pathNameAlias);

    /// <summary>
    /// Removes a script resource by its name.
    /// </summary>
    /// <param name="scriptName">The name of the script resource to remove.</param>
    void RemoveScriptByName(string scriptName);

    /// <summary>
    /// Removes a script resource by its path.
    /// </summary>
    /// <param name="scriptPath">The path of the script resource to remove.</param>
    /// <param name="pathNameAlias">The name alias for the path to remove.</param>
    void RemoveScriptByPath(string scriptPath, string pathNameAlias);

    /// <summary>
    /// Removes a stylesheet resource by its name.
    /// </summary>
    /// <param name="stylesheetName">The name of the stylesheet resource to remove.</param>
    void RemoveStylesheetByName(string stylesheetName);

    /// <summary>
    /// Removes a stylesheet resource by its path.
    /// </summary>
    /// <param name="stylesheetPath">The path of the stylesheet resource to remove.</param>
    /// <param name="pathNameAlias">The name alias for the path to remove.</param>
    void RemoveStylesheetByPath(string stylesheetPath, string pathNameAlias);

    /// <summary>
    /// Renders the dependencies for the specified resource type and provider.
    /// </summary>
    /// <param name="resourceType">The type of resource to render dependencies for.</param>
    /// <param name="provider">The provider to use for rendering dependencies.</param>
    /// <param name="applicationPath">The application path to use for resolving resource paths.</param>
    /// <returns>A string containing the rendered dependencies.</returns>
    string RenderDependencies(ResourceType resourceType, string provider, string applicationPath);
}
