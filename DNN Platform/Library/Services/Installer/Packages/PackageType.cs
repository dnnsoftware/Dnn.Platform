﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
            PackageType = string.Empty;
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
