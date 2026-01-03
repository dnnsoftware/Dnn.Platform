// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.ModuleControl
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.UI.Modules;

    /// <summary>
    /// Defines the contract for MVC-based module controls in the DNN MVC pipeline.
    /// </summary>
    public interface IMvcModuleControl : IModuleControl
    {
        /// <summary>
        /// Renders the module control and returns the resulting HTML.
        /// </summary>
        /// <param name="htmlHelper">The MVC HTML helper.</param>
        /// <returns>An HTML string representing the rendered control.</returns>
        IHtmlString Html(HtmlHelper htmlHelper);
    }
}
