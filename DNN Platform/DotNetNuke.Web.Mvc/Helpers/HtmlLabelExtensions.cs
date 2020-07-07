// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;

    /// <summary>
    /// Represents support for the HTML label element in an ASP.NET MVC view.
    /// </summary>
    public static class HtmlLabelExtensions
    {
        /// <summary>
        /// Returns an HTML label element and the property name of the property that is represented by the specified expression.
        /// </summary>
        ///
        /// <returns>
        /// An HTML label element and the property name of the property that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the property to display.</param>
        public static MvcHtmlString Label(this DnnHtmlHelper html, string expression)
        {
            return html.HtmlHelper.Label(expression);
        }

        /// <summary>
        /// Returns an HTML label element and the property name of the property that is represented by the specified expression using the label text.
        /// </summary>
        ///
        /// <returns>
        /// An HTML label element and the property name of the property that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the property to display.</param><param name="labelText">The label text to display.</param>
        public static MvcHtmlString Label(this DnnHtmlHelper html, string expression, string labelText)
        {
            return html.HtmlHelper.Label(expression, labelText);
        }

        /// <summary>
        /// Returns an HTML label element and the property name of the property that is represented by the specified expression.
        /// </summary>
        ///
        /// <returns>
        /// An HTML label element and the property name of the property that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the property to display.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param>
        public static MvcHtmlString Label(this DnnHtmlHelper html, string expression, object htmlAttributes)
        {
            return html.HtmlHelper.Label(expression, htmlAttributes);
        }

        /// <summary>
        /// Returns an HTML label element and the property name of the property that is represented by the specified expression.
        /// </summary>
        ///
        /// <returns>
        /// An HTML label element and the property name of the property that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the property to display.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param>
        public static MvcHtmlString Label(this DnnHtmlHelper html, string expression, IDictionary<string, object> htmlAttributes)
        {
            return html.HtmlHelper.Label(expression, htmlAttributes);
        }

        /// <summary>
        /// Returns an HTML label element and the property name of the property that is represented by the specified expression.
        /// </summary>
        ///
        /// <returns>
        /// An HTML label element and the property name of the property that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the property to display.</param><param name="labelText">The label text.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param>
        public static MvcHtmlString Label(this DnnHtmlHelper html, string expression, string labelText, object htmlAttributes)
        {
            return html.HtmlHelper.Label(expression, labelText, htmlAttributes);
        }

        /// <summary>
        /// Returns an HTML label element and the property name of the property that is represented by the specified expression.
        /// </summary>
        ///
        /// <returns>
        /// An HTML label element and the property name of the property that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the property to display.</param><param name="labelText">The label text.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param>
        public static MvcHtmlString Label(this DnnHtmlHelper html, string expression, string labelText, IDictionary<string, object> htmlAttributes)
        {
            return html.HtmlHelper.Label(expression, labelText, htmlAttributes);
        }

        /// <summary>
        /// Returns an HTML label element and the property name of the property that is represented by the specified expression.
        /// </summary>
        ///
        /// <returns>
        /// An HTML label element and the property name of the property that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the property to display.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TValue">The type of the value.</typeparam>
        public static MvcHtmlString LabelFor<TModel, TValue>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return html.HtmlHelper.LabelFor(expression);
        }

        /// <summary>
        /// Returns an HTML label element and the property name of the property that is represented by the specified expression using the label text.
        /// </summary>
        ///
        /// <returns>
        /// An HTML label element and the property name of the property that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the property to display.</param><param name="labelText">The label text to display.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TValue">The type of the value.</typeparam>
        public static MvcHtmlString LabelFor<TModel, TValue>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string labelText)
        {
            return html.HtmlHelper.LabelFor(expression, labelText);
        }

        /// <summary>
        /// Returns an HTML label element and the property name of the property that is represented by the specified expression.
        /// </summary>
        ///
        /// <returns>
        /// An HTML label element and the property name of the property that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the property to display.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TValue">The value.</typeparam>
        public static MvcHtmlString LabelFor<TModel, TValue>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, object htmlAttributes)
        {
            return html.HtmlHelper.LabelFor(expression, htmlAttributes);
        }

        /// <summary>
        /// Returns an HTML label element and the property name of the property that is represented by the specified expression.
        /// </summary>
        ///
        /// <returns>
        /// An HTML label element and the property name of the property that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the property to display.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TValue">The type of the value.</typeparam>
        public static MvcHtmlString LabelFor<TModel, TValue>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, IDictionary<string, object> htmlAttributes)
        {
            return html.HtmlHelper.LabelFor(expression, htmlAttributes);
        }

        /// <summary>
        /// Returns an HTML label element and the property name of the property that is represented by the specified expression.
        /// </summary>
        ///
        /// <returns>
        /// An HTML label element and the property name of the property that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the property to display.</param><param name="labelText">The label text.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TValue">The Value.</typeparam>
        public static MvcHtmlString LabelFor<TModel, TValue>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string labelText, object htmlAttributes)
        {
            return html.HtmlHelper.LabelFor(expression, labelText, htmlAttributes);
        }

        /// <summary>
        /// Returns an HTML label element and the property name of the property that is represented by the specified expression.
        /// </summary>
        ///
        /// <returns>
        /// An HTML label element and the property name of the property that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the property to display.</param><param name="labelText">The label text to display.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TValue">The type of the value.</typeparam>
        public static MvcHtmlString LabelFor<TModel, TValue>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string labelText, IDictionary<string, object> htmlAttributes)
        {
            return html.HtmlHelper.LabelFor(expression, labelText, htmlAttributes);
        }

        /// <summary>
        /// Returns an HTML label element and the property name of the property that is represented by the model.
        /// </summary>
        ///
        /// <returns>
        /// An HTML label element and the property name of the property that is represented by the model.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param>
        public static MvcHtmlString LabelForModel(this DnnHtmlHelper html)
        {
            return html.HtmlHelper.LabelForModel();
        }

        /// <summary>
        /// Returns an HTML label element and the property name of the property that is represented by the specified expression using the label text.
        /// </summary>
        ///
        /// <returns>
        /// An HTML label element and the property name of the property that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="labelText">The label text to display.</param>
        public static MvcHtmlString LabelForModel(this DnnHtmlHelper html, string labelText)
        {
            return html.HtmlHelper.LabelForModel(labelText);
        }

        /// <summary>
        /// Returns an HTML label element and the property name of the property that is represented by the specified expression.
        /// </summary>
        ///
        /// <returns>
        /// An HTML label element and the property name of the property that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param>
        public static MvcHtmlString LabelForModel(this DnnHtmlHelper html, object htmlAttributes)
        {
            return html.HtmlHelper.LabelForModel(htmlAttributes);
        }

        /// <summary>
        /// Returns an HTML label element and the property name of the property that is represented by the specified expression.
        /// </summary>
        ///
        /// <returns>
        /// An HTML label element and the property name of the property that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param>
        public static MvcHtmlString LabelForModel(this DnnHtmlHelper html, IDictionary<string, object> htmlAttributes)
        {
            return html.HtmlHelper.LabelForModel(htmlAttributes);
        }

        /// <summary>
        /// Returns an HTML label element and the property name of the property that is represented by the specified expression.
        /// </summary>
        ///
        /// <returns>
        /// An HTML label element and the property name of the property that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="labelText">The label text.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param>
        public static MvcHtmlString LabelForModel(this DnnHtmlHelper html, string labelText, object htmlAttributes)
        {
            return html.HtmlHelper.LabelForModel(labelText, htmlAttributes);
        }

        /// <summary>
        /// Returns an HTML label element and the property name of the property that is represented by the specified expression.
        /// </summary>
        ///
        /// <returns>
        /// An HTML label element and the property name of the property that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="labelText">The label Text.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param>
        public static MvcHtmlString LabelForModel(this DnnHtmlHelper html, string labelText, IDictionary<string, object> htmlAttributes)
        {
            return html.HtmlHelper.LabelForModel(labelText, htmlAttributes);
        }
    }
}
