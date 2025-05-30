// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Framework.JavaScriptLibraries;

using System;
using System.Web.UI;

/// <summary>Provides the ability to request JavaScript libraries to be registered on a page.</summary>
public interface IJavaScriptLibraryHelper
{
    /// <summary>checks whether the script file is a known JavaScript library.</summary>
    /// <param name="libraryName">The name of the JavaScript library.</param>
    /// <returns><see langword="true"/> if a library with the given name is installed, otherwise <see langword="false"/>.</returns>
    bool IsInstalled(string libraryName);

    /// <summary>determine whether to use the debug script for a file.</summary>
    /// <returns>whether to use the debug script.</returns>
    bool UseDebugScript();

    /// <summary>Returns the version of a javascript library from the database.</summary>
    /// <param name="libraryName">The name of the JavaScript library.</param>
    /// <returns>The highest version number of the library or <see cref="string.Empty"/>.</returns>
    string Version(string libraryName);

    /// <summary>Requests a script to be added to the page.</summary>
    /// <param name="libraryName">The name of the JavaScript library.</param>
    void RequestRegistration(string libraryName);

    /// <summary>Requests a script to be added to the page.</summary>
    /// <param name="libraryName">The name of the JavaScript library.</param>
    /// <param name="version">the library's version.</param>
    void RequestRegistration(string libraryName, Version version);

    /// <summary>Requests a script to be added to the page.</summary>
    /// <param name="libraryName">The name of the JavaScript library.</param>
    /// <param name="version">the library's version.</param>
    /// <param name="specific">
    /// how much of the <paramref name="version"/> to pay attention to.
    /// When <see cref="SpecificVersion.Latest"/> is passed, ignore the <paramref name="version"/>.
    /// When <see cref="SpecificVersion.LatestMajor"/> is passed, match the major version.
    /// When <see cref="SpecificVersion.LatestMinor"/> is passed, match the major and minor versions.
    /// When <see cref="SpecificVersion.Exact"/> is passed, match all parts of the version.
    /// </param>
    void RequestRegistration(string libraryName, Version version, SpecificVersion specific);

    /// <summary>Gets the path for a JS library's primary script.</summary>
    /// <param name="libraryName">The name of the JavaScript library.</param>
    /// <returns>The path or <see langword="null"/>.</returns>
    string GetScriptPath(string libraryName);
}
