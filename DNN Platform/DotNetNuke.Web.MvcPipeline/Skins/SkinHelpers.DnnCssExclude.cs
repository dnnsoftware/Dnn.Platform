// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Skins
{
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Web.MvcPipeline.Models;

    /// <summary>
    /// Skin helper methods for excluding previously registered CSS resources.
    /// </summary>
    public static partial class SkinHelpers
    {
        /// <summary>
        /// Excludes a previously registered stylesheet by its logical name.
        /// </summary>
        /// <param name="helper">The HTML helper for the current <see cref="PageModel"/>.</param>
        /// <param name="name">The logical stylesheet name to remove.</param>
        /// <returns>An empty HTML string.</returns>
        public static IHtmlString DnnCssExclude(this HtmlHelper<PageModel> helper, string name)
        {
            HtmlHelpers.GetClientResourcesController(helper)
                 .RemoveStylesheetByName(name);
            return new MvcHtmlString(string.Empty);
        }
    }
}
