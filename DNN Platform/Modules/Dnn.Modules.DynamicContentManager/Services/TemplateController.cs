// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Xml.Linq;
using Dnn.DynamicContent;
using Dnn.DynamicContent.Localization;
using Dnn.Modules.DynamicContentManager.Services.ViewModels;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
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
    public class TemplateController : BaseController
    {
        /// <summary>
        /// DeleteTemplate deletes a single Template
        /// </summary>
        /// <param name="viewModel">The Template to delete</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteTemplate(TemplateViewModel viewModel)
        {
            var response = DeleteEntity(() => ContentTemplateManager.Instance.GetContentTemplate(viewModel.TemplateId, PortalSettings.PortalId, true),
                                        (template) => {
                                                            ContentTemplateManager.Instance.DeleteContentTemplate(template);
                                                            var file = FileManager.Instance.GetFile(template.TemplateFileId);
                                                            FileManager.Instance.DeleteFile(file);
                                                        });

            return response;
        }

        /// <summary>
        /// GetSnippets retrieves a collection of code snippets
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetSnippets()
        {
            var snippets = new List<SnippetViewModel>();
            LoadSnippets(Globals.HostMapPath, snippets);
            LoadSnippets(PortalSettings.HomeDirectoryMapPath, snippets);

            var response = new
                                {
                                    results = snippets,
                                    totalResults = snippets.Count
                                };

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        private void LoadSnippets(string path, List<SnippetViewModel> snippets)
        {
            var filePath = path + "Config/Snippets.config";
            if (File.Exists(filePath))
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var xmlDoc = XDocument.Load(stream);
                    if (xmlDoc.Root != null)
                    {
                        var xmlSnippets = from el in xmlDoc.Root.Elements()
                                          select el;

                        snippets.AddRange(from xmlSnippet in xmlSnippets
                            let xmlTitle = (from el in xmlSnippet.Descendants() where el.Name.LocalName == "Title" select el).First()
                            let xmlCode = (from el in xmlSnippet.Descendants() where el.Name.LocalName == "Code" select el).First()
                            select new SnippetViewModel
                                            {
                                                Name = xmlTitle.Value,
                                                Snippet = xmlCode.Value
                                            });
                    }
                }
            }
        }

        /// <summary>
        /// GetTemplate retrieves a single Template
        /// </summary>
        /// <param name="templateId">The id of the Template</param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetTemplate(int templateId)
        {
            return GetEntity(() => ContentTemplateManager.Instance.GetContentTemplate(templateId, PortalSettings.PortalId, true),
                           template => new TemplateViewModel(template, PortalSettings));
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
            return GetPage(() => ContentTemplateManager.Instance.GetContentTemplates(searchTerm, PortalSettings.PortalId, pageIndex, pageSize, true),
                           template => new TemplateViewModel(template, PortalSettings));
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
            var templateStream = new MemoryStream(Encoding.UTF8.GetBytes(viewModel.Content ?? ""));
            var folderPath = viewModel.FilePath.Substring(0, viewModel.FilePath.LastIndexOf("/", StringComparison.Ordinal));
            var fileName = Path.GetFileName(viewModel.FilePath);
            var portalId = (viewModel.IsSystem) ? -1 : PortalSettings.PortalId;

            var folder = FolderManager.Instance.GetFolder(portalId, folderPath) ??
                         FolderManager.Instance.AddFolder(portalId, folderPath);

            var contentType = FileContentTypeManager.Instance.GetContentType(Path.GetExtension(fileName));
            var file = FileManager.Instance.AddFile(folder, fileName, templateStream, true, true, true, contentType, PortalSettings.UserId);

            if (file == null)
            {
                return Request.CreateResponse(new { success = false, message = Localization.GetString("FileCreateError", LocalResourceFile) });
            }

            var templateId = viewModel.TemplateId;
            var localizedNames = new List<ContentTypeLocalization>();
            string defaultName = ParseLocalizations(viewModel.LocalizedNames, localizedNames, portalId);

            return SaveEntity(templateId,
                /*CheckEntity*/ () => ContentTemplateManager.Instance.GetContentTemplates(portalId, true).SingleOrDefault((t => t.Name == defaultName)),

                /*ErrorMsg*/    LocalizeString("TemplateExists"),

                /*CreateEntity*/() => new ContentTemplate
                                            {
                                                ContentTypeId = viewModel.ContentTypeId,
                                                Name = defaultName,
                                                TemplateFileId = file.FileId,
                                                PortalId = portalId

                                            },

                /*AddEntity*/   template => ContentTemplateManager.Instance.AddContentTemplate(template),

                /*GetEntity*/   () => ContentTemplateManager.Instance.GetContentTemplate(templateId, PortalSettings.PortalId, true),

                /*UpdateEntity*/template =>
                                            {
                                                template.Name = defaultName;
                                                ContentTemplateManager.Instance.UpdateContentTemplate(template);
                                            },

                /*SaveLocal*/   id => SaveContentLocalizations(localizedNames, ContentTemplateManager.NameKey, id, portalId));
        }
    }
}
 