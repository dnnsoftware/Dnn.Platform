// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Tabs
{
    public enum PortalAliasUsageType
    {
        /// <summary>Default.</summary>
        Default = 0,

        /// <summary>Child pages inherit.</summary>
        ChildPagesInherit = 1,

        /// <summary>Child pages do not inherit.</summary>
        ChildPagesDoNotInherit = 2,

        /// <summary>Inherited from parent.</summary>
        InheritedFromParent = 3,
    }
}
