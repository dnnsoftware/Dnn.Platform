// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace DotNetNuke.Web.Mvc.Helpers
{
    public static class HtmlLabelExtensions
    {
        public static MvcHtmlString LabelFor<TModel, TValue>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.LabelFor(expression);
        }

        public static MvcHtmlString LabelFor<TModel, TValue>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string labelText)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.LabelFor(expression, labelText);
        }

        public static MvcHtmlString LabelFor<TModel, TValue>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, object htmlAttributes)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.LabelFor(expression, htmlAttributes);
        }

        public static MvcHtmlString LabelFor<TModel, TValue>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, IDictionary<string, object> htmlAttributes)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.LabelFor(expression, htmlAttributes);
        }

        public static MvcHtmlString LabelFor<TModel, TValue>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string labelText, object htmlAttributes)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.LabelFor(expression, labelText, htmlAttributes);
        }

        public static MvcHtmlString LabelFor<TModel, TValue>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string labelText, IDictionary<string, object> htmlAttributes)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.LabelFor(expression, labelText, htmlAttributes);
        }
    }
}
