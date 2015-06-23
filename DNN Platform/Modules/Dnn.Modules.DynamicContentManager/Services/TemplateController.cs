// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Dnn.DynamicContent;
using Dnn.Modules.DynamicContentManager.Services.ViewModels;
using DotNetNuke.Security;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Api;

namespace Dnn.Modules.DynamicContentManager.Services
{
    /// <summary>
    /// TemplateController provides the Web Services to manage Templates
    /// </summary>
    [SupportedModules("Dnn.DynamicContentManager")]
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
    public class TemplateController : DnnApiController
    {
        private string LocalResourceFile = "~/DesktopModules/Dnn/DynamicContentManager/App_LocalResources/Manager.resx";

        /// <summary>
        /// DeleteTemplate deletes a single Template
        /// </summary>
        /// <param name="viewModel">The Template to delete</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteTemplate(TemplateViewModel viewModel)
        {
            var template = ContentTemplateManager.Instance.GetContentTemplate(viewModel.TemplateId, PortalSettings.PortalId, true);

            if (template != null)
            {
                ContentTemplateManager.Instance.DeleteContentTemplate(template);
            }

            var response = new
            {
                success = true
            };

            return Request.CreateResponse(response);
        }

        /// <summary>
        /// GetTemplate retrieves a single Template
        /// </summary>
        /// <param name="templateId">The id of the Template</param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetTemplate(int templateId)
        {
            var template = ContentTemplateManager.Instance.GetContentTemplate(templateId, PortalSettings.PortalId, true);

            var response = new
                            {
                                success = true,
                                data = new
                                        {
                                            template = new TemplateViewModel(template, PortalSettings.UserInfo.IsSuperUser)
                                        }
                            };

            return Request.CreateResponse(response);
        }

        /// <summary>
        /// GetTemplates retrieves a page of Templates that satisfy the searchTerm
        /// </summary>
        /// <param name="searchTerm">The text to use to search for Templates</param>
        /// <param name="pageIndex">The page to return, where 0 is the first page</param>
        /// <param name="pageSize">The size of the page</param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetTemplates(string searchTerm, int pageIndex, int pageSize)
        {
            var templateList = ContentTemplateManager.Instance.GetContentTemplates(searchTerm, PortalSettings.PortalId, pageIndex, pageSize, true);

            var templates = templateList
                                .Select(template => new TemplateViewModel(template, PortalSettings.UserInfo.IsSuperUser))
                                .ToList();

            var response = new
            {
                success = true,
                data = new
                        {
                            results = templates,
                            totalResults = templateList.TotalCount
                        }
            };

            return Request.CreateResponse(response);
        }

        /// <summary>
        /// SaveTemplate saves the template
        /// </summary>
        /// <param name="viewModel">The ViewModel for the Template to save</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SaveTemplate(TemplateViewModel viewModel)
        {
            ContentTemplate template;

            var templateStream = new MemoryStream(Encoding.UTF8.GetBytes(viewModel.Content ?? ""));
            var folderPath = viewModel.FilePath.Substring(0, viewModel.FilePath.LastIndexOf("/", StringComparison.Ordinal));
            var fileName = Path.GetFileName(viewModel.FilePath);
            var portalId = (viewModel.IsSystem) ? -1 : PortalSettings.PortalId;

            var folder = FolderManager.Instance.GetFolder(portalId, folderPath);
            if (folder == null)
            {
                return Request.CreateResponse(new {success = false, message = Localization.GetString("FolderNotFound", LocalResourceFile)});
            }

            var contentType = FileManager.Instance.GetContentType(Path.GetExtension(fileName));
            var file = FileManager.Instance.AddFile(folder, fileName, templateStream, true, true, true, contentType, PortalSettings.UserId);

            if (file == null)
            {
                return Request.CreateResponse(new { success = false, message = Localization.GetString("FileCreateError", LocalResourceFile) });
            }

            if (viewModel.TemplateId == -1)
            {
                template = new ContentTemplate()
                                    {
                                        ContentTypeId = viewModel.ContentTypeId,
                                        Name = viewModel.Name,
                                        TemplateFileId = file.FileId,
                                        PortalId = viewModel.IsSystem ? -1 : PortalSettings.PortalId
                                    };
                ContentTemplateManager.Instance.AddContentTemplate(template);
            }
            else
            {
                //Update
                template = ContentTemplateManager.Instance.GetContentTemplate(viewModel.TemplateId, PortalSettings.PortalId, true);

                if (template != null)
                {
                    template.Name = viewModel.Name;
                    ContentTemplateManager.Instance.UpdateContentTemplate(template);
                }
            }
            var response = new
                            {
                                success = true,
                                data = new
                                            {
                                                templateId = template.TemplateId
                                            }
                            };

            return Request.CreateResponse(response);
        }
    }
}