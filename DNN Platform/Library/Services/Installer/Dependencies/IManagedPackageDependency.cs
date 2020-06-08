// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using DotNetNuke.Services.Installer.Packages;

namespace DotNetNuke.Services.Installer.Dependencies
{
    public interface IManagedPackageDependency
    {
        PackageDependencyInfo PackageDependency { get; set; }
    }
}
