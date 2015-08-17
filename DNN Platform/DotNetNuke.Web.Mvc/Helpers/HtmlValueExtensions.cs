// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace DotNetNuke.Web.Mvc.Helpers
{
    public static class HtmlValueExtensions
    {
        public static MvcHtmlString ValueFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.ValueFor(expression);
        }

        public static MvcHtmlString ValueFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, string format)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.ValueFor(expression, format);
        }
    }
}
