// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.MvcPipeline.Models
{
    using System.Collections.Generic;

    public class PaneModel
    {
        private Dictionary<string, ContainerModel> containers;

        public PaneModel(string name)
        {
            this.Name = name;
        }

        public string CssClass { get; set; }

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
