// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Steps
{
    /// <summary>Replaces a given module in a page with another module, for a specific portal.</summary>
    internal interface IReplacePortalTabModuleStep : IStep
    {
        /// <summary>
        /// Gets or sets the parent <see cref="IReplaceTabModuleStep"/> instance,
        /// which provides information of page name, old module name and new module name.
        /// </summary>
        IReplaceTabModuleStep ParentStep { get; set; }

        /// <summary>Gets or sets the Portal Id.</summary>
        int PortalId { get; set; }
    }
}
