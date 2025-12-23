// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.ModuleControl.Razor
{
    using System.Web;
    using System.Web.Mvc;

    /// <summary>
    /// Holds contextual information required to render a Razor-based module view.
    /// </summary>
    public class RazorModuleViewContext
    {
        /// <summary>
        /// Gets or sets the current HTTP context.
        /// </summary>
        public HttpContextBase HttpContext { get; internal set; }

        /// <summary>
        /// Gets or sets the <see cref="ViewDataDictionary"/> used by the view.
        /// </summary>
        public ViewDataDictionary ViewData { get; internal set; }
    }
}
