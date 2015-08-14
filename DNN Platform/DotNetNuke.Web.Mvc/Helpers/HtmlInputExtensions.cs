// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace DotNetNuke.Web.Mvc.Helpers
{
    public static class HtmlInputExtensions
    {
        // CheckBox

        public static MvcHtmlString CheckBoxFor<TModel>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, bool>> expression)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.CheckBoxFor(expression);
        }

        public static MvcHtmlString CheckBoxFor<TModel>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, bool>> expression, object htmlAttributes)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.CheckBoxFor(expression, htmlAttributes);
        }

        public static MvcHtmlString CheckBoxFor<TModel>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, bool>> expression, IDictionary<string, object> htmlAttributes)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.CheckBoxFor(expression, htmlAttributes);
        }

        // Hidden

        public static MvcHtmlString HiddenFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.HiddenFor(expression);
        }

        public static MvcHtmlString HiddenFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, object htmlAttributes)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.HiddenFor(expression, htmlAttributes);
        }

        public static MvcHtmlString HiddenFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, IDictionary<string, object> htmlAttributes)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.HiddenFor(expression, htmlAttributes);
        }

        // Password

        public static MvcHtmlString PasswordFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.PasswordFor(expression);
        }

        public static MvcHtmlString PasswordFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, object htmlAttributes)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.PasswordFor(expression, htmlAttributes);
        }

        public static MvcHtmlString PasswordFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, IDictionary<string, object> htmlAttributes)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.PasswordFor(expression, htmlAttributes);
        }

        // RadioButton

        public static MvcHtmlString RadioButtonFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, object value)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.RadioButtonFor(expression, value);
        }

        public static MvcHtmlString RadioButtonFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, object value, object htmlAttributes)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.RadioButtonFor(expression, value, htmlAttributes);
        }

        public static MvcHtmlString RadioButtonFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, object value, IDictionary<string, object> htmlAttributes)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.RadioButtonFor(expression, value, htmlAttributes);
        }

        // TextBox

        public static MvcHtmlString TextBoxFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.TextBoxFor(expression);
        }

        public static MvcHtmlString TextBoxFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, string format)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.TextBoxFor(expression, format);
        }

        public static MvcHtmlString TextBoxFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, object htmlAttributes)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.TextBoxFor(expression, htmlAttributes);
        }

        public static MvcHtmlString TextBoxFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, string format, object htmlAttributes)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.TextBoxFor(expression, format, htmlAttributes);
        }

        public static MvcHtmlString TextBoxFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, IDictionary<string, object> htmlAttributes)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.TextBoxFor(expression, htmlAttributes);
        }

        public static MvcHtmlString TextBoxFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, string format, IDictionary<string, object> htmlAttributes)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.TextBoxFor(expression, format, htmlAttributes);
        }
    }
}
