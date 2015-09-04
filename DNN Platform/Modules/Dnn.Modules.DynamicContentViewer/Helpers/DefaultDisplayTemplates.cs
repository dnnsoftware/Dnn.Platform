// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


using System;
using System.Globalization;
using System.Web.Mvc;

namespace Dnn.Modules.DynamicContentViewer.Helpers
{
    internal class DefaultDisplayTemplates
    {
        internal static string BooleanTemplate(HtmlHelper html)
        {
            bool value = false;
            if (html.ViewContext.ViewData.Model != null)
            {
                value = Convert.ToBoolean(html.ViewContext.ViewData.Model, CultureInfo.InvariantCulture);
            }

            TagBuilder inputTag = new TagBuilder("input");
            inputTag.AddCssClass("check-box");
            inputTag.Attributes["disabled"] = "disabled";
            inputTag.Attributes["type"] = "checkbox";
            if (value)
            {
                inputTag.Attributes["checked"] = "checked";
            }

            return inputTag.ToString(TagRenderMode.SelfClosing);
        }

        internal static string BytesTemplate(HtmlHelper html)
        {
            var byteArray = html.ViewContext.ViewData.Model as byte[];
            return byteArray.ToHexString();
        }

        internal static string StringTemplate(HtmlHelper html)
        {
            return html.Encode(html.ViewContext.ViewData.Model);
        }

        internal static string UrlTemplate(HtmlHelper html)
        {
            return String.Format(CultureInfo.InvariantCulture,
                                 "<a href=\"{0}\">{1}</a>",
                                 html.AttributeEncode(html.ViewContext.ViewData.Model),
                                 html.Encode(html.ViewContext.ViewData.Model));
        }
    }
}
