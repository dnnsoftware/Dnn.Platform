// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Urls;

using System;

/// <inheritdoc cref="Abstractions.Urls.BrowserTypes"/>
[Obsolete("Deprecated in DotNetNuke 9.7.2. Use DotNetNuke.Abstractions.Urls.BrowserTypes instead. Scheduled removal in v11.0.0.")]
public enum BrowserTypes
{
    /// <inheritdoc cref="Abstractions.Urls.BrowserTypes.Normal"/>
    Normal = 0,

    /// <inheritdoc cref="Abstractions.Urls.BrowserTypes.Mobile"/>
    Mobile = 1,
}
