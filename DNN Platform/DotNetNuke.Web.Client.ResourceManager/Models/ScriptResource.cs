// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ResourceManager.Models
{
    using DotNetNuke.Abstractions.ClientResources;

    /// <summary>
    /// Represents a script resource that can be registered and rendered in the client resources system.
    /// </summary>
    public class ScriptResource : ResourceBase, IScriptResource
    {
        private readonly IClientResourcesController clientResourcesController;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptResource"/> class.
        /// </summary>
        /// <param name="clientResourcesController">The client resources controller.</param>
        public ScriptResource(IClientResourcesController clientResourcesController)
        {
            this.clientResourcesController = clientResourcesController;
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
        public string Type { get; set; } = string.Empty;

        /// <inheritdoc />
        public new void Register()
        {
            this.clientResourcesController.AddScript(this);
        }

        /// <inheritdoc />
        public new string Render(int crmVersion, bool useCdn, string applicationPath)
        {
            var htmlString = "<script";
            htmlString += $" src=\"{this.GetVersionedPath(crmVersion, useCdn, applicationPath)}\"";
            if (this.Async)
            {
                htmlString += " async";
            }

            if (this.Defer)
            {
                htmlString += " defer";
            }

            if (this.NoModule)
            {
                htmlString += " nomodule";
            }

            if (!string.IsNullOrEmpty(this.Type))
            {
                htmlString += $" type=\"{this.Type}\"";
            }

            htmlString += this.RenderMimeType();
            htmlString += this.RenderBlocking();
            htmlString += this.RenderCrossOriginAttribute();
            htmlString += this.RenderFetchPriority();
            htmlString += this.RenderIntegrity();
            htmlString += this.RenderReferrerPolicy();
            htmlString += this.RenderAttributes();
            htmlString += " type=\"text/javascript\"></script>";
            return htmlString;
        }
    }
}
