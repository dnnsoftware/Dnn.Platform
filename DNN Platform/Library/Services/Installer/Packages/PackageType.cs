// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Security;

#endregion

namespace DotNetNuke.Services.Installer.Packages
{
    /// <summary>
    /// This class allows PackageType to have a memeber named PackageType
    /// to remain compatible with the original VB implementation
    /// </summary>
    [Serializable]
    public class PackageTypeMemberNameFixer
    {
        public PackageTypeMemberNameFixer()
        {
            this.PackageType = string.Empty;
        }

        public string PackageType { get; set; }
    }

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The PackageType class represents a single Installer Package Type
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class PackageType : PackageTypeMemberNameFixer
    {
        public string Description { get; set; }
        public string EditorControlSrc { get; set; }
        public SecurityAccessLevel SecurityAccessLevel { get; set; }
        public bool SupportsSideBySideInstallation { get; set; }
    }
}
