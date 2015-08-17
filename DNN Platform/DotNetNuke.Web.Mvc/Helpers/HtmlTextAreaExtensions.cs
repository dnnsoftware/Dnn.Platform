// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace DotNetNuke.Web.Mvc.Helpers
{
    public static class HtmlTextAreaExtensions
    {
        private const int TextAreaRows = 2;
        private const int TextAreaColumns = 20;

        public static MvcHtmlString TextAreaFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.TextAreaFor(expression, TextAreaRows, TextAreaColumns, null);
        }

        public static MvcHtmlString TextAreaFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, object htmlAttributes)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.TextAreaFor(expression, TextAreaRows, TextAreaColumns, htmlAttributes);
        }

        public static MvcHtmlString TextAreaFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, IDictionary<string, object> htmlAttributes)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.TextAreaFor(expression, TextAreaRows, TextAreaColumns, htmlAttributes);
        }

        public static MvcHtmlString TextAreaFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, int rows, int columns, object htmlAttributes)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.TextAreaFor(expression, rows, columns, htmlAttributes);
        }

        public static MvcHtmlString TextAreaFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, int rows, int columns, IDictionary<string, object> htmlAttributes)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.TextAreaFor(expression, rows, columns, htmlAttributes);
        }

    }
}
