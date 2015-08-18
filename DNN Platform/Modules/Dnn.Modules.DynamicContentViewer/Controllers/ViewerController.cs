// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Web.Mvc;
using Dnn.DynamicContent;
using Dnn.Modules.DynamicContentViewer.Models;
using DotNetNuke.Application;
using DotNetNuke.Collections;
using DotNetNuke.Common;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Host;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Web.Mvc.Framework.Controllers;

namespace Dnn.Modules.DynamicContentViewer.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class ViewerController : DnnController
    {
        private DynamicContentItem GetOrCreateContentItem()
        {
            var contentTypeId = ActiveModule.ModuleSettings.GetValueOrDefault(Settings.DCC_ContentTypeId, -1);
            var contentItem = DynamicContentItemManager.Instance.GetContentItems(ActiveModule.ModuleID, contentTypeId).SingleOrDefault();

            if (contentItem == null)
            {
                var contentType = DynamicContentTypeManager.Instance.GetContentType(contentTypeId, PortalSettings.PortalId, true);
                contentItem = DynamicContentItemManager.Instance.CreateContentItem(PortalSettings.PortalId, ActivePage.TabID, ActiveModule.ModuleID, contentType);
                DynamicContentItemManager.Instance.AddContentItem(contentItem);
            }
            return contentItem;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var viewName = "GettingStarted";
            var templateId = ActiveModule.ModuleSettings.GetValueOrDefault(Settings.DCC_ViewTemplateId, -1);
            if (templateId > -1)
            {
                ContentTemplate template = ContentTemplateManager.Instance.GetContentTemplate(templateId, PortalSettings.PortalId, true);
                if (template != null)
                {
                    var fileId = template.TemplateFileId;
                    var file = FileManager.Instance.GetFile(fileId);
                    if (file != null)
                    {
                        if (file.PortalId > -1)
                        {
                            viewName = "~" + PortalSettings.HomeDirectory + file.RelativePath;
                        }
                        else
                        {
                            viewName = "~" + Globals.HostPath + file.RelativePath;
                        }
                    }
                }
            }

            if (viewName == "GettingStarted")
            {
                return View(viewName);
            }

            var contentItem = GetOrCreateContentItem();
            var model = new ExpandoObject();
            var modelDictionary = (IDictionary<string, object>)model;
            foreach (var field in contentItem.Fields)
            {
                object fieldValue;
                if (field.Value.Value == null)
                {
                    switch (field.Value.Definition.DataType.UnderlyingDataType)
                    {
                        case UnderlyingDataType.String:
                            fieldValue = String.Empty;
                            break;
                        case UnderlyingDataType.Boolean:
                            fieldValue = false;
                            break;
                        default:
                            fieldValue = 0;
                            break;
                    }
                }
                else
                {
                    fieldValue = field.Value.Value;
                }
                modelDictionary.Add(field.Key, fieldValue);
            }
            
            return View(viewName, model);
        }
    }
}
