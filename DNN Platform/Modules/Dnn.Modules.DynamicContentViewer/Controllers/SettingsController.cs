// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using System.Web.Mvc;
using Dnn.DynamicContent;
using Dnn.Modules.DynamicContentViewer.Models;
using DotNetNuke.Collections;
using DotNetNuke.Web.Mvc.Framework.Controllers;

namespace Dnn.Modules.DynamicContentViewer.Controllers
{
    /// <summary>
    /// The Settings Controller manages the modules settings
    /// </summary>
    public class SettingsController : DnnController
    {

        /// <summary>
        /// The Index action renders the default Settings View
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var contentTypeId = ActiveModule.ModuleSettings.GetValueOrDefault(Settings.DCC_ContentTypeId, -1);
            var settings = new Settings
                                {
                                    ContentTypeId = contentTypeId,
                                    ContentTypes = DynamicContentTypeManager.Instance.GetContentTypes(PortalSettings.PortalId, true).ToList(),
                                    ViewTemplateId = ActiveModule.ModuleSettings.GetValueOrDefault(Settings.DCC_ViewTemplateId, -1),
                                    Templates = ContentTemplateManager.Instance.GetContentTemplates(PortalSettings.PortalId, true).Where(t => t.ContentTypeId == contentTypeId).ToList(),
                                    EditTemplateId = ActiveModule.ModuleSettings.GetValueOrDefault(Settings.DCC_EditTemplateId, -1),
                                };
            return View(settings);
        }
    }
}
