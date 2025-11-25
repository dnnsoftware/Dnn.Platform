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
        public static IHtmlString JavaScriptLibraryInclude(this HtmlHelper<PageModel> helper, string name, Version version, SpecificVersion? specificVersion)
        {
            var javaScript = Globals.GetCurrentServiceProvider().GetRequiredService<IJavaScriptLibraryHelper>();

            if (version == null)
            {
                javaScript.RequestRegistration(name);
            }
            else if (specificVersion == null)
            {
                javaScript.RequestRegistration(name, version);
            }
            else
            {
                javaScript.RequestRegistration(name, version, specificVersion.Value);
            }

            return new MvcHtmlString(string.Empty);
        }
    }
}
