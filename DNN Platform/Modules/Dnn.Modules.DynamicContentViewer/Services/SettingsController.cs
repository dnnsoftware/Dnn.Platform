// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Dnn.DynamicContent;
using Dnn.Modules.DynamicContentViewer.Models;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Security;
using DotNetNuke.UI.Modules.Html5;
using DotNetNuke.Web.Api;

namespace Dnn.Modules.DynamicContentViewer.Services
{
    /// <summary>
    /// SettingsController provides the Web Services to manage Settings
    /// </summary>
    [SupportedModules("Dnn.DynamicContentViewer")]
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
    public class SettingsController : DnnApiController
    {
        /// <summary>
        /// GetTemplates retrieves the Templates for a ContentType
        /// </summary>
        /// <param name="contentTypeId">The Id of the content type</param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetTemplates(int contentTypeId)
        {
            var templateList = ContentTemplateManager.Instance.GetContentTemplates(PortalSettings.PortalId, true)
                                                .Where(t => t.ContentTypeId == contentTypeId);
            var templates = templateList
                                .Select(t => new { name = t.Name, value = t.TemplateId, isEdit = t.IsEditTemplate })
                                .ToList();

            var response = new
                            {
                                results = templates
                            };

            return Request.CreateResponse(response);
        }

        /// <summary>
        /// Saves the module's settings
        /// </summary>
        /// <param name="settings">The settings to save</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SaveSettings(Settings settings)
        {
            ModuleController.Instance.UpdateModuleSetting(ActiveModule.ModuleID, Settings.DCC_ContentTypeId, settings.ContentTypeId.ToString());
            ModuleController.Instance.UpdateModuleSetting(ActiveModule.ModuleID, Settings.DCC_EditTemplateId, settings.EditTemplateId.ToString());
            ModuleController.Instance.UpdateModuleSetting(ActiveModule.ModuleID, Settings.DCC_ViewTemplateId, settings.ViewTemplateId.ToString());

            var response = new
                            {
                                success = true
                            };

            return Request.CreateResponse(response);
        }
    }
}
