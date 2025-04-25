// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs.TabVersions
{
    /// <summary>This enum represents the possible list of action that can be done in a Tab Version (i.e.: add module, modified module, deleted module, reset (restore version)).</summary>
    public enum TabVersionDetailAction
    {
        /// <summary>Added a module.</summary>
        Added = 0,

        /// <summary>Modified a module.</summary>
        Modified = 1,

        /// <summary>Deleted a module.</summary>
        Deleted = 2,

        /// <summary>Restored a version.</summary>
        Reset = 3,
    }
}
