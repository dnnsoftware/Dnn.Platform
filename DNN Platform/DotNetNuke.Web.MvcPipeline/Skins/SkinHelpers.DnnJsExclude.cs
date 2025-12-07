// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Skins
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Web.MvcPipeline.Models;

    public static partial class SkinHelpers
    {
        public static IHtmlString DnnJsExclude(this HtmlHelper<PageModel> helper, string name)
        {
            HtmlHelpers.GetClientResourcesController(helper)
                .RemoveScriptByName(name);
            return new MvcHtmlString(string.Empty);
        }
    }
}
