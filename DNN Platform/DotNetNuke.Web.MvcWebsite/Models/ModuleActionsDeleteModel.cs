// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcWebsite.Models
{
    /// <summary>
    /// Represents the model for module deletion requests.
    /// </summary>
    public class ModuleActionsDeleteModel
    {
        /// <summary>
        /// Gets or sets the module identifier to delete.
        /// </summary>
        public int ModuleId { get; set; }

        /// <summary>
        /// Gets or sets the tab (page) identifier where the module is located.
        /// </summary>
        public int TabId { get; set; }
    }
}
