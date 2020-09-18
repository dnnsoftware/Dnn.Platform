// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Installer.Packages
{
    using System;

    [Serializable]
    public class PackageDependencyInfo
    {
        public int PackageDependencyId { get; set; }

        public int PackageId { get; set; }

        public string PackageName { get; set; }

        public Version Version { get; set; }
    }
}
