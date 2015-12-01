// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace DotNetNuke.Web.Mvc.Helpers
{
    /// <summary>
    /// Represents the functionality to render a partial view as an HTML-encoded string.
    /// </summary>
    public static class HtmlPartialExtensions
    {
        /// <summary>
        /// Renders the specified partial view as an HTML-encoded string.
        /// </summary>
        /// 
        /// <returns>
        /// The partial view that is rendered as an HTML-encoded string.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="partialViewName">The name of the partial view to render.</param>
        public static MvcHtmlString Partial(this DnnHtmlHelper html, string partialViewName)
        {
            return html.HtmlHelper.Partial(partialViewName);
        }

        /// <summary>
        /// Renders the specified partial view as an HTML-encoded string.
        /// </summary>
        /// 
        /// <returns>
        /// The partial view that is rendered as an HTML-encoded string.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="partialViewName">The name of the partial view to render.</param><param name="viewData">The view data dictionary for the partial view.</param>
        public static MvcHtmlString Partial(this DnnHtmlHelper html, string partialViewName, ViewDataDictionary viewData)
        {
            return html.HtmlHelper.Partial(partialViewName, (object) null, viewData);
        }

        /// <summary>
        /// Renders the specified partial view as an HTML-encoded string.
        /// </summary>
        /// 
        /// <returns>
        /// The partial view that is rendered as an HTML-encoded string.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="partialViewName">The name of the partial view to render.</param><param name="model">The model for the partial view.</param>
        public static MvcHtmlString Partial(this DnnHtmlHelper html, string partialViewName, object model)
        {
            return html.HtmlHelper.Partial(partialViewName, model, html.HtmlHelper.ViewData);
        }

        /// <summary>
        /// Renders the specified partial view as an HTML-encoded string.
        /// </summary>
        /// 
        /// <returns>
        /// The partial view that is rendered as an HTML-encoded string.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="partialViewName">The name of the partial view.</param><param name="model">The model for the partial view.</param><param name="viewData">The view data dictionary for the partial view.</param>
        public static MvcHtmlString Partial(this DnnHtmlHelper html, string partialViewName, object model,
            ViewDataDictionary viewData)
        {
            return html.HtmlHelper.Partial(partialViewName, model, viewData);
        }
    }
}