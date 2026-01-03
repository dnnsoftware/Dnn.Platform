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
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Registers a CSS resource.</summary>
    public class DnnCssInclude : ClientResourceInclude
    {
        private readonly IClientResourceController clientResourceController;

        /// <summary>Initializes a new instance of the <see cref="DnnCssInclude"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.1. Use overload with IClientResourceController. Scheduled removal in v12.0.0.")]
        public DnnCssInclude()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnCssInclude"/> class.</summary>
        /// <param name="clientResourceController">The client resources controller.</param>
        public DnnCssInclude(IClientResourceController clientResourceController)
        {
            this.clientResourceController = clientResourceController ?? DependencyInjection.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>();
            this.ForceProvider = ClientResourceProviders.DefaultCssProvider;
            this.DependencyType = ClientDependencyType.Css;
        }

        /// <inheritdoc cref="ILinkResource.Media" />
        public string CssMedia { get; set; }

        /// <inheritdoc cref="ILinkResource.Preload" />
        public bool Preload { get; set; }

        /// <inheritdoc/>
        protected override void OnLoad(EventArgs e)
        {
            var stylesheet = this.clientResourceController.CreateStylesheet(this.FilePath, this.PathNameAlias)
                .SetMedia(this.CssMedia);
            if (this.Preload)
            {
                stylesheet.SetPreload();
            }

            this.RegisterResource(stylesheet);
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
