// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Web.Mvc;
using System.Web.UI.WebControls;
using DotNetNuke.Web.Mvc.Helpers;

namespace Dnn.Modules.DynamicContentViewer.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    public static class DnnDisplayExtensions
    {

        public static MvcHtmlString DisplayFor<TModel>(this DnnHelper<TModel> dnnHelper, string fieldName)
        {
            return TemplateHelper.TemplateFor(dnnHelper, fieldName, null, null, DataBoundControlMode.ReadOnly, null);
        }

        public static MvcHtmlString DisplayFor<TModel>(this DnnHelper<TModel> dnnHelper, string fieldName, object additionalViewData)
        {
            return TemplateHelper.TemplateFor(dnnHelper, fieldName, null, null, DataBoundControlMode.ReadOnly, additionalViewData);
        }

        public static MvcHtmlString DisplayFor<TModel>(this DnnHelper<TModel> dnnHelper, string fieldName, string templateName)
        {
            return TemplateHelper.TemplateFor(dnnHelper, fieldName, templateName, null, DataBoundControlMode.ReadOnly, null);
        }

        public static MvcHtmlString DisplayFor<TModel>(this DnnHelper<TModel> dnnHelper, string fieldName, string templateName, object additionalViewData)
        {
            return TemplateHelper.TemplateFor(dnnHelper, fieldName, templateName, null, DataBoundControlMode.ReadOnly, additionalViewData);
        }

        public static MvcHtmlString DisplayFor<TModel>(this DnnHelper<TModel> dnnHelper, string fieldName, string templateName, string htmlFieldName)
        {
            return TemplateHelper.TemplateFor(dnnHelper, fieldName, templateName, htmlFieldName, DataBoundControlMode.ReadOnly, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dnnHelper"></param>
        /// <param name="fieldName"></param>
        /// <param name="templateName"></param>
        /// <param name="htmlFieldName"></param>
        /// <param name="additionalViewData"></param>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public static MvcHtmlString DisplayFor<TModel>(this DnnHelper<TModel> dnnHelper, string fieldName, string templateName, string htmlFieldName, object additionalViewData)
        {
            return TemplateHelper.TemplateFor(dnnHelper, fieldName, templateName, htmlFieldName, DataBoundControlMode.ReadOnly, additionalViewData);
        }
    }
}
