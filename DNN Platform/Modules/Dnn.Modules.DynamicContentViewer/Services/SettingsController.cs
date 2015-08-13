// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Dnn.DynamicContent;
using DotNetNuke.Security;
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
                                .Select(t => new { name = t.Name, value = t.TemplateId })
                                .ToList();

            var response = new
                            {
                                success = true,
                                data = new
                                        {
                                            results = templates
                                }
                            };

            return Request.CreateResponse(response);
        }

    }
}
