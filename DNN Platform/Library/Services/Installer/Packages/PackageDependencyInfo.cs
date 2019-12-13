// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

namespace DotNetNuke.Services.Installer.Packages
{
    [Serializable]
    public class PackageDependencyInfo
    {
	    public int PackageDependencyId { get; set; }
	    public int PackageId { get; set; }
	    public string PackageName { get; set; }
	    public Version Version { get; set; }
    }
}
