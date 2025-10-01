// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ResourceManager.Models
{
    using System.Text;

    using DotNetNuke.Abstractions.ClientResources;

    /// <summary>
    /// Represents a script resource that can be registered and rendered in the client resources system.
    /// </summary>
    public class ScriptResource : ResourceBase, IScriptResource
    {
        private readonly IClientResourceController clientResourceController;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptResource"/> class.
        /// </summary>
        /// <param name="clientResourceController">The client resources controller.</param>
        public ScriptResource(IClientResourceController clientResourceController)
        {
            this.clientResourceController = clientResourceController;
            this.Provider = ClientResourceProviders.DefaultJsProvider;
            this.Priority = (int)FileOrder.Js.DefaultPriority;
        }

        /// <inheritdoc />
        public bool Async { get; set; } = false;

        /// <inheritdoc />
        public bool Defer { get; set; } = false;

        /// <inheritdoc />
        public bool NoModule { get; set; } = false;

        /// <inheritdoc />
        public override void Register()
        {
            this.clientResourceController.AddScript(this);
        }

        /// <inheritdoc />
        public override string Render(int crmVersion, bool useCdn, string applicationPath)
        {
            var htmlString = new StringBuilder("<script");
            htmlString.Append($" src=\"{this.GetVersionedPath(crmVersion, useCdn, applicationPath)}\"");
            if (this.Async)
            {
                htmlString.Append(" async");
            }

            if (this.Defer)
            {
                htmlString.Append(" defer");
            }

            if (this.NoModule)
            {
                htmlString.Append(" nomodule");
            }

            this.RenderType(htmlString);
            this.RenderBlocking(htmlString);
            this.RenderCrossOriginAttribute(htmlString);
            this.RenderFetchPriority(htmlString);
            this.RenderIntegrity(htmlString);
            this.RenderReferrerPolicy(htmlString);
            this.RenderAttributes(htmlString);
            htmlString.Append(" type=\"text/javascript\"></script>");
            return htmlString.ToString();
        }
    }
}
