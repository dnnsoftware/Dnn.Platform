// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.ModuleControl.Razor
{
    using System.Web;
    using System.Web.Mvc;

    /// <summary>
    /// Razor result that renders a raw HTML content string.
    /// </summary>
    public class ContentRazorModuleResult : IRazorModuleResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentRazorModuleResult"/> class.
        /// </summary>
        /// <param name="content">The raw HTML content to render.</param>
        public ContentRazorModuleResult(string content)
        {
            this.Content = content;
        }

        /// <summary>
        /// Gets the raw HTML content to render.
        /// </summary>
        public string Content { get; private set; }

        /// <inheritdoc/>
        public IHtmlString Execute(HtmlHelper htmlHelper)
        {
            return new MvcHtmlString(this.Content);
        }
    }
}
