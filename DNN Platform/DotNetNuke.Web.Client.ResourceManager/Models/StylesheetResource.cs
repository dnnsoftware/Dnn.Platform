// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ResourceManager.Models
{
    using DotNetNuke.Abstractions.ClientResources;

    /// <summary>
    /// Represents a stylesheet resource that can be registered and rendered in the client resources controller.
    /// </summary>
    public class StylesheetResource : LinkResource, IStylesheetResource
    {
        private readonly IClientResourcesController clientResourcesController;

        /// <summary>
        /// Initializes a new instance of the <see cref="StylesheetResource"/> class.
        /// </summary>
        /// <param name="clientResourcesController">The client resources controller used to register the stylesheet.</param>
        public StylesheetResource(IClientResourcesController clientResourcesController)
        {
            this.clientResourcesController = clientResourcesController;
            this.Provider = ClientResourceProviders.DefaultCssProvider;
            this.Priority = (int)FileOrder.Css.DefaultPriority;
        }

        /// <inheritdoc />
        public bool Disabled { get; set; } = false;

        /// <inheritdoc />
        public new void Register()
        {
            this.clientResourcesController.AddStylesheet(this);
        }

        /// <inheritdoc />
        public new string Render(int crmVersion, bool useCdn, string applicationPath)
        {
            var htmlString = "<link";
            htmlString += $" href=\"{this.GetVersionedPath(crmVersion, useCdn, applicationPath)}\"";
            if (this.Preload)
            {
                htmlString += $" rel=\"preload\" as=\"style\"";
            }
            else
            {
                htmlString += $" rel=\"stylesheet\"";
            }

            if (this.Disabled)
            {
                htmlString += " disabled";
            }

            htmlString += this.RenderBlocking();
            htmlString += this.RenderCrossOriginAttribute();
            htmlString += this.RenderFetchPriority();
            htmlString += this.RenderIntegrity();
            htmlString += this.RenderReferrerPolicy();
            htmlString += this.RenderAttributes();
            htmlString += " />";
            return htmlString;
        }
    }
}
