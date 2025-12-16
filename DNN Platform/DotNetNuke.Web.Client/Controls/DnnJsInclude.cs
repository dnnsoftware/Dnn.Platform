// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ClientResourceManagement
{
    using System;
    using System.Web.UI;

    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Web.Client.Cdf;
    using DotNetNuke.Web.Client.ResourceManager;

    /// <summary>Registers a JavaScript resource.</summary>
    public class DnnJsInclude : ClientResourceInclude
    {
        private readonly IClientResourceController clientResourceController;

        /// <summary>
        /// Initializes a new instance of the <see cref="DnnJsInclude"/> class.
        /// Sets up default settings for the control.
        /// </summary>
        /// <param name="clientResourceController">The client resources controller.</param>
        public DnnJsInclude(IClientResourceController clientResourceController)
        {
            this.clientResourceController = clientResourceController;
            this.ForceProvider = ClientResourceProviders.DefaultJsProvider;
            this.DependencyType = ClientDependencyType.Javascript;
        }

        /// <inheritdoc cref="IScriptResource.Async" />
        public bool Async { get; set; }

        /// <inheritdoc cref="IScriptResource.Defer" />
        public bool Defer { get; set; }

        /// <inheritdoc cref="IScriptResource.NoModule" />
        public bool NoModule { get; set; }

        /// <inheritdoc/>
        protected override void OnLoad(System.EventArgs e)
        {
            var script = this.clientResourceController.CreateScript(this.FilePath, this.PathNameAlias);
            if (this.Async)
            {
                script = script.SetAsync();
            }

            if (this.Defer)
            {
                script = script.SetDefer();
            }

            if (this.NoModule)
            {
                script = script.SetNoModule();
            }

            this.RegisterResource(script);
            base.OnLoad(e);
        }

        /// <inheritdoc/>
        protected override void Render(HtmlTextWriter writer)
        {
            if (this.AddTag || this.Context.IsDebuggingEnabled)
            {
                writer.Write("<!--CDF(Javascript|{0}|{1}|{2})-->", this.FilePath, this.ForceProvider, this.Priority);
            }
        }
    }
}
