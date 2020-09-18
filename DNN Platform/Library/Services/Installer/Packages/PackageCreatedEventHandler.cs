// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Installer.Packages
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Services.Installer.Packages
    /// Class:      PackageCreatedEventHandler
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The PackageCreatedEventHandler delegate defines a custom event handler for a
    /// PAckage Created Event.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public delegate void PackageCreatedEventHandler(object sender, PackageCreatedEventArgs e);
}
