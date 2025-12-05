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

    /// <summary>Registers a CSS resource.</summary>
    public class DnnCssInclude : ClientResourceInclude
    {
        private readonly IClientResourceController clientResourceController;

        /// <summary>
        /// Initializes a new instance of the <see cref="DnnCssInclude"/> class.
        /// </summary>
        /// <param name="clientResourceController">The client resources controller.</param>
        public DnnCssInclude(IClientResourceController clientResourceController)
            : base()
        {
            this.clientResourceController = clientResourceController;
            this.ForceProvider = ClientResourceProviders.DefaultCssProvider;
            this.DependencyType = ClientDependencyType.Css;
        }

        public string CssMedia { get; set; }

        /// <inheritdoc/>
        protected override void OnInit(EventArgs e)
        {
        }

        /// <inheritdoc/>
        protected override void OnLoad(System.EventArgs e)
        {
            this.clientResourceController.CreateStylesheet(this.FilePath, this.PathNameAlias)
                        .SetNameAndVersion(this.Name, this.Version, this.ForceVersion)
                        .SetProvider(this.ForceProvider)
                        .SetPriority(this.Priority)
                        .SetMedia(this.CssMedia)
                        .Register();
            base.OnLoad(e);
        }

        /// <inheritdoc/>
        protected override void Render(HtmlTextWriter writer)
        {
            if (this.AddTag || this.Context.IsDebuggingEnabled)
            {
                writer.Write("<!--CDF(Css|{0}|{1}|{2})-->", this.FilePath, this.ForceProvider, this.Priority);
            }
        }
    }
}
