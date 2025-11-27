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
            if (!string.IsNullOrEmpty(cssClass))
            {
                link.AddCssClass(cssClass);
            }

            if (!string.IsNullOrEmpty(target))
            {
                link.Attributes.Add("target", target);
            }

            // set home page link to community URL
            string url = "http://www.dnnsoftware.com/community?utm_source=dnn-install&utm_medium=web-link&utm_content=gravity-skin-link&utm_campaign=dnn-install";
            string utmTerm = "&utm_term=cms-by-dnn";
            string hostName = helper.ViewContext.HttpContext.Request.Url.Host.ToLowerInvariant().Replace("www.", string.Empty);
            int charPos = 0;
            string linkText = "CMS by DNN";
            if (hostName.Length > 0)
            {
                // convert first letter of hostname to int pos in alphabet
                charPos = char.ToUpper(hostName[0]) - 64;
            }

            // vary link by first letter of host name
            if (charPos <= 5)
            {
                linkText = "Open Source ASP.NET CMS by DNN";
                utmTerm = "&utm_term=open+source+asp.net+by+dnn";
            }

            if (charPos > 5 && charPos <= 10)
            {
                linkText = "DNN - .NET Open Source CMS";
                utmTerm = "&utm_term=dnn+.net+open+source+cms";
            }

            if (charPos > 10 && charPos <= 15)
            {
                linkText = "Web Content Management by DNN";
                utmTerm = "&utm_term=web+content+management+by+dnn";
            }

            if (charPos > 15 && charPos <= 20)
            {
                linkText = "DNN .NET CMS";
                utmTerm = "&utm_term=dnn+.net+cms";
            }

            if (charPos > 20 && charPos <= 25)
            {
                linkText = "WCM by DNN";
                utmTerm = "&utm_term=wcm+by+dnn";
            }

            link.SetInnerText(linkText);
            link.Attributes.Add("href", HttpUtility.HtmlEncode(url + utmTerm));

            return new MvcHtmlString(link.ToString());
        }
    }
}
