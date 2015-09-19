// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

namespace Dnn.Modules.DynamicContentViewer.Helpers
{
    internal class DefaultEditorTemplates
    {
        private const string HtmlAttributeKey = "htmlAttributes";

        private static IDictionary<string, object> CreateHtmlAttributes(HtmlHelper html, string className, string inputType = null)
        {
            object htmlAttributesObject = html.ViewContext.ViewData[HtmlAttributeKey];
            if (htmlAttributesObject != null)
            {
                return MergeHtmlAttributes(htmlAttributesObject, className, inputType);
            }

            var htmlAttributes = new Dictionary<string, object>()
            {
                { "class", className }
            };
            if (inputType != null)
            {
                htmlAttributes.Add("type", inputType);
            }
            return htmlAttributes;
        }

        private static string HtmlInputTemplateHelper(HtmlHelper html, string inputType = null)
        {
            return HtmlInputTemplateHelper(html, inputType, html.ViewContext.ViewData.TemplateInfo.FormattedModelValue);
        }

        private static string HtmlInputTemplateHelper(HtmlHelper html, string inputType, object value)
        {
            return html.TextBox(
                    name: String.Empty,
                    value: value,
                    htmlAttributes: CreateHtmlAttributes(html, className: "text-box single-line", inputType: inputType))
                .ToHtmlString();
        }

        private static IDictionary<string, object> MergeHtmlAttributes(object htmlAttributesObject, string className, string inputType)
        {
            RouteValueDictionary htmlAttributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributesObject);
            object htmlClassName;
            if (htmlAttributes.TryGetValue("class", out htmlClassName))
            {
                htmlClassName += " " + className;
                htmlAttributes["class"] = htmlClassName;
            }
            else
            {
                htmlAttributes.Add("class", className);
            }

            // The input type from the provided htmlAttributes overrides the inputType parameter.
            if (inputType != null && !htmlAttributes.ContainsKey("type"))
            {
                htmlAttributes.Add("type", inputType);
            }

            return htmlAttributes;
        }

        internal static string StringInputTemplate(HtmlHelper html)
        {
            return HtmlInputTemplateHelper(html);
        }

        internal static string UrlInputTemplate(HtmlHelper html)
        {
            return HtmlInputTemplateHelper(html, inputType: "url");
        }


    }
}
