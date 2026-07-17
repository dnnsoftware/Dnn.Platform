// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.MvcPipeline.Models
{
    /// <summary>
    /// Base model for MVC module view models, exposing common module context.
    /// </summary>
    public class ModuleModelBase
    {
        /// <summary>
        /// Gets or sets the identifier of the module instance.
        /// </summary>
        public int ModuleId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the tab on which the module resides.
        /// </summary>
        public int TabId { get; set; }

        /// <summary>
        /// Gets or sets the path to the local resource file for the module.
        /// </summary>
        public string LocalResourceFile { get; set; }
    }
}
