// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Modules
{
    using System;

    /// <summary>The DesktopModuleSupportedFeature enum provides an enumeration of Supported Features.</summary>
    [Flags]
    public enum DesktopModuleSupportedFeature
    {
        /// <summary>The module supports import and export.</summary>
        IsPortable = 1,

        /// <summary>The module supports indexing module content.</summary>
        IsSearchable = 2,

        /// <summary>The module supports running custom logic on installs and upgrades of the module.</summary>
        IsUpgradeable = 4,
    }
}
