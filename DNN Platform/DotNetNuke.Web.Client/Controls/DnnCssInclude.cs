// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ClientResourceManagement
{
    using System.Web.UI;

    using ClientDependency.Core.Controls;

    /// <summary>
    /// Registers a CSS resource.
    /// </summary>
    public class DnnCssInclude : CssInclude
    {
        public DnnCssInclude()
        {
            this.ForceProvider = ClientResourceManager.DefaultCssProvider;
        }

        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);

            this.PathNameAlias = this.PathNameAlias.ToLowerInvariant();
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (this.AddTag || this.Context.IsDebuggingEnabled)
            {
                writer.Write("<!--CDF({0}|{1}|{2}|{3})-->", this.DependencyType, this.FilePath, this.ForceProvider, this.Priority);
            }
        }
    }
}
