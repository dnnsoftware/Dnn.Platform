// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.ClientResources;

using System.Collections.Generic;

/// <summary>Provides an interface for managing client resources.</summary>
public interface IClientResourcesController
{
    /// <summary>Registers a stylesheet that has an admin level priority.</summary>
    /// <param name="filePath">The path to the CSS stylesheet.</param>
    void RegisterAdminStylesheet(string filePath);

    /// <summary>Registers the <c>default.css</c> stylesheet.</summary>
    /// <param name="filePath">The path to the CSS stylesheet.</param>
    void RegisterDefaultStylesheet(string filePath);

    /// <summary>Registers a stylesheet for a specific feature.</summary>
    /// <param name="filePath">The path to the CSS stylesheet.</param>
    void RegisterFeatureStylesheet(string filePath);

    /// <summary>Requests that a CSS file be registered on the client browser.</summary>
    /// <param name="filePath">The relative file path to the CSS resource.</param>
    void RegisterStyleSheet(string filePath);

    /// <summary>Requests that a CSS file be registered on the client browser.</summary>
    /// <param name="filePath">The relative file path to the CSS resource.</param>
    /// <param name="htmlAttributes">A dictionary of HTML attributes to use for the <c>link</c> tag. The key being the attribute name and the value its value.</param>
    void RegisterStyleSheet(string filePath, IDictionary<string, string> htmlAttributes);

    /// <summary>Requests that a CSS file be registered on the client browser. Defaults to rendering in the page header.</summary>
    /// <param name="filePath">The relative file path to the CSS resource.</param>
    /// <param name="priority">The relative priority in which the file should be loaded.</param>
    void RegisterStyleSheet(string filePath, int priority);

    /// <summary>Requests that a CSS file be registered on the client browser. Defaults to rendering in the page header.</summary>
    /// <param name="filePath">The relative file path to the CSS resource.</param>
    /// <param name="priority">The relative priority in which the file should be loaded.</param>
    /// <param name="htmlAttributes">A dictionary of HTML attributes to use for the <c>link</c> tag. The key being the attribute name and the value its value.</param>
    void RegisterStyleSheet(string filePath, int priority, IDictionary<string, string> htmlAttributes);

    /// <summary>Requests that a CSS file be registered on the client browser. Defaults to rendering in the page header.</summary>
    /// <param name="filePath">The relative file path to the CSS resource.</param>
    /// <param name="priority">The relative priority in which the file should be loaded.</param>
    void RegisterStyleSheet(string filePath, FileOrder.Css priority);

    /// <summary>Requests that a CSS file be registered on the client browser. Defaults to rendering in the page header.</summary>
    /// <param name="filePath">The relative file path to the CSS resource.</param>
    /// <param name="priority">The relative priority in which the file should be loaded.</param>
    /// <param name="htmlAttributes">A dictionary of HTML attributes to use for the <c>link</c> tag. The key being the attribute name and the value its value.</param>
    void RegisterStyleSheet(string filePath, FileOrder.Css priority, IDictionary<string, string> htmlAttributes);

    /// <summary>Requests that a CSS file be registered on the client browser. Allows for overriding the default provider.</summary>
    /// <param name="filePath">The relative file path to the CSS resource.</param>
    /// <param name="priority">The relative priority in which the file should be loaded.</param>
    /// <param name="provider">The provider name to be used to render the css file on the page.</param>
    void RegisterStyleSheet(string filePath, int priority, string provider);

    /// <summary>Requests that a CSS file be registered on the client browser. Allows for overriding the default provider.</summary>
    /// <param name="filePath">The relative file path to the CSS resource.</param>
    /// <param name="priority">The relative priority in which the file should be loaded.</param>
    /// <param name="provider">The provider name to be used to render the css file on the page.</param>
    /// <param name="htmlAttributes">A dictionary of HTML attributes to use for the <c>link</c> tag. The key being the attribute name and the value its value.</param>
    void RegisterStyleSheet(string filePath, int priority, string provider, IDictionary<string, string> htmlAttributes);

    /// <summary>Requests that a CSS file be registered on the client browser. Allows for overriding the default provider.</summary>
    /// <param name="filePath">The relative file path to the CSS resource.</param>
    /// <param name="priority">The relative priority in which the file should be loaded.</param>
    /// <param name="provider">The provider name to be used to render the css file on the page.</param>
    /// <param name="name">Name of framework like Bootstrap, Angular, etc.</param>
    /// <param name="version">Version number of framework.</param>
    void RegisterStyleSheet(string filePath, int priority, string provider, string name, string version);

    /// <summary>Requests that a CSS file be registered on the client browser. Allows for overriding the default provider.</summary>
    /// <param name="filePath">The relative file path to the CSS resource.</param>
    /// <param name="priority">The relative priority in which the file should be loaded.</param>
    /// <param name="provider">The provider name to be used to render the css file on the page.</param>
    /// <param name="name">Name of framework like Bootstrap, Angular, etc.</param>
    /// <param name="version">Version number of framework.</param>
    /// <param name="htmlAttributes">A dictionary of HTML attributes to use for the <c>link</c> tag. The key being the attribute name and the value its value.</param>
    void RegisterStyleSheet(string filePath, int priority, string provider, string name, string version, IDictionary<string, string> htmlAttributes);

    /// <summary>Requests that a JavaScript file be registered on the client browser.</summary>
    /// <param name="filePath">The relative file path to the JavaScript resource.</param>
    void RegisterScript(string filePath);

    /// <summary>Requests that a JavaScript file be registered on the client browser.</summary>
    /// <param name="filePath">The relative file path to the JavaScript resource.</param>
    /// <param name="htmlAttributes">A dictionary of HTML attributes to use for the <c>script</c> tag. The key being the attribute name and the value its value.</param>
    void RegisterScript(string filePath, IDictionary<string, string> htmlAttributes);

    /// <summary>Requests that a JavaScript file be registered on the client browser.</summary>
    /// <param name="filePath">The relative file path to the JavaScript resource.</param>
    /// <param name="priority">The relative priority in which the file should be loaded.</param>
    void RegisterScript(string filePath, int priority);

    /// <summary>Requests that a JavaScript file be registered on the client browser.</summary>
    /// <param name="filePath">The relative file path to the JavaScript resource.</param>
    /// <param name="priority">The relative priority in which the file should be loaded.</param>
    /// <param name="htmlAttributes">A dictionary of HTML attributes to use for the <c>script</c> tag. The key being the attribute name and the value its value.</param>
    void RegisterScript(string filePath, int priority, IDictionary<string, string> htmlAttributes);

    /// <summary>Requests that a JavaScript file be registered on the client browser.</summary>
    /// <param name="filePath">The relative file path to the JavaScript resource.</param>
    /// <param name="priority">The relative priority in which the file should be loaded.</param>
    void RegisterScript(string filePath, FileOrder.Js priority);

    /// <summary>Requests that a JavaScript file be registered on the client browser.</summary>
    /// <param name="filePath">The relative file path to the JavaScript resource.</param>
    /// <param name="priority">The relative priority in which the file should be loaded.</param>
    /// <param name="htmlAttributes">A dictionary of HTML attributes to use for the <c>script</c> tag. The key being the attribute name and the value its value.</param>
    void RegisterScript(string filePath, FileOrder.Js priority, IDictionary<string, string> htmlAttributes);

    /// <summary>Requests that a JavaScript file be registered on the client browser.</summary>
    /// <param name="filePath">The relative file path to the JavaScript resource.</param>
    /// <param name="priority">The relative priority in which the file should be loaded.</param>
    /// <param name="provider">The name of the provider responsible for rendering the script output.</param>
    void RegisterScript(string filePath, FileOrder.Js priority, string provider);

    /// <summary>Requests that a JavaScript file be registered on the client browser.</summary>
    /// <param name="filePath">The relative file path to the JavaScript resource.</param>
    /// <param name="priority">The relative priority in which the file should be loaded.</param>
    /// <param name="provider">The name of the provider responsible for rendering the script output.</param>
    /// <param name="htmlAttributes">A dictionary of HTML attributes to use for the <c>script</c> tag. The key being the attribute name and the value its value.</param>
    void RegisterScript(string filePath, FileOrder.Js priority, string provider, IDictionary<string, string> htmlAttributes);

    /// <summary>Requests that a JavaScript file be registered on the client browser.</summary>
    /// <param name="filePath">The relative file path to the JavaScript resource.</param>
    /// <param name="priority">The relative priority in which the file should be loaded.</param>
    /// <param name="provider">The name of the provider responsible for rendering the script output.</param>
    void RegisterScript(string filePath, int priority, string provider);

    /// <summary>Requests that a JavaScript file be registered on the client browser.</summary>
    /// <param name="filePath">The relative file path to the JavaScript resource.</param>
    /// <param name="priority">The relative priority in which the file should be loaded.</param>
    /// <param name="provider">The name of the provider responsible for rendering the script output.</param>
    /// <param name="htmlAttributes">A dictionary of HTML attributes to use for the <c>script</c> tag. The key being the attribute name and the value its value.</param>
    void RegisterScript(string filePath, int priority, string provider, IDictionary<string, string> htmlAttributes);

    /// <summary>Requests that a JavaScript file be registered on the client browser.</summary>
    /// <param name="filePath">The relative file path to the JavaScript resource.</param>
    /// <param name="priority">The relative priority in which the file should be loaded.</param>
    /// <param name="provider">The name of the provider responsible for rendering the script output.</param>
    /// <param name="name">Name of framework like Bootstrap, Angular, etc.</param>
    /// <param name="version">Version number of framework.</param>
    void RegisterScript(string filePath, int priority, string provider, string name, string version);

    /// <summary>Requests that a JavaScript file be registered on the client browser.</summary>
    /// <param name="filePath">The relative file path to the JavaScript resource.</param>
    /// <param name="priority">The relative priority in which the file should be loaded.</param>
    /// <param name="provider">The name of the provider responsible for rendering the script output.</param>
    /// <param name="name">Name of framework like Bootstrap, Angular, etc.</param>
    /// <param name="version">Version number of framework.</param>
    /// <param name="htmlAttributes">A dictionary of HTML attributes to use for the <c>script</c> tag. The key being the attribute name and the value its value.</param>
    void RegisterScript(string filePath, int priority, string provider, string name, string version, IDictionary<string, string> htmlAttributes);
}
