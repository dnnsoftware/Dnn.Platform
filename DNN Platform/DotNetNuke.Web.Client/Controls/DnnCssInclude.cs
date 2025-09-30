// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ClientResourceManagement
{
    using System.Web.UI;

    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Web.Client.Cdf;

    /// <summary>Registers a CSS resource.</summary>
    public class DnnCssInclude : ClientResourceInclude
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DnnCssInclude"/> class.
        /// </summary>
        /// <param name="clientResourceController">The client resources controller.</param>
        public DnnCssInclude(IClientResourceController clientResourceController)
            : base(clientResourceController)
        {
            this.ForceProvider = ClientResourceProviders.DefaultCssProvider;
            this.DependencyType = ClientDependencyType.Css;
        }

        /// <inheritdoc/>
        protected override void OnLoad(System.EventArgs e)
        {
            this.PathNameAlias = string.IsNullOrEmpty(this.PathNameAlias) ? string.Empty : this.PathNameAlias.ToLowerInvariant();
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
