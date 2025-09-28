// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ResourceManager.Models
{
    using DotNetNuke.Abstractions.ClientResources;

    /// <summary>
    /// Represents a font resource that can be registered and rendered for client use.
    /// </summary>
    public class FontResource : ResourceBase, IFontResource
    {
        private readonly IClientResourcesController clientResourcesController;

        /// <summary>
        /// Initializes a new instance of the <see cref="FontResource"/> class.
        /// </summary>
        /// <param name="clientResourcesController">The client resources controller used to manage font resources.</param>
        public FontResource(IClientResourcesController clientResourcesController)
        {
            this.clientResourcesController = clientResourcesController;
            this.Provider = ClientResourceProviders.DnnPageHeaderProvider;
        }

        /// <inheritdoc />
        public new void Register()
        {
            this.clientResourcesController.AddFont(this);
        }

        /// <inheritdoc />
        public new string Render(int crmVersion, bool useCdn, string applicationPath)
        {
            var htmlString = "<link";
            htmlString += $" href=\"{this.GetVersionedPath(crmVersion, useCdn, applicationPath)}\"";
            if (this.Preload)
            {
                htmlString += $" rel=\"preload\" as=\"font\"";
            }
            else
            {
                htmlString += $" rel=\"font\"";
            }

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
