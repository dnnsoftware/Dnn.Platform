// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ClientResourceManagement
{
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.ClientResources;

    /// <summary>Renders client resources.</summary>
    public class DnnResources : Literal
    {
        private readonly IClientResourceController clientResourceController;

        /// <summary>Initializes a new instance of the <see cref="DnnResources"/> class.</summary>
        /// <param name="clientResourceController">The client resource controller.</param>
        public DnnResources(IClientResourceController clientResourceController)
        {
            this.clientResourceController = clientResourceController;
        }

        /// <summary>Gets or sets the application path.</summary>
        public string ApplicationPath { get; set; }

        /// <summary>Gets or sets the provider.</summary>
        public string Provider { get; set; }

        /// <inheritdoc />
        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);
            writer.Write(this.clientResourceController.RenderDependencies(ResourceType.All, this.Provider, this.ApplicationPath));
        }
    }
}
