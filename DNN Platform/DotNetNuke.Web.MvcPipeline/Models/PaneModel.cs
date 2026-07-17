// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.MvcPipeline.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a pane within a skin, containing one or more module containers.
    /// </summary>
    public class PaneModel
    {
        private Dictionary<string, ContainerModel> containers;

        /// <summary>
        /// Initializes a new instance of the <see cref="PaneModel"/> class.
        /// </summary>
        /// <param name="name">The name of the pane.</param>
        public PaneModel(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Gets or sets the CSS class applied to the pane.
        /// </summary>
        public string CssClass { get; set; }

        /// <summary>
        /// Gets the collection of containers hosted within the pane.
        /// </summary>
        public Dictionary<string, ContainerModel> Containers
        {
            get
            {
                return this.containers ?? (this.containers = new Dictionary<string, ContainerModel>());
            }
        }

        /// <summary>Gets or sets the name (ID) of the Pane.</summary>
        public string Name { get; set; }
    }
}
