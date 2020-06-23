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
    /// Provides support for validating the input from an HTML form.
    /// </summary>
    public static class HtmlValidationExtensions
    {
        // Validate

        /// <summary>
        /// Retrieves the validation metadata for the specified model and applies each rule to the data field.
        /// </summary>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="modelName">The name of the property or model object that is being validated.</param><exception cref="T:System.ArgumentNullException">The <paramref name="modelName"/> parameter is null.</exception>
        public static void Validate(this DnnHtmlHelper html, string modelName)
        {
            html.HtmlHelper.Validate(modelName);
        }

        /// <summary>
        /// Retrieves the validation metadata for the specified model and applies each rule to the data field.
        /// </summary>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to render.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TProperty">The type of the property.</typeparam>
        public static void ValidateFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            htmlHelper.ValidateFor(expression);
        }

        // ValidationMessage

        /// <summary>
        /// Displays a validation message if an error exists for the specified field in the <see cref="T:System.Web.Mvc.ModelStateDictionary"/> object.
        /// </summary>
        ///
        /// <returns>
        /// If the property or object is valid, an empty string; otherwise, a span element that contains an error message.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="modelName">The name of the property or model object that is being validated.</param>
        public static MvcHtmlString ValidationMessage(this DnnHtmlHelper html, string modelName)
        {
            return html.HtmlHelper.ValidationMessage(modelName);
        }

        /// <summary>
        /// Displays a validation message if an error exists for the specified field in the <see cref="T:System.Web.Mvc.ModelStateDictionary"/> object.
        /// </summary>
        ///
        /// <returns>
        /// If the property or object is valid, an empty string; otherwise, a span element that contains an error message.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="modelName">The name of the property or model object that is being validated.</param><param name="htmlAttributes">An object that contains the HTML attributes for the element. </param>
        public static MvcHtmlString ValidationMessage(this DnnHtmlHelper html, string modelName, object htmlAttributes)
        {
            return html.HtmlHelper.ValidationMessage(modelName, htmlAttributes);
        }

        /// <summary>
        /// Displays a validation message if an error exists for the specified field in the <see cref="T:System.Web.Mvc.ModelStateDictionary"/> object.
        /// </summary>
        ///
        /// <returns>
        /// If the property or object is valid, an empty string; otherwise, a span element that contains an error message.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="modelName">The name of the property or model object that is being validated.</param><param name="validationMessage">The message to display if the specified field contains an error.</param>
        public static MvcHtmlString ValidationMessage(this DnnHtmlHelper html, string modelName, string validationMessage)
        {
            return html.HtmlHelper.ValidationMessage(modelName, validationMessage);
        }

        /// <summary>
        /// Displays a validation message if an error exists for the specified field in the <see cref="T:System.Web.Mvc.ModelStateDictionary"/> object.
        /// </summary>
        ///
        /// <returns>
        /// If the property or object is valid, an empty string; otherwise, a span element that contains an error message.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="modelName">The name of the property or model object that is being validated.</param><param name="validationMessage">The message to display if the specified field contains an error.</param><param name="htmlAttributes">An object that contains the HTML attributes for the element. </param>
        public static MvcHtmlString ValidationMessage(this DnnHtmlHelper html, string modelName, string validationMessage, object htmlAttributes)
        {
            return html.HtmlHelper.ValidationMessage(modelName, validationMessage, htmlAttributes);
        }

        /// <summary>
        /// Displays a validation message if an error exists for the specified field in the <see cref="T:System.Web.Mvc.ModelStateDictionary"/> object.
        /// </summary>
        ///
        /// <returns>
        /// If the property or object is valid, an empty string; otherwise, a span element that contains an error message.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="modelName">The name of the property or model object that is being validated.</param><param name="htmlAttributes">An object that contains the HTML attributes for the element.</param>
        public static MvcHtmlString ValidationMessage(this DnnHtmlHelper html, string modelName, IDictionary<string, object> htmlAttributes)
        {
            return html.HtmlHelper.ValidationMessage(modelName, htmlAttributes);
        }

        /// <summary>
        /// Displays a validation message if an error exists for the specified field in the <see cref="T:System.Web.Mvc.ModelStateDictionary"/> object.
        /// </summary>
        ///
        /// <returns>
        /// If the property or object is valid, an empty string; otherwise, a span element that contains an error message.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="modelName">The name of the property or model object that is being validated.</param><param name="validationMessage">The message to display if the specified field contains an error.</param><param name="htmlAttributes">An object that contains the HTML attributes for the element.</param>
        public static MvcHtmlString ValidationMessage(this DnnHtmlHelper html, string modelName, string validationMessage, IDictionary<string, object> htmlAttributes)
        {
            return html.HtmlHelper.ValidationMessage(modelName, validationMessage, htmlAttributes);
        }

        /// <summary>
        /// Returns the HTML markup for a validation-error message for each data field that is represented by the specified expression.
        /// </summary>
        ///
        /// <returns>
        /// If the property or object is valid, an empty string; otherwise, a span element that contains an error message.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to render.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TProperty">The type of the property.</typeparam>
        public static MvcHtmlString ValidationMessageFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.ValidationMessageFor(expression);
        }

        /// <summary>
        /// Returns the HTML markup for a validation-error message for each data field that is represented by the specified expression, using the specified message.
        /// </summary>
        ///
        /// <returns>
        /// If the property or object is valid, an empty string; otherwise, a span element that contains an error message.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to render.</param><param name="validationMessage">The message to display if the specified field contains an error.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TProperty">The type of the property.</typeparam>
        public static MvcHtmlString ValidationMessageFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, string validationMessage)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.ValidationMessageFor(expression, validationMessage);
        }

        /// <summary>
        /// Returns the HTML markup for a validation-error message for each data field that is represented by the specified expression, using the specified message and HTML attributes.
        /// </summary>
        ///
        /// <returns>
        /// If the property or object is valid, an empty string; otherwise, a span element that contains an error message.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to render.</param><param name="validationMessage">The message to display if the specified field contains an error.</param><param name="htmlAttributes">An object that contains the HTML attributes for the element.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TProperty">The type of the property.</typeparam>
        public static MvcHtmlString ValidationMessageFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, string validationMessage, object htmlAttributes)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.ValidationMessageFor(expression, validationMessage, htmlAttributes);
        }

        /// <summary>
        /// Returns the HTML markup for a validation-error message for each data field that is represented by the specified expression, using the specified message and HTML attributes.
        /// </summary>
        ///
        /// <returns>
        /// If the property or object is valid, an empty string; otherwise, a span element that contains an error message.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to render.</param><param name="validationMessage">The message to display if the specified field contains an error.</param><param name="htmlAttributes">An object that contains the HTML attributes for the element.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TProperty">The type of the property.</typeparam>
        public static MvcHtmlString ValidationMessageFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, string validationMessage, IDictionary<string, object> htmlAttributes)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.ValidationMessageFor(expression, validationMessage, htmlAttributes);
        }

        // ValidationSummary

        /// <summary>
        /// Returns an unordered list (ul element) of validation messages that are in the <see cref="T:System.Web.Mvc.ModelStateDictionary"/> object.
        /// </summary>
        ///
        /// <returns>
        /// A string that contains an unordered list (ul element) of validation messages.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param>
        public static MvcHtmlString ValidationSummary(this DnnHtmlHelper html)
        {
            return html.HtmlHelper.ValidationSummary();
        }

        /// <summary>
        /// Returns an unordered list (ul element) of validation messages that are in the <see cref="T:System.Web.Mvc.ModelStateDictionary"/> object and optionally displays only model-level errors.
        /// </summary>
        ///
        /// <returns>
        /// A string that contains an unordered list (ul element) of validation messages.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="excludePropertyErrors">true to have the summary display model-level errors only, or false to have the summary display all errors.</param>
        public static MvcHtmlString ValidationSummary(this DnnHtmlHelper html, bool excludePropertyErrors)
        {
            return html.HtmlHelper.ValidationSummary(excludePropertyErrors);
        }

        /// <summary>
        /// Returns an unordered list (ul element) of validation messages that are in the <see cref="T:System.Web.Mvc.ModelStateDictionary"/> object.
        /// </summary>
        ///
        /// <returns>
        /// A string that contains an unordered list (ul element) of validation messages.
        /// </returns>
        /// <param name="html">The HMTL helper instance that this method extends.</param><param name="message">The message to display if the specified field contains an error.</param>
        public static MvcHtmlString ValidationSummary(this DnnHtmlHelper html, string message)
        {
            return html.HtmlHelper.ValidationSummary(message);
        }

        /// <summary>
        /// Returns an unordered list (ul element) of validation messages that are in the <see cref="T:System.Web.Mvc.ModelStateDictionary"/> object and optionally displays only model-level errors.
        /// </summary>
        ///
        /// <returns>
        /// A string that contains an unordered list (ul element) of validation messages.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="excludePropertyErrors">true to have the summary display model-level errors only, or false to have the summary display all errors.</param><param name="message">The message to display with the validation summary.</param>
        public static MvcHtmlString ValidationSummary(this DnnHtmlHelper html, bool excludePropertyErrors, string message)
        {
            return html.HtmlHelper.ValidationSummary(excludePropertyErrors, message);
        }

        /// <summary>
        /// Returns an unordered list (ul element) of validation messages in the <see cref="T:System.Web.Mvc.ModelStateDictionary"/> object.
        /// </summary>
        ///
        /// <returns>
        /// A string that contains an unordered list (ul element) of validation messages.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="message">The message to display if the specified field contains an error.</param><param name="htmlAttributes">An object that contains the HTML attributes for the element. </param>
        public static MvcHtmlString ValidationSummary(this DnnHtmlHelper html, string message, object htmlAttributes)
        {
            return html.HtmlHelper.ValidationSummary(message, htmlAttributes);
        }

        /// <summary>
        /// Returns an unordered list (ul element) of validation messages that are in the <see cref="T:System.Web.Mvc.ModelStateDictionary"/> object and optionally displays only model-level errors.
        /// </summary>
        ///
        /// <returns>
        /// A string that contains an unordered list (ul element) of validation messages.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="excludePropertyErrors">true to have the summary display model-level errors only, or false to have the summary display all errors.</param><param name="message">The message to display with the validation summary.</param><param name="htmlAttributes">An object that contains the HTML attributes for the element.</param>
        public static MvcHtmlString ValidationSummary(this DnnHtmlHelper html, bool excludePropertyErrors, string message, object htmlAttributes)
        {
            return html.HtmlHelper.ValidationSummary(excludePropertyErrors, message, htmlAttributes);
        }

        /// <summary>
        /// Returns an unordered list (ul element) of validation messages that are in the <see cref="T:System.Web.Mvc.ModelStateDictionary"/> object.
        /// </summary>
        ///
        /// <returns>
        /// A string that contains an unordered list (ul element) of validation messages.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="message">The message to display if the specified field contains an error.</param><param name="htmlAttributes">A dictionary that contains the HTML attributes for the element.</param>
        public static MvcHtmlString ValidationSummary(this DnnHtmlHelper html, string message, IDictionary<string, object> htmlAttributes)
        {
            return html.HtmlHelper.ValidationSummary(message, htmlAttributes);
        }

        /// <summary>
        /// Returns an unordered list (ul element) of validation messages that are in the <see cref="T:System.Web.Mvc.ModelStateDictionary"/> object and optionally displays only model-level errors.
        /// </summary>
        ///
        /// <returns>
        /// A string that contains an unordered list (ul element) of validation messages.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="excludePropertyErrors">true to have the summary display model-level errors only, or false to have the summary display all errors.</param><param name="message">The message to display with the validation summary.</param><param name="htmlAttributes">A dictionary that contains the HTML attributes for the element.</param>
        public static MvcHtmlString ValidationSummary(this DnnHtmlHelper html, bool excludePropertyErrors, string message, IDictionary<string, object> htmlAttributes)
        {
            return html.HtmlHelper.ValidationSummary(excludePropertyErrors, message, htmlAttributes);
        }
    }
}
