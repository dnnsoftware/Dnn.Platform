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
    /// Skin helper methods for excluding previously registered JavaScript resources.
    /// </summary>
    public static partial class SkinHelpers
    {
        /// <summary>
        /// Excludes a previously registered JavaScript resource by its logical name.
        /// </summary>
        /// <param name="helper">The HTML helper for the current <see cref="PageModel"/>.</param>
        /// <param name="name">The logical script name to remove.</param>
        /// <returns>An empty HTML string.</returns>
        public static IHtmlString DnnJsExclude(this HtmlHelper<PageModel> helper, string name)
        {
            HtmlHelpers.GetClientResourcesController(helper)
                .RemoveScriptByName(name);
            return new MvcHtmlString(string.Empty);
        }
    }
}
