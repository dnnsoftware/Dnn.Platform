// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace DotNetNuke.Web.Mvc.Helpers
{
    public static class HtmlEditorExtensions
    {
        public static MvcHtmlString EditorFor<TModel, TValue>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.EditorFor(expression);
        }

        public static MvcHtmlString EditorFor<TModel, TValue>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, object additionalViewData)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.EditorFor(expression, additionalViewData);
        }

        public static MvcHtmlString EditorFor<TModel, TValue>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string templateName)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.EditorFor(expression, templateName);
        }

        public static MvcHtmlString EditorFor<TModel, TValue>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string templateName, object additionalViewData)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.EditorFor(expression, templateName, additionalViewData);
        }

        public static MvcHtmlString EditorFor<TModel, TValue>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string templateName, string htmlFieldName)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.EditorFor(expression, templateName, htmlFieldName);
        }

        public static MvcHtmlString EditorFor<TModel, TValue>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string templateName, string htmlFieldName, object additionalViewData)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.EditorFor(expression, templateName, htmlFieldName, additionalViewData);
        }
    }
}
