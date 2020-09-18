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
    /// Gets the HTML ID and name attributes of the <see cref="T:DotNetNuke.Web.Mvc.HtmlHelper"/> string.
    /// </summary>
    public static class HtmlNameExtensions
    {
        /// <summary>
        /// Gets the ID of the <see cref="T:DotNetNuke.Web.Mvc.HtmlHelper"/> string.
        /// </summary>
        ///
        /// <returns>
        /// The HTML ID attribute value for the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">An expression that identifies the object that contains the ID.</param>
        public static MvcHtmlString Id(this DnnHtmlHelper html, string name)
        {
            return html.HtmlHelper.Id(name);
        }

        /// <summary>
        /// Gets the ID of the <see cref="T:DotNetNuke.Web.Mvc.HtmlHelper"/> string.
        /// </summary>
        ///
        /// <returns>
        /// The HTML ID attribute value for the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the ID.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TProperty">The type of the property.</typeparam>
        public static MvcHtmlString IdFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression)
        {
            return html.HtmlHelper.IdFor(expression);
        }

        /// <summary>
        /// Gets the ID of the <see cref="T:DotNetNuke.Web.Mvc.HtmlHelper"/> string.
        /// </summary>
        ///
        /// <returns>
        /// The HTML ID attribute value for the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param>
        public static MvcHtmlString IdForModel(this DnnHtmlHelper html)
        {
            return html.HtmlHelper.IdForModel();
        }

        /// <summary>
        /// Gets the full HTML field name for the object that is represented by the expression.
        /// </summary>
        ///
        /// <returns>
        /// The full HTML field name for the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">An expression that identifies the object that contains the name.</param>
        public static MvcHtmlString Name(this DnnHtmlHelper html, string name)
        {
            return html.HtmlHelper.Name(name);
        }

        /// <summary>
        /// Gets the full HTML field name for the object that is represented by the expression.
        /// </summary>
        ///
        /// <returns>
        /// The full HTML field name for the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the name.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TProperty">The type of the property.</typeparam>
        public static MvcHtmlString NameFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression)
        {
            return html.HtmlHelper.NameFor(expression);
        }

        /// <summary>
        /// Gets the full HTML field name for the object that is represented by the expression.
        /// </summary>
        ///
        /// <returns>
        /// The full HTML field name for the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param>
        public static MvcHtmlString NameForModel(this DnnHtmlHelper html)
        {
            return html.HtmlHelper.NameForModel();
        }
    }
}
