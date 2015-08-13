// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace DotNetNuke.Web.Mvc.Helpers
{
    public static class HtmlValidationExtensions
    {
        // Validate

        public static void ValidateFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            htmlHelper.ValidateFor(expression);
        }

        // ValidationMessage

        public static MvcHtmlString ValidationMessageFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.ValidationMessageFor(expression);
        }

        public static MvcHtmlString ValidationMessageFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, string validationMessage)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.ValidationMessageFor(expression, validationMessage);
        }

        public static MvcHtmlString ValidationMessageFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, string validationMessage, object htmlAttributes)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.ValidationMessageFor(expression, validationMessage, htmlAttributes);
        }

        public static MvcHtmlString ValidationMessageFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, string validationMessage, IDictionary<string, object> htmlAttributes)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.ValidationMessageFor(expression, validationMessage, htmlAttributes);
        }

        // ValidationSummary

        public static MvcHtmlString ValidationSummary(this DnnHtmlHelper html)
        {
            return html.HtmlHelper.ValidationSummary();
        }

        public static MvcHtmlString ValidationSummary(this DnnHtmlHelper html, bool excludePropertyErrors)
        {
            return html.HtmlHelper.ValidationSummary(excludePropertyErrors);
        }

        public static MvcHtmlString ValidationSummary(this DnnHtmlHelper html, string message)
        {
            return html.HtmlHelper.ValidationSummary(message);
        }

        public static MvcHtmlString ValidationSummary(this DnnHtmlHelper html, bool excludePropertyErrors, string message)
        {
            return html.HtmlHelper.ValidationSummary(excludePropertyErrors, message);
        }

        public static MvcHtmlString ValidationSummary(this DnnHtmlHelper html, string message, object htmlAttributes)
        {
            return html.HtmlHelper.ValidationSummary(message, htmlAttributes);
        }

        public static MvcHtmlString ValidationSummary(this DnnHtmlHelper html, bool excludePropertyErrors, string message, object htmlAttributes)
        {
            return html.HtmlHelper.ValidationSummary(excludePropertyErrors, message, htmlAttributes);
        }

        public static MvcHtmlString ValidationSummary(this DnnHtmlHelper html, string message, IDictionary<string, object> htmlAttributes)
        {
            return html.HtmlHelper.ValidationSummary(message, htmlAttributes);
        }

        public static MvcHtmlString ValidationSummary(this DnnHtmlHelper html, bool excludePropertyErrors, string message, IDictionary<string, object> htmlAttributes)
        {
            return html.HtmlHelper.ValidationSummary(excludePropertyErrors, message, htmlAttributes);
        }
    }
}
