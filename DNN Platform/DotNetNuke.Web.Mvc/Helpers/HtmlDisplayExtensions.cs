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
    /// Represents support for rendering object values as HTML.
    /// </summary>
    public static class HtmlDisplayExtensions
    {
        /// <summary>
        /// Returns HTML markup for each property in the object that is represented by a string expression.
        /// </summary>
        ///
        /// <returns>
        /// The HTML markup for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to display.</param>
        public static MvcHtmlString Display(this DnnHtmlHelper html, string expression)
        {
            return html.HtmlHelper.Display(expression);
        }

        /// <summary>
        /// Returns HTML markup for each property in the object that is represented by a string expression, using additional view data.
        /// </summary>
        ///
        /// <returns>
        /// The HTML markup for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to display.</param><param name="additionalViewData">An anonymous object that can contain additional view data that will be merged into the <see cref="T:System.Web.Mvc.ViewDataDictionary`1"/> instance that is created for the template.</param>
        public static MvcHtmlString Display(this DnnHtmlHelper html, string expression, object additionalViewData)
        {
            return html.HtmlHelper.Display(expression, additionalViewData);
        }

        /// <summary>
        /// Returns HTML markup for each property in the object that is represented by the expression, using the specified template.
        /// </summary>
        ///
        /// <returns>
        /// The HTML markup for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to display.</param><param name="templateName">The name of the template that is used to render the object.</param>
        public static MvcHtmlString Display(this DnnHtmlHelper html, string expression, string templateName)
        {
            return html.HtmlHelper.Display(expression, templateName);
        }

        /// <summary>
        /// Returns HTML markup for each property in the object that is represented by the expression, using the specified template and additional view data.
        /// </summary>
        ///
        /// <returns>
        /// The HTML markup for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to display.</param><param name="templateName">The name of the template that is used to render the object.</param><param name="additionalViewData">An anonymous object that can contain additional view data that will be merged into the <see cref="T:System.Web.Mvc.ViewDataDictionary`1"/> instance that is created for the template.</param>
        public static MvcHtmlString Display(this DnnHtmlHelper html, string expression, string templateName, object additionalViewData)
        {
            return html.HtmlHelper.Display(expression, templateName, additionalViewData);
        }

        /// <summary>
        /// Returns HTML markup for each property in the object that is represented by the expression, using the specified template and an HTML field ID.
        /// </summary>
        ///
        /// <returns>
        /// The HTML markup for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to display.</param><param name="templateName">The name of the template that is used to render the object.</param><param name="htmlFieldName">A string that is used to disambiguate the names of HTML input elements that are rendered for properties that have the same name.</param>
        public static MvcHtmlString Display(this DnnHtmlHelper html, string expression, string templateName, string htmlFieldName)
        {
            return html.HtmlHelper.Display(expression, templateName, htmlFieldName);
        }

        /// <summary>
        /// Returns HTML markup for each property in the object that is represented by the expression, using the specified template, HTML field ID, and additional view data.
        /// </summary>
        ///
        /// <returns>
        /// The HTML markup for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to display.</param><param name="templateName">The name of the template that is used to render the object.</param><param name="htmlFieldName">A string that is used to disambiguate the names of HTML input elements that are rendered for properties that have the same name.</param><param name="additionalViewData">An anonymous object that can contain additional view data that will be merged into the <see cref="T:System.Web.Mvc.ViewDataDictionary`1"/> instance that is created for the template.</param>
        public static MvcHtmlString Display(this DnnHtmlHelper html, string expression, string templateName, string htmlFieldName, object additionalViewData)
        {
            return html.HtmlHelper.Display(expression, templateName, htmlFieldName, additionalViewData);
        }

        /// <summary>
        /// Returns HTML markup for each property in the object that is represented by the <see cref="T:System.Linq.Expressions.Expression"/> expression.
        /// </summary>
        ///
        /// <returns>
        /// The HTML markup for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to display.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TValue">The type of the value.</typeparam>
        public static MvcHtmlString DisplayFor<TModel, TValue>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            return html.HtmlHelper.DisplayFor(expression);
        }

        /// <summary>
        /// Returns a string that contains each property value in the object that is represented by the specified expression, using additional view data.
        /// </summary>
        ///
        /// <returns>
        /// The HTML markup for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to display.</param><param name="additionalViewData">An anonymous object that can contain additional view data that will be merged into the <see cref="T:System.Web.Mvc.ViewDataDictionary`1"/> instance that is created for the template.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TValue">The type of the value.</typeparam>
        public static MvcHtmlString DisplayFor<TModel, TValue>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, object additionalViewData)
        {
            return html.HtmlHelper.DisplayFor(expression, additionalViewData);
        }

        /// <summary>
        /// Returns a string that contains each property value in the object that is represented by the <see cref="T:System.Linq.Expressions.Expression"/>, using the specified template.
        /// </summary>
        ///
        /// <returns>
        /// The HTML markup for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to display.</param><param name="templateName">The name of the template that is used to render the object.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TValue">The type of the value.</typeparam>
        public static MvcHtmlString DisplayFor<TModel, TValue>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string templateName)
        {
            return html.HtmlHelper.DisplayFor(expression, templateName);
        }

        /// <summary>
        /// Returns a string that contains each property value in the object that is represented by the specified expression, using the specified template and additional view data.
        /// </summary>
        ///
        /// <returns>
        /// The HTML markup for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to display.</param><param name="templateName">The name of the template that is used to render the object.</param><param name="additionalViewData">An anonymous object that can contain additional view data that will be merged into the <see cref="T:System.Web.Mvc.ViewDataDictionary`1"/> instance that is created for the template.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TValue">The type of the value.</typeparam>
        public static MvcHtmlString DisplayFor<TModel, TValue>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string templateName, object additionalViewData)
        {
            return html.HtmlHelper.DisplayFor(expression, templateName, additionalViewData);
        }

        /// <summary>
        /// Returns HTML markup for each property in the object that is represented by the <see cref="T:System.Linq.Expressions.Expression"/>, using the specified template and an HTML field ID.
        /// </summary>
        ///
        /// <returns>
        /// The HTML markup for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to display.</param><param name="templateName">The name of the template that is used to render the object.</param><param name="htmlFieldName">A string that is used to disambiguate the names of HTML input elements that are rendered for properties that have the same name.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TValue">The type of the value.</typeparam>
        public static MvcHtmlString DisplayFor<TModel, TValue>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string templateName, string htmlFieldName)
        {
            return html.HtmlHelper.DisplayFor(expression, templateName, htmlFieldName);
        }

        /// <summary>
        /// Returns HTML markup for each property in the object that is represented by the specified expression, using the template, an HTML field ID, and additional view data.
        /// </summary>
        ///
        /// <returns>
        /// The HTML markup for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to display.</param><param name="templateName">The name of the template that is used to render the object.</param><param name="htmlFieldName">A string that is used to disambiguate the names of HTML input elements that are rendered for properties that have the same name.</param><param name="additionalViewData">An anonymous object that can contain additional view data that will be merged into the <see cref="T:System.Web.Mvc.ViewDataDictionary`1"/> instance that is created for the template.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TValue">The type of the value.</typeparam>
        public static MvcHtmlString DisplayFor<TModel, TValue>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string templateName, string htmlFieldName, object additionalViewData)
        {
            return html.HtmlHelper.DisplayFor(expression, templateName, htmlFieldName, additionalViewData);
        }

        /// <summary>
        /// Returns HTML markup for each property in the model.
        /// </summary>
        ///
        /// <returns>
        /// The HTML markup for each property in the model.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param>
        public static MvcHtmlString DisplayForModel(this DnnHtmlHelper html)
        {
            return html.HtmlHelper.DisplayForModel(html.ViewData.ModelMetadata);
        }

        /// <summary>
        /// Returns HTML markup for each property in the model, using additional view data.
        /// </summary>
        ///
        /// <returns>
        /// The HTML markup for each property in the model.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="additionalViewData">An anonymous object that can contain additional view data that will be merged into the <see cref="T:System.Web.Mvc.ViewDataDictionary`1"/> instance that is created for the template.</param>
        public static MvcHtmlString DisplayForModel(this DnnHtmlHelper html, object additionalViewData)
        {
            return html.HtmlHelper.DisplayForModel(additionalViewData);
        }

        /// <summary>
        /// Returns HTML markup for each property in the model using the specified template.
        /// </summary>
        ///
        /// <returns>
        /// The HTML markup for each property in the model.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="templateName">The name of the template that is used to render the object.</param>
        public static MvcHtmlString DisplayForModel(this DnnHtmlHelper html, string templateName)
        {
            return html.HtmlHelper.DisplayForModel(templateName);
        }

        /// <summary>
        /// Returns HTML markup for each property in the model, using the specified template and additional view data.
        /// </summary>
        ///
        /// <returns>
        /// The HTML markup for each property in the model.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="templateName">The name of the template that is used to render the object.</param><param name="additionalViewData">An anonymous object that can contain additional view data that will be merged into the <see cref="T:System.Web.Mvc.ViewDataDictionary`1"/> instance that is created for the template.</param>
        public static MvcHtmlString DisplayForModel(this DnnHtmlHelper html, string templateName, object additionalViewData)
        {
            return html.HtmlHelper.DisplayForModel(string.Empty, templateName, additionalViewData);
        }

        /// <summary>
        /// Returns HTML markup for each property in the model using the specified template and HTML field ID.
        /// </summary>
        ///
        /// <returns>
        /// The HTML markup for each property in the model.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="templateName">The name of the template that is used to render the object.</param><param name="htmlFieldName">A string that is used to disambiguate the names of HTML input elements that are rendered for properties that have the same name.</param>
        public static MvcHtmlString DisplayForModel(this DnnHtmlHelper html, string templateName, string htmlFieldName)
        {
            return html.HtmlHelper.DisplayForModel(htmlFieldName, templateName);
        }

        /// <summary>
        /// Returns HTML markup for each property in the model, using the specified template, an HTML field ID, and additional view data.
        /// </summary>
        ///
        /// <returns>
        /// The HTML markup for each property in the model.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="templateName">The name of the template that is used to render the object.</param><param name="htmlFieldName">A string that is used to disambiguate the names of HTML input elements that are rendered for properties that have the same name.</param><param name="additionalViewData">An anonymous object that can contain additional view data that will be merged into the <see cref="T:System.Web.Mvc.ViewDataDictionary`1"/> instance that is created for the template.</param>
        public static MvcHtmlString DisplayForModel(this DnnHtmlHelper html, string templateName, string htmlFieldName, object additionalViewData)
        {
            return html.HtmlHelper.DisplayForModel(htmlFieldName, templateName, additionalViewData);
        }
    }
}
