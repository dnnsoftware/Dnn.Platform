// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.MvcPipeline.Models
{
    /// <summary>
    /// View model used to invoke an MVC module control within a container.
    /// </summary>
    public class ControlViewModel
    {
        /// <summary>
        /// Gets or sets the identifier of the module.
        /// </summary>
        public int ModuleId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the tab on which the module resides.
        /// </summary>
        public int TabId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the module control to render.
        /// </summary>
        public int ModuleControlId { get; set; }

        /// <summary>
        /// Gets or sets the name of the pane where the module is placed.
        /// </summary>
        public string PanaName { get; set; }

        /// <summary>
        /// Gets or sets the source path of the container control.
        /// </summary>
        public string ContainerSrc { get; set; }

        /// <summary>
        /// Gets or sets the container path.
        /// </summary>
        public string ContainerPath { get; set; }

        /// <summary>
        /// Gets or sets the icon file for the module.
        /// </summary>
        public string IconFile { get; set; }
    }
}
