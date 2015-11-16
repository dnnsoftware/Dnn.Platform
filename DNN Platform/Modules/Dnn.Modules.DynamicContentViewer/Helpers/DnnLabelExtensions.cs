// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Web.Mvc;
using DotNetNuke.Web.Mvc.Helpers;

namespace Dnn.Modules.DynamicContentViewer.Helpers
{
    /// <summary>
    /// A class which provides extension methods to render Content Filed labels
    /// </summary>
    public static class DnnLabelExtensions
    {
        /// <summary>
        /// The LabelFor helper is an extension method that is used to render a label a Content Field
        /// </summary>
        /// <param name="dnnHelper">The instance of the DnnHelper class.</param>
        /// <param name="fieldName">The name of the Content Field</param>
        /// <typeparam name="TModel">The model type - this will actually fail if the Model is not a DynamicContentItem</typeparam>
        /// <returns>The html to render</returns>
        public static MvcHtmlString LabelFor<TModel>(this DnnHelper<TModel> dnnHelper, string fieldName)
        {
            return TemplateHelper.LabelFor(dnnHelper, fieldName, String.Empty, null);
        }

        /// <summary>
        /// The LabelFor helper is an extension method that is used to render a label a Content Field
        /// </summary>
        /// <param name="dnnHelper">The instance of the DnnHelper class.</param>
        /// <param name="fieldName">The name of the Content Field</param>
        /// <param name="htmlFieldName">The fieldname for the field that is being "labelled" - Defaults to fieldName</param>
        /// <typeparam name="TModel">The model type - this will actually fail if the Model is not a DynamicContentItem</typeparam>
        /// <returns>The html to render</returns>
        public static MvcHtmlString LabelFor<TModel>(this DnnHelper<TModel> dnnHelper, string fieldName, string htmlFieldName)
        {
            return TemplateHelper.LabelFor(dnnHelper, fieldName, htmlFieldName, null);
        }

        /// <summary>
        /// The LabelFor helper is an extension method that is used to render a label a Content Field
        /// </summary>
        /// <param name="dnnHelper">The instance of the DnnHelper class.</param>
        /// <param name="fieldName">The name of the Content Field</param>
        /// <param name="htmlFieldName">The fieldname for the field that is being "labelled" - Defaults to fieldName</param>
        /// <param name="htmlAttributes">A collection of HTML attributes to apply to the Label</param>
        /// <typeparam name="TModel">The model type - this will actually fail if the Model is not a DynamicContentItem</typeparam>
        /// <returns>The html to render</returns>
        public static MvcHtmlString LabelFor<TModel>(this DnnHelper<TModel> dnnHelper, string fieldName, string htmlFieldName, object htmlAttributes)
        {
            return TemplateHelper.LabelFor(dnnHelper, fieldName, htmlFieldName, htmlAttributes);
        }
    }
}
