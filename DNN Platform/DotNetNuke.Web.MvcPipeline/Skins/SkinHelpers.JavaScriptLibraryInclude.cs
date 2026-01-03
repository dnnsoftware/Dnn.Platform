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
    /// Skin helper methods for including registered JavaScript libraries by name and version.
    /// </summary>
    public static partial class SkinHelpers
    {
        /// <summary>
        /// Registers a JavaScript library by name and optional version using the DNN JavaScript library helper.
        /// </summary>
        /// <param name="helper">The HTML helper for the current <see cref="PageModel"/>.</param>
        /// <param name="name">The logical name of the JavaScript library.</param>
        /// <param name="version">Optional version string (for example, "1.2.3").</param>
        /// <param name="specificVersion">Optional specific version selector (for example, "Latest", "Exact").</param>
        /// <returns>An empty HTML string; the library is registered via the JavaScript library helper.</returns>
        public static IHtmlString JavaScriptLibraryInclude(this HtmlHelper<PageModel> helper, string name, string version = null, string specificVersion = null)
        {
            var javaScript = HtmlHelpers.GetDependencyProvider(helper).GetRequiredService<IJavaScriptLibraryHelper>();
            SpecificVersion specificVer;
            if (version == null)
            {
                javaScript.RequestRegistration(name);
            }
            else if (!Enum.TryParse(specificVersion, true, out specificVer))
            {
                javaScript.RequestRegistration(name, new Version(version));
            }
            else
            {
                javaScript.RequestRegistration(name, new Version(version), specificVer);
            }

            return new MvcHtmlString(string.Empty);
        }
    }
}
