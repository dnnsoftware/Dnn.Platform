// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
