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
        public static IHtmlString Meta(this HtmlHelper<PageModel> helper, string name = "", string content = "", string httpEquiv = "", bool insertFirst = false)
        {
            var metaTag = new TagBuilder("meta");

            if (!string.IsNullOrEmpty(name))
            {
                metaTag.Attributes.Add("name", name);
            }

            if (!string.IsNullOrEmpty(content))
            {
                metaTag.Attributes.Add("content", content);
            }

            if (!string.IsNullOrEmpty(httpEquiv))
            {
                metaTag.Attributes.Add("http-equiv", httpEquiv);
            }

            return new MvcHtmlString(metaTag.ToString());
        }
    }
}
