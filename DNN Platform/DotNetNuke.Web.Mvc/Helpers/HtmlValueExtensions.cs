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
    /// Provides a mechanism to create custom HTML markup compatible with the ASP.NET MVC model binders and templates.
    /// </summary>
    public static class HtmlValueExtensions
    {
        /// <summary>
        /// Provides a mechanism to create custom HTML markup compatible with the ASP.NET MVC model binders and templates.
        /// </summary>
        ///
        /// <returns>
        /// The HTML markup for the value.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the model.</param>
        public static MvcHtmlString Value(this DnnHtmlHelper html, string name)
        {
            return html.HtmlHelper.Value(name);
        }

        /// <summary>
        /// Provides a mechanism to create custom HTML markup compatible with the ASP.NET MVC model binders and templates.
        /// </summary>
        ///
        /// <returns>
        /// The HTML markup for the value.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the model.</param><param name="format">The format string.</param>
        public static MvcHtmlString Value(this DnnHtmlHelper html, string name, string format)
        {
            return html.HtmlHelper.Value(name, format);
        }

        /// <summary>
        /// Provides a mechanism to create custom HTML markup compatible with the ASP.NET MVC model binders and templates.
        /// </summary>
        ///
        /// <returns>
        /// The HTML markup for the value.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to expose.</param><typeparam name="TModel">The model.</typeparam><typeparam name="TProperty">The property.</typeparam>
        public static MvcHtmlString ValueFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression)
        {
            return html.HtmlHelper.ValueFor(expression);
        }

        /// <summary>
        /// Provides a mechanism to create custom HTML markup compatible with the ASP.NET MVC model binders and templates.
        /// </summary>
        ///
        /// <returns>
        /// The HTML markup for the value.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to expose.</param><param name="format">The format string.</param><typeparam name="TModel">The model.</typeparam><typeparam name="TProperty">The property.</typeparam>
        public static MvcHtmlString ValueFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, string format)
        {
            return html.HtmlHelper.ValueFor(expression, format);
        }

        /// <summary>
        /// Provides a mechanism to create custom HTML markup compatible with the ASP.NET MVC model binders and templates.
        /// </summary>
        ///
        /// <returns>
        /// The HTML markup for the value.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param>
        public static MvcHtmlString ValueForModel(this DnnHtmlHelper html)
        {
            return html.HtmlHelper.ValueForModel();
        }

        /// <summary>
        /// Provides a mechanism to create custom HTML markup compatible with the ASP.NET MVC model binders and templates.
        /// </summary>
        ///
        /// <returns>
        /// The HTML markup for the value.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="format">The format string.</param>
        public static MvcHtmlString ValueForModel(this DnnHtmlHelper html, string format)
        {
            return html.HtmlHelper.Value(format);
        }
    }
}
