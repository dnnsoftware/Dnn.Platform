// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Skins
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Web.MvcPipeline.Models;

    /// <summary>
    /// Skin helper methods for rendering meta tags into the page head.
    /// </summary>
    public static partial class SkinHelpers
    {
        /// <summary>
        /// Renders a meta tag with the specified attributes.
        /// </summary>
        /// <param name="helper">The HTML helper for the current <see cref="PageModel"/>.</param>
        /// <param name="name">The meta name attribute.</param>
        /// <param name="content">The meta content attribute.</param>
        /// <param name="httpEquiv">The meta http-equiv attribute.</param>
        /// <param name="insertFirst">Unused for MVC rendering; kept for API parity.</param>
        /// <returns>An HTML string representing the meta tag.</returns>
        public static IHtmlString Meta(this HtmlHelper<PageModel> helper, string name = "", string content = "", string httpEquiv = "", bool insertFirst = false)
        {
            var metaTag = new TagBuilder("meta");

            if (!string.IsNullOrEmpty(name))
            {
                metaTag.Attributes.Add("name", name);
            }

            if (!string.IsNullOrEmpty(content))
            {
                metaTag.Attributes.Add("content", content);
            }

            if (!string.IsNullOrEmpty(httpEquiv))
            {
                metaTag.Attributes.Add("http-equiv", httpEquiv);
            }

            return new MvcHtmlString(metaTag.ToString());
        }
    }
}
