// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Web.UI;

namespace DotNetNuke.Web.Client.ClientResourceManagement
{
    using ClientDependency.Core.Controls;

    /// <summary>
    /// Registers a JavaScript resource
    /// </summary>
    public class DnnJsInclude : JsInclude
    {
        /// <summary>
        /// Sets up default settings for the control
        /// </summary>
        public DnnJsInclude()
        {
            ForceProvider = ClientResourceManager.DefaultJsProvider;
        }

        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);

            this.PathNameAlias = this.PathNameAlias.ToLowerInvariant();
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (AddTag || Context.IsDebuggingEnabled)
            {
                writer.Write("<!--CDF({0}|{1}|{2}|{3})-->", DependencyType, FilePath, ForceProvider, Priority);
            }
        }
    }
}
