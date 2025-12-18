// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Installer.Packages;

using System.Diagnostics.CodeAnalysis;

/// <summary>The PackageCreatedEventHandler delegate defines a custom event handler for a Package Created Event.</summary>
/// <param name="sender">The event sender.</param>
/// <param name="e">The event arguments.</param>
[SuppressMessage("Microsoft.Design", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Breaking change")]
public delegate void PackageCreatedEventHandler(object sender, PackageCreatedEventArgs e);
