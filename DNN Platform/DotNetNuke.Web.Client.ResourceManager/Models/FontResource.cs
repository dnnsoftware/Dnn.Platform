// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ResourceManager.Models
{
    using System.Text;

    using DotNetNuke.Abstractions.ClientResources;

    /// <summary>
    /// Represents a font resource that can be registered and rendered for client use.
    /// </summary>
    public class FontResource : LinkResource, IFontResource
    {
        private readonly IClientResourceController clientResourceController;

        /// <summary>
        /// Initializes a new instance of the <see cref="FontResource"/> class.
        /// </summary>
        /// <param name="clientResourceController">The client resources controller used to manage font resources.</param>
        public FontResource(IClientResourceController clientResourceController)
        {
            this.clientResourceController = clientResourceController;
            this.Provider = ClientResourceProviders.DnnPageHeaderProvider;
        }

        /// <inheritdoc />
        public override void Register()
        {
            this.clientResourceController.AddFont(this);
        }

        /// <inheritdoc />
        public override string Render(int crmVersion, bool useCdn, string applicationPath)
        {
            var htmlString = new StringBuilder("<link");
            htmlString.Append($" href=\"{WebUtility.HtmlEncode(this.GetVersionedPath(crmVersion, useCdn, applicationPath))}\"");
            if (this.Preload)
            {
                htmlString.Append($" rel=\"preload\" as=\"font\"");
            }
            else
            {
                htmlString.Append($" rel=\"font\"");
            }

            this.RenderType(htmlString);
            this.RenderCrossOriginAttribute(htmlString);
            this.RenderFetchPriority(htmlString);
            this.RenderIntegrity(htmlString);
            this.RenderReferrerPolicy(htmlString);
            this.RenderAttributes(htmlString);
            htmlString.Append(" />");
            return htmlString.ToString();
        }
    }
}
