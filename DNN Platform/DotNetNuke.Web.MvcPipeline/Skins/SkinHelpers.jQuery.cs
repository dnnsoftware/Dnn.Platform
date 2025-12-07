// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Skins
{
    using System;
    using System.Web;
    using System.Web.Mvc;
    using DotNetNuke.Common;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Web.MvcPipeline.Models;
    using Microsoft.Extensions.DependencyInjection;

    public static partial class SkinHelpers
    {
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
