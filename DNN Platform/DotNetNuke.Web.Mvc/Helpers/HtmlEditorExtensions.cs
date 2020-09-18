// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Helpers
{
    using System;
    using System.Linq.Expressions;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;

    /// <summary>
    /// Represents support for the HTML input element in an application.
    /// </summary>
    public static class HtmlEditorExtensions
    {
        /// <summary>
        /// Returns an HTML input element for each property in the object that is represented by the expression.
        /// </summary>
        ///
        /// <returns>
        /// An HTML input element for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to display.</param>
        public static MvcHtmlString Editor(this DnnHtmlHelper html, string expression)
        {
            return html.HtmlHelper.Editor(expression);
        }

        /// <summary>
        /// Returns an HTML input element for each property in the object that is represented by the expression, using additional view data.
        /// </summary>
        ///
        /// <returns>
        /// An HTML input element for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to display.</param><param name="additionalViewData">An anonymous object that can contain additional view data that will be merged into the <see cref="T:System.Web.Mvc.ViewDataDictionary`1"/> instance that is created for the template.</param>
        public static MvcHtmlString Editor(this DnnHtmlHelper html, string expression, object additionalViewData)
        {
            return html.HtmlHelper.Editor(expression, additionalViewData);
        }

        /// <summary>
        /// Returns an HTML input element for each property in the object that is represented by the expression, using the specified template.
        /// </summary>
        ///
        /// <returns>
        /// An HTML input element for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to display.</param><param name="templateName">The name of the template to use to render the object.</param>
        public static MvcHtmlString Editor(this DnnHtmlHelper html, string expression, string templateName)
        {
            return html.HtmlHelper.Editor(expression, templateName);
        }

        /// <summary>
        /// Returns an HTML input element for each property in the object that is represented by the expression, using the specified template and additional view data.
        /// </summary>
        ///
        /// <returns>
        /// An HTML input element for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to display.</param><param name="templateName">The name of the template to use to render the object.</param><param name="additionalViewData">An anonymous object that can contain additional view data that will be merged into the <see cref="T:System.Web.Mvc.ViewDataDictionary`1"/> instance that is created for the template.</param>
        public static MvcHtmlString Editor(this DnnHtmlHelper html, string expression, string templateName, object additionalViewData)
        {
            return html.HtmlHelper.Editor(expression, templateName, additionalViewData);
        }

        /// <summary>
        /// Returns an HTML input element for each property in the object that is represented by the expression, using the specified template and HTML field name.
        /// </summary>
        ///
        /// <returns>
        /// An HTML input element for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to display.</param><param name="templateName">The name of the template to use to render the object.</param><param name="htmlFieldName">A string that is used to disambiguate the names of HTML input elements that are rendered for properties that have the same name.</param>
        public static MvcHtmlString Editor(this DnnHtmlHelper html, string expression, string templateName, string htmlFieldName)
        {
            return html.HtmlHelper.Editor(expression, templateName, htmlFieldName);
        }

        /// <summary>
        /// Returns an HTML input element for each property in the object that is represented by the expression, using the specified template, HTML field name, and additional view data.
        /// </summary>
        ///
        /// <returns>
        /// An HTML input element for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to display.</param><param name="templateName">The name of the template to use to render the object.</param><param name="htmlFieldName">A string that is used to disambiguate the names of HTML input elements that are rendered for properties that have the same name.</param><param name="additionalViewData">An anonymous object that can contain additional view data that will be merged into the <see cref="T:System.Web.Mvc.ViewDataDictionary`1"/> instance that is created for the template.</param>
        public static MvcHtmlString Editor(this DnnHtmlHelper html, string expression, string templateName, string htmlFieldName, object additionalViewData)
        {
            return html.HtmlHelper.Editor(expression, templateName, htmlFieldName, additionalViewData);
        }

        /// <summary>
        /// Returns an HTML input element for each property in the object that is represented by the <see cref="T:System.Linq.Expressions.Expression"/> expression.
        /// </summary>
        ///
        /// <returns>
        /// An HTML input element for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to display.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TValue">The type of the value.</typeparam>
        public static MvcHtmlString EditorFor<TModel, TValue>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            return html.HtmlHelper.EditorFor(expression);
        }

        /// <summary>
        /// Returns an HTML input element for each property in the object that is represented by the expression, using additional view data.
        /// </summary>
        ///
        /// <returns>
        /// An HTML input element for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to display.</param><param name="additionalViewData">An anonymous object that can contain additional view data that will be merged into the <see cref="T:System.Web.Mvc.ViewDataDictionary`1"/> instance that is created for the template.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TValue">The type of the value.</typeparam>
        public static MvcHtmlString EditorFor<TModel, TValue>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, object additionalViewData)
        {
            return html.HtmlHelper.EditorFor(expression, additionalViewData);
        }

        /// <summary>
        /// Returns an HTML input element for each property in the object that is represented by the <see cref="T:System.Linq.Expressions.Expression"/> expression, using the specified template.
        /// </summary>
        ///
        /// <returns>
        /// An HTML input element for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to display.</param><param name="templateName">The name of the template to use to render the object.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TValue">The type of the value.</typeparam>
        public static MvcHtmlString EditorFor<TModel, TValue>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string templateName)
        {
            return html.HtmlHelper.EditorFor(expression, templateName);
        }

        /// <summary>
        /// Returns an HTML input element for each property in the object that is represented by the expression, using the specified template and additional view data.
        /// </summary>
        ///
        /// <returns>
        /// An HTML input element for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to display.</param><param name="templateName">The name of the template to use to render the object.</param><param name="additionalViewData">An anonymous object that can contain additional view data that will be merged into the <see cref="T:System.Web.Mvc.ViewDataDictionary`1"/> instance that is created for the template.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TValue">The type of the value.</typeparam>
        public static MvcHtmlString EditorFor<TModel, TValue>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string templateName, object additionalViewData)
        {
            return html.HtmlHelper.EditorFor(expression, templateName, additionalViewData);
        }

        /// <summary>
        /// Returns an HTML input element for each property in the object that is represented by the <see cref="T:System.Linq.Expressions.Expression"/> expression, using the specified template and HTML field name.
        /// </summary>
        ///
        /// <returns>
        /// An HTML input element for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to display.</param><param name="templateName">The name of the template to use to render the object.</param><param name="htmlFieldName">A string that is used to disambiguate the names of HTML input elements that are rendered for properties that have the same name.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TValue">The type of the value.</typeparam>
        public static MvcHtmlString EditorFor<TModel, TValue>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string templateName, string htmlFieldName)
        {
            return html.HtmlHelper.EditorFor(expression, templateName, htmlFieldName);
        }

        /// <summary>
        /// Returns an HTML input element for each property in the object that is represented by the expression, using the specified template, HTML field name, and additional view data.
        /// </summary>
        ///
        /// <returns>
        /// An HTML input element for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to display.</param><param name="templateName">The name of the template to use to render the object.</param><param name="htmlFieldName">A string that is used to disambiguate the names of HTML input elements that are rendered for properties that have the same name.</param><param name="additionalViewData">An anonymous object that can contain additional view data that will be merged into the <see cref="T:System.Web.Mvc.ViewDataDictionary`1"/> instance that is created for the template.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TValue">The type of the value.</typeparam>
        public static MvcHtmlString EditorFor<TModel, TValue>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string templateName, string htmlFieldName, object additionalViewData)
        {
            return html.HtmlHelper.EditorFor(expression, templateName, htmlFieldName, additionalViewData);
        }

        /// <summary>
        /// Returns an HTML input element for each property in the model.
        /// </summary>
        ///
        /// <returns>
        /// An HTML input element for each property in the model.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param>
        public static MvcHtmlString EditorForModel(this DnnHtmlHelper html)
        {
            return html.HtmlHelper.EditorForModel();
        }

        /// <summary>
        /// Returns an HTML input element for each property in the model, using additional view data.
        /// </summary>
        ///
        /// <returns>
        /// An HTML input element for each property in the model.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="additionalViewData">An anonymous object that can contain additional view data that will be merged into the <see cref="T:System.Web.Mvc.ViewDataDictionary`1"/> instance that is created for the template.</param>
        public static MvcHtmlString EditorForModel(this DnnHtmlHelper html, object additionalViewData)
        {
            return html.HtmlHelper.EditorForModel(additionalViewData);
        }

        /// <summary>
        /// Returns an HTML input element for each property in the model, using the specified template.
        /// </summary>
        ///
        /// <returns>
        /// An HTML input element for each property in the model and in the specified template.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="templateName">The name of the template to use to render the object.</param>
        public static MvcHtmlString EditorForModel(this DnnHtmlHelper html, string templateName)
        {
            return html.HtmlHelper.EditorForModel(templateName);
        }

        /// <summary>
        /// Returns an HTML input element for each property in the model, using the specified template and additional view data.
        /// </summary>
        ///
        /// <returns>
        /// An HTML input element for each property in the model.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="templateName">The name of the template to use to render the object.</param><param name="additionalViewData">An anonymous object that can contain additional view data that will be merged into the <see cref="T:System.Web.Mvc.ViewDataDictionary`1"/> instance that is created for the template.</param>
        public static MvcHtmlString EditorForModel(this DnnHtmlHelper html, string templateName, object additionalViewData)
        {
            return html.HtmlHelper.EditorForModel(templateName, additionalViewData);
        }

        /// <summary>
        /// Returns an HTML input element for each property in the model, using the specified template name and HTML field name.
        /// </summary>
        ///
        /// <returns>
        /// An HTML input element for each property in the model and in the named template.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="templateName">The name of the template to use to render the object.</param><param name="htmlFieldName">A string that is used to disambiguate the names of HTML input elements that are rendered for properties that have the same name.</param>
        public static MvcHtmlString EditorForModel(this DnnHtmlHelper html, string templateName, string htmlFieldName)
        {
            return html.HtmlHelper.EditorForModel(templateName, htmlFieldName);
        }

        /// <summary>
        /// Returns an HTML input element for each property in the model, using the template name, HTML field name, and additional view data.
        /// </summary>
        ///
        /// <returns>
        /// An HTML input element for each property in the model.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="templateName">The name of the template to use to render the object.</param><param name="htmlFieldName">A string that is used to disambiguate the names of HTML input elements that are rendered for properties that have the same name.</param><param name="additionalViewData">An anonymous object that can contain additional view data that will be merged into the <see cref="T:System.Web.Mvc.ViewDataDictionary`1"/> instance that is created for the template.</param>
        public static MvcHtmlString EditorForModel(this DnnHtmlHelper html, string templateName, string htmlFieldName, object additionalViewData)
        {
            return html.HtmlHelper.EditorForModel(templateName, htmlFieldName, additionalViewData);
        }
    }
}
