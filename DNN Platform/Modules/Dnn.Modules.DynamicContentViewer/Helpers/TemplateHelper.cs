// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Dnn.DynamicContent;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Web.Mvc.Common;
using DotNetNuke.Web.Mvc.Helpers;

namespace Dnn.Modules.DynamicContentViewer.Helpers
{
    internal class TemplateHelper
    {
        private static string ExecuteTemplate(DnnHelper dnnHelper, string template, ViewDataDictionary viewData)
        {
            var viewContext = dnnHelper.ViewContext;

            ViewEngineResult viewEngineResult = ViewEngines.Engines.FindPartialView(dnnHelper.ViewContext, template);
            if (viewEngineResult.View != null)
            {
                using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
                {
                    viewEngineResult.View.Render(new ViewContext(viewContext, viewEngineResult.View, viewData, viewContext.TempData, writer), writer);
                    return writer.ToString();
                }
            }

            return String.Empty;
        }

        private static string GetTemplate(string templateName, string rootPath, string rootMapPath, DataBoundControlMode mode)
        {
            var displayPath = "{0}Content Templates\\DisplayTemplates\\{1}.cshtml";
            var editorPath = "{0}Content Templates\\EditorTemplates\\{1}.cshtml";
            var templatePath = (mode == DataBoundControlMode.ReadOnly) ? displayPath : editorPath;

            var path = String.Format(templatePath.Replace("\\", "/"), rootPath, templateName);
            var mapPath = String.Format(templatePath, rootMapPath, templateName);
            if (File.Exists(mapPath))
            {
                return path;
            }

            return String.Empty;
        }

        private static string GetTemplate(string templateName, DataType dataType, DataBoundControlMode mode, PortalSettings settings)
        {
            var portalRoot = settings.HomeDirectory;
            var hostRoot = Globals.HostPath;
            var portalRootMap = settings.HomeDirectoryMapPath;
            var hostRootMap = Globals.HostMapPath;

            //Check Portal for Template Name
            var path = GetTemplate(templateName, portalRoot, portalRootMap, mode);

            //Check Host for Template Name
            if (String.IsNullOrEmpty(path))
            {
                path = GetTemplate(templateName, hostRoot, hostRootMap, mode);
            }

            //Check Portal for DataType
            if (String.IsNullOrEmpty(path))
            {
                path = GetTemplate(dataType.Name, portalRoot, portalRootMap, mode);
            }

            //Check Host for DataType
            if (String.IsNullOrEmpty(path))
            {
                path = GetTemplate(dataType.Name, hostRoot, hostRootMap, mode);
            }

            //Check Portal for Underlying DataType
            if (String.IsNullOrEmpty(path))
            {
                path = GetTemplate(dataType.UnderlyingDataType.ToString(), portalRoot, portalRootMap, mode);
            }

            //Check Host for Underlying DataType
            if (String.IsNullOrEmpty(path))
            {
                path = GetTemplate(dataType.UnderlyingDataType.ToString(), hostRoot, hostRootMap, mode);
            }

            return path;
        }

        internal static MvcHtmlString TemplateFor<TModel>(DnnHelper<TModel> dnnHelper, string fieldName, string templateName, string htmlFieldName, DataBoundControlMode mode, object additionalViewData)
        {
            var contentItem = dnnHelper.ViewData.Model as DynamicContentItem;

            if (contentItem == null)
            {
                throw new InvalidOperationException("This helper is only supported for models of type DynamicContentItem");
            }

            var contentField = contentItem.Fields[fieldName];

            if (contentField == null)
            {
                throw new InvalidOperationException("The fieldName does not represent a valid DynamicContentField");
            }

            var dataType = contentField.Definition.DataType;

            var template = GetTemplate(templateName, dataType, mode, dnnHelper.PortalSettings);

            string htmlString;
            if (String.IsNullOrEmpty(template))
            {
                htmlString = contentField.ToString();
            }
            else
            {
                if (contentField.Value == null)
                {
                    htmlString = String.Empty;
                }
                else
                {
                    var viewData = new ViewDataDictionary(dnnHelper.ViewData)
                    {
                        Model = contentField.Value
                    };

                    if (additionalViewData != null)
                    {
                        foreach (KeyValuePair<string, object> kvp in TypeHelper.ObjectToDictionary(additionalViewData))
                        {
                            viewData[kvp.Key] = kvp.Value;
                        }
                    }

                    htmlString = ExecuteTemplate(dnnHelper, template, viewData);
                }
            }

            return MvcHtmlString.Create(htmlString);
        }
    }
}
