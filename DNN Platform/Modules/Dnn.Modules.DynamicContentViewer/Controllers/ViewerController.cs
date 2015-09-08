// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web.Mvc;
using Dnn.DynamicContent;
using Dnn.Modules.DynamicContentViewer.Models;
using DotNetNuke.Collections;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Web.Mvc.Framework.ActionFilters;
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
		[ModuleActionItems]
        public ActionResult Index()
        {
            var viewName = "GettingStarted";
            var templateId = ActiveModule.ModuleSettings.GetValueOrDefault(Settings.DCC_ViewTemplateId, -1);
            ContentTemplate template = null;
            if (templateId > -1)
            {
                template = ContentTemplateManager.Instance.GetContentTemplate(templateId, PortalSettings.PortalId, true);
            }
            else
            {
                template = ContentTemplateManager.Instance.GetContentTemplates(PortalSettings.PortalId, true)
                        .SingleOrDefault(t => t.Name == "Getting Started");
            }

            IFileInfo file = null;
            if (template != null)
            {
                file = FileManager.Instance.GetFile(template.TemplateFileId);
            }

            if (file != null)
            {
                if (file.PortalId > -1)
                {
                    viewName = PortalSettings.HomeDirectory + file.RelativePath;
                }
                else
                {
                    viewName = Globals.HostPath + file.RelativePath;
                }
            }

            if (viewName == "GettingStarted")
            {
                return View(viewName);
            }

            return View(viewName, GetOrCreateContentItem());
        }

        public ModuleActionCollection GetIndexActions()
        {
            var actions = new ModuleActionCollection();

            var managerModule = ModuleController.Instance.GetModuleByDefinition(PortalSettings.PortalId, "Dnn.DynamicContentManager");

            if (managerModule != null && ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, "EDIT", managerModule))
            {
                actions.Add(-1,
                        LocalizeString("EditTemplates"),
                        ModuleActionType.AddContent,
                        "",
                        "",
                        Globals.NavigateURL(managerModule.TabID, String.Empty, "tab=Templates"),
                        false,
                        SecurityAccessLevel.Edit,
                        true,
                        false);
            }
            return actions;
        }
    }
}
