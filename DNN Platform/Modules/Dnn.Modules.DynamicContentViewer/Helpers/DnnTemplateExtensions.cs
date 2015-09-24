// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Web.Mvc;
using DotNetNuke.Web.Mvc.Helpers;

namespace Dnn.Modules.DynamicContentViewer.Helpers
{
    public static class DnnTemplateExtensions
    {
        public static MvcHtmlString TemplateFor<TModel>(this DnnHelper<TModel> dnnHelper, string templateName)
        {
            return TemplateHelper.TemplateFor(dnnHelper, templateName, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dnnHelper"></param>
        /// <param name="templateName"></param>
        /// <param name="additionalViewData"></param>
        /// <typeparam name="TModel"></typeparam>
        /// <returns></returns>
        public static MvcHtmlString TemplateFor<TModel>(this DnnHelper<TModel> dnnHelper, string templateName, object additionalViewData)
        {
            return TemplateHelper.TemplateFor(dnnHelper, templateName, additionalViewData);
        }

    }
}
