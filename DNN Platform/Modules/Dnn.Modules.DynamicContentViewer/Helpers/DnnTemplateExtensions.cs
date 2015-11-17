// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Web.Mvc;
using DotNetNuke.Web.Mvc.Helpers;

namespace Dnn.Modules.DynamicContentViewer.Helpers
{
    /// <summary>
    /// A class which provides extension methods to get template.
    /// </summary>
    public static class DnnTemplateExtensions
    {
        /// <summary>
        /// Get Template.
        /// </summary>
        /// <typeparam name="TModel">Model Type.</typeparam>
        /// <param name="dnnHelper">Helper Instance.</param>
        /// <param name="templateName">Template Name.</param>
        /// <returns></returns>
        public static MvcHtmlString TemplateFor<TModel>(this DnnHelper<TModel> dnnHelper, string templateName)
        {
            return TemplateHelper.TemplateFor(dnnHelper, templateName, null);
        }

        /// <summary>
        /// Get Template.
        /// </summary>
        /// <typeparam name="TModel">Model Type.</typeparam>
        /// <param name="dnnHelper">Helper Instance.</param>
        /// <param name="templateName">Template Name.</param>
        /// <param name="additionalViewData">Additional View Data.</param>
        /// <returns></returns>
        public static MvcHtmlString TemplateFor<TModel>(this DnnHelper<TModel> dnnHelper, string templateName, object additionalViewData)
        {
            return TemplateHelper.TemplateFor(dnnHelper, templateName, additionalViewData);
        }

    }
}
