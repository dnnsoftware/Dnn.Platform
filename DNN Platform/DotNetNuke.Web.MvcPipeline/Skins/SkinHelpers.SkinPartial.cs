// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Skins
{
    using System;
    using System.IO;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;
    using System.Web.WebPages.Html;

    using DotNetNuke.Web.MvcPipeline.Models;

    /// <summary>
    /// Skin helper methods for rendering reusable Razor skin partials.
    /// </summary>
    public static partial class SkinHelpers
    {
        /// <summary>
        /// Renders a Razor partial view located under the current skin's <c>Views</c> folder.
        /// </summary>
        /// <param name="helper">The HTML helper for the current <see cref="PageModel"/>.</param>
        /// <param name="name">The partial view name (without path or extension).</param>
        /// <returns>An HTML string containing the rendered partial view.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the page model is not available.</exception>
        public static IHtmlString SkinPartial(this HtmlHelper<PageModel> helper, string name = "")
        {
            var model = helper.ViewData.Model;
            if (model == null)
            {
                throw new InvalidOperationException("The model need to be present.");
            }

            var skinPath = Path.GetDirectoryName(model.Skin.SkinSrc);
            return helper.Partial("~" + skinPath + "/Views/" + name + ".cshtml");
        }
    }
}
