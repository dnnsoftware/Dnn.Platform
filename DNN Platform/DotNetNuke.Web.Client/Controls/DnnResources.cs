// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ClientResourceManagement
{
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.ClientResources;

    public class DnnResources : Literal
    {
        private readonly IClientResourcesController clientResourcesController;

        public DnnResources(IClientResourcesController clientResourcesController)
        {
            this.clientResourcesController = clientResourcesController;
        }

        public string ApplicationPath { get; set; }

        public string Provider { get; set; }

        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);
            writer.Write(this.clientResourcesController.RenderDependencies(ResourceType.All, this.Provider, this.ApplicationPath));
        }
    }
}
