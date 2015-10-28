// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Dnn.DynamicContent;
using DotNetNuke.Web.Mvc.Helpers;

namespace Dnn.Modules.DynamicContentViewer.Helpers
{
    /// <summary>
    /// A class which provides extension methods to render Content Filed editors
    /// </summary>
    public static class DnnEditorExtensions
    {
        /// <summary>
        /// Get the Edit Template for field.
        /// </summary>
        /// <typeparam name="TModel">Model Type.</typeparam>
        /// <param name="dnnHelper">Helper Instance.</param>
        /// <param name="fieldName">Field Name.</param>
        /// <returns></returns>
        public static MvcHtmlString EditorFor<TModel>(this DnnHelper<TModel> dnnHelper, string fieldName)
        {
            return TemplateHelper.TemplateFor(dnnHelper, fieldName, null, null, DataBoundControlMode.Edit, null);
        }

        /// <summary>
        /// Get the Edit Template for field.
        /// </summary>
        /// <typeparam name="TModel">Model Type.</typeparam>
        /// <param name="dnnHelper">Helper Instance.</param>
        /// <param name="fieldName">Field Name.</param>
        /// <param name="additionalViewData">Addtional View Data.</param>
        /// <returns></returns>
        public static MvcHtmlString EditorFor<TModel>(this DnnHelper<TModel> dnnHelper, string fieldName, object additionalViewData)
        {
            return TemplateHelper.TemplateFor(dnnHelper, fieldName, null, null, DataBoundControlMode.Edit, additionalViewData);
        }

        /// <summary>
        /// Get the Edit Template for field.
        /// </summary>
        /// <typeparam name="TModel">Model Type.</typeparam>
        /// <param name="dnnHelper">Helper Instance.</param>
        /// <param name="fieldName">Field Name.</param>
        /// <param name="templateName">Specific Template Name.</param>
        /// <returns></returns>
        public static MvcHtmlString EditorFor<TModel>(this DnnHelper<TModel> dnnHelper, string fieldName, string templateName)
        {
            return TemplateHelper.TemplateFor(dnnHelper, fieldName, templateName, null, DataBoundControlMode.Edit, null);
        }

        /// <summary>
        /// Get the Edit Template for field.
        /// </summary>
        /// <typeparam name="TModel">Model Type.</typeparam>
        /// <param name="dnnHelper">Helper Instance.</param>
        /// <param name="fieldName">Field Name.</param>
        /// <param name="templateName">Specific Template Name.</param>
        /// <param name="additionalViewData">Addtional View Data.</param>
        /// <returns></returns>
        public static MvcHtmlString EditorFor<TModel>(this DnnHelper<TModel> dnnHelper, string fieldName, string templateName, object additionalViewData)
        {
            return TemplateHelper.TemplateFor(dnnHelper, fieldName, templateName, null, DataBoundControlMode.Edit, additionalViewData);
        }

        /// <summary>
        /// Get the Edit Template for field.
        /// </summary>
        /// <typeparam name="TModel">Model Type.</typeparam>
        /// <param name="dnnHelper">Helper Instance.</param>
        /// <param name="fieldName">Field Name.</param>
        /// <param name="templateName">Specific Template Name.</param>
        /// /// <param name="htmlFieldName">HTML Field Name.</param>
        /// <returns></returns>
        public static MvcHtmlString EditorFor<TModel>(this DnnHelper<TModel> dnnHelper, string fieldName, string templateName, string htmlFieldName)
        {
            return TemplateHelper.TemplateFor(dnnHelper, fieldName, templateName, htmlFieldName, DataBoundControlMode.Edit, null);
        }

        /// <summary>
        /// Get the Edit Template for field.
        /// </summary>
        /// <typeparam name="TModel">Model Type.</typeparam>
        /// <param name="dnnHelper">Helper Instance.</param>
        /// <param name="fieldName">Field Name.</param>
        /// <param name="templateName">Specific Template Name.</param>
        /// /// <param name="htmlFieldName">HTML Field Name.</param>
        /// /// <param name="additionalViewData">Addtional View Data.</param>
        /// <returns></returns>
        public static MvcHtmlString EditorFor<TModel>(this DnnHelper<TModel> dnnHelper, string fieldName, string templateName, string htmlFieldName, object additionalViewData)
        {
            return TemplateHelper.TemplateFor(dnnHelper, fieldName, templateName, htmlFieldName, DataBoundControlMode.Edit, additionalViewData);
        }
    }
}
