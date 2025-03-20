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
        public static IHtmlString DnnLink(this HtmlHelper<PageModel> helper, string cssClass = "", string target = "")
        {
            var link = new TagBuilder("a");
            link.Attributes.Add("href", "http://www.dnnsoftware.com/community?utm_source=dnn-install&utm_medium=web-link&utm_content=gravity-skin-link&utm_campaign=dnn-install");
            link.Attributes.Add("class", cssClass);
            link.Attributes.Add("target", target);
            link.SetInnerText("CMS by DNN");

            return new MvcHtmlString(link.ToString());
        }
    }
}
