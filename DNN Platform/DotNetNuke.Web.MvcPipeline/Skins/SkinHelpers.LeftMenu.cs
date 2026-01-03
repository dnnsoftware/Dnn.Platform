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
    /// Skin helper methods for legacy left menu functionality.
    /// </summary>
    public static partial class SkinHelpers
    {
        /// <summary>
        /// Returns an empty string because the legacy left menu skin object is deprecated.
        /// </summary>
        /// <param name="helper">The HTML helper for the current <see cref="PageModel"/>.</param>
        /// <returns>An empty HTML string.</returns>
        public static IHtmlString LeftMenu(this HtmlHelper<PageModel> helper)
        {
            return new MvcHtmlString(string.Empty); // LeftMenu is deprecated and should return an empty string.
        }
    }
}
