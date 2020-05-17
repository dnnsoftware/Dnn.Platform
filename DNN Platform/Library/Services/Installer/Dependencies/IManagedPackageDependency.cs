// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Services.Installer.Packages;

namespace DotNetNuke.Services.Installer.Dependencies
{
    public interface IManagedPackageDependency
    {
        PackageDependencyInfo PackageDependency { get; set; }
    }
}
