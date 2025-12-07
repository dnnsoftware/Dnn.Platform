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
