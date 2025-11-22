// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Skins
{
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Web.MvcPipeline.Models;

    public static partial class SkinHelpers
    {
        public static IHtmlString DnnCssExclude(this HtmlHelper<PageModel> helper, string name)
        {
            GetClientResourcesController()
                 .RemoveScriptByName(name);
            return new MvcHtmlString(string.Empty);
        }
    }
}
