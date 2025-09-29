// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.ClientResources;

/// <summary>Marker interface for script resources.</summary>
public interface IScriptResource : IResource
{
    /// <summary>
    /// Gets or sets a value indicating whether the async attribute should be added to the script element.
    /// For classic scripts, if the async attribute is present, then the classic script will be fetched in parallel to parsing and evaluated as soon as it is available.
    /// For module scripts, if the async attribute is present then the scripts and all their dependencies will be fetched in parallel to parsing and evaluated as soon as they are available.
    /// If the attribute is specified with the defer attribute, the element will act as if only the async attribute is specified.
    /// </summary>
    bool Async { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the defer attribute should be added to the script element.
    /// This Boolean attribute is set to indicate to a browser that the script is meant to be executed after the document has been parsed, but before firing DOMContentLoaded event.
    /// Scripts with the defer attribute will prevent the DOMContentLoaded event from firing until the script has loaded and finished evaluating.
    /// Scripts with the defer attribute will execute in the order in which they appear in the document.
    /// If the attribute is specified with the async attribute, the element will act as if only the async attribute is specified.
    /// </summary>
    bool Defer { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the noModule attribute should be added to the script element.
    /// This Boolean attribute is set to indicate that the script should not be executed in browsers that support ES modules — in effect,
    /// this can be used to serve fallback scripts to older browsers that do not support modular JavaScript code.
    /// </summary>
    bool NoModule { get; set; }
}
