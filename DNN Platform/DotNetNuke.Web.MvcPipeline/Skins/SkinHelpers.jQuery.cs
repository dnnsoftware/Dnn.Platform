// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Skins
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Web.MvcPipeline.Models;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Skin helper methods for requesting jQuery and related JavaScript libraries.
    /// </summary>
    public static partial class SkinHelpers
    {
        /// <summary>
        /// Requests registration of jQuery and optional related libraries for the current page.
        /// </summary>
        /// <param name="helper">The HTML helper for the current <see cref="PageModel"/>.</param>
        /// <param name="dnnjQueryPlugins">If set to <c>true</c>, requests the DNN jQuery plugins bundle.</param>
        /// <param name="jQueryHoverIntent">If set to <c>true</c>, requests the HoverIntent plugin.</param>
        /// <param name="jQueryUI">If set to <c>true</c>, requests jQuery UI.</param>
        /// <returns>An empty HTML string; scripts are registered via the JavaScript library helper.</returns>
        public static IHtmlString JQuery(this HtmlHelper<PageModel> helper, bool dnnjQueryPlugins = false, bool jQueryHoverIntent = false, bool jQueryUI = false)
        {
            var javaScript = HtmlHelpers.GetDependencyProvider(helper).GetRequiredService<IJavaScriptLibraryHelper>();

            javaScript.RequestRegistration(CommonJs.jQuery);
            javaScript.RequestRegistration(CommonJs.jQueryMigrate);

            if (jQueryUI)
            {
                javaScript.RequestRegistration(CommonJs.jQueryUI);
            }

            if (dnnjQueryPlugins)
            {
                javaScript.RequestRegistration(CommonJs.DnnPlugins);
            }

            if (jQueryHoverIntent)
            {
                javaScript.RequestRegistration(CommonJs.HoverIntent);
            }

            return new MvcHtmlString(string.Empty);
        }
    }
}
