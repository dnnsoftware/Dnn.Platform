// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Urls;

using DotNetNuke.Internal.SourceGenerators;

using NewBrowserTypes = DotNetNuke.Abstractions.Urls.BrowserTypes;
#pragma warning disable CS0618 // Type or member is obsolete
using OldBrowserTypes = DotNetNuke.Entities.Urls.BrowserTypes;
#pragma warning restore CS0618 // Type or member is obsolete

/// <summary>Extension methods for <see cref="BrowserTypes"/>.</summary>
[DnnDeprecated(9, 7, 2, "Use DotNetNuke.Abstractions.Urls.BrowserTypes instead")]
public static partial class BrowserTypesExtensions
{
    /// <summary>Convert between <see cref="OldBrowserTypes"/> and <see cref="NewBrowserTypes"/>.</summary>
    /// <param name="browserTypes">The old browser types value.</param>
    /// <returns>The new browser types value.</returns>
    public static NewBrowserTypes ToAbstractionsBrowserTypes(this OldBrowserTypes browserTypes)
    {
        return (NewBrowserTypes)browserTypes;
    }

    /// <summary>Convert between <see cref="OldBrowserTypes"/> and <see cref="NewBrowserTypes"/>.</summary>
    /// <param name="browserTypes">The old browser types value.</param>
    /// <returns>The new browser types value.</returns>
    public static OldBrowserTypes ToDeprecatedBrowserTypes(this NewBrowserTypes browserTypes)
    {
        return (OldBrowserTypes)browserTypes;
    }
}
