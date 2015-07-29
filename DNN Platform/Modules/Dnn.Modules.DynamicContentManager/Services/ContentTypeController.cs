// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Dnn.DynamicContent;
using Dnn.DynamicContent.Localization;
using Dnn.Modules.DynamicContentManager.Services.ViewModels;
using DotNetNuke.Collections;
using DotNetNuke.Security;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Api;

namespace Dnn.Modules.DynamicContentManager.Services
{
    /// <summary>
    /// ContentTypeController provides the Web Services to manage Data Types
    /// </summary>
    [SupportedModules("Dnn.DynamicContentManager")]
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
    public class ContentTypeController : BaseController
    {
        /// <summary>
        /// DeleteContentField deletes a single ContentField
        /// </summary>
        /// <param name="viewModel">The ContentField to delete</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteContentField(ContentFieldViewModel viewModel)
        {
            return DeleteEntity(() => FieldDefinitionManager.Instance.GetFieldDefinition(viewModel.ContentFieldId, viewModel.ContentTypeId),
                                contentField => FieldDefinitionManager.Instance.DeleteFieldDefinition(contentField));
        }

        /// <summary>
        /// DeleteContentType deletes a single ContentType
        /// </summary>
        /// <param name="viewModel">The ContentType to delete</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteContentType(ContentTypeViewModel viewModel)
        {
            return DeleteEntity(() => DynamicContentTypeManager.Instance.GetContentType(viewModel.ContentTypeId, PortalSettings.PortalId, true),
                                contentType => DynamicContentTypeManager.Instance.DeleteContentType(contentType));
        }

        /// <summary>
        /// GetContentField retrieves a single Content Field
        /// </summary>
        /// <param name="contentTypeId">The id of the Content Type</param>
        /// <param name="contentFieldId">The id of the Content Field</param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetContentField(int contentTypeId, int contentFieldId)
        {
            return GetEntity(() => FieldDefinitionManager.Instance.GetFieldDefinitions(contentTypeId).SingleOrDefault((c) => c.FieldDefinitionId == contentFieldId),
                           contentField => new ContentFieldViewModel(contentField, PortalSettings));
        }

        /// <summary>
        /// GetContentFields retrieves a page of Content Fields
        /// </summary>
        /// <param name="contentTypeId">The id of the ContentType</param>
        /// <param name="pageIndex">The page to fetch</param>
        /// <param name="pageSize">The size of page to fetch</param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetContentFields(int contentTypeId, int pageIndex = 0, int pageSize = 999)
        {
            return GetPage<FieldDefinition, ContentFieldViewModel>(
                                () =>
                                    {
                                        var contentType = DynamicContentTypeManager.Instance.GetContentType(contentTypeId, PortalSettings.PortalId, true);
                                        return new PagedList<FieldDefinition>(contentType.FieldDefinitions, pageIndex, pageSize);
                                    },
                                contentField => new ContentFieldViewModel(contentField, PortalSettings));
        }

        /// <summary>
        /// GetContentType retrieves a single ContentType
        /// </summary>
        /// <param name="contentTypeId">The id of the ContentType</param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetContentType(int contentTypeId)
        {
            return GetEntity(() => DynamicContentTypeManager.Instance.GetContentType(contentTypeId, PortalSettings.PortalId, true),
                           contentType => new ContentTypeViewModel(contentType, PortalSettings, true));
        }

        /// <summary>
        /// GetContentTypes retrieves a page of ContentTypes that satisfy the searchTerm
        /// </summary>
        /// <param name="searchTerm">The text to use to search for ContentTypes</param>
        /// <param name="pageIndex">The page to return, where 0 is the first page</param>
        /// <param name="pageSize">The size of the page</param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetContentTypes(string searchTerm, int pageIndex, int pageSize)
        {
            return GetPage(() => DynamicContentTypeManager.Instance.GetContentTypes(searchTerm, PortalSettings.PortalId, pageIndex, pageSize, true),
                            contentType => new ContentTypeViewModel(contentType, PortalSettings));

        }

        /// <summary>
        /// SaveContentField saves the content field
        /// </summary>
        /// <param name="viewModel">The ViewModel for the Content Field to save</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SaveContentField(ContentFieldViewModel viewModel)
        {
            DynamicContentType contentType = DynamicContentTypeManager.Instance.GetContentType(viewModel.ContentTypeId, PortalSettings.PortalId, true);
            var portalId = contentType.PortalId;
            var contentFieldId = viewModel.ContentFieldId;

            var localizedNames = new List<ContentTypeLocalization>();
            string defaultName = ParseLocalizations(viewModel.LocalizedNames, localizedNames, portalId);

            var localizedLabels = new List<ContentTypeLocalization>();
            string defaultLabel = ParseLocalizations(viewModel.LocalizedLabels, localizedLabels, portalId);

            var localizedDescriptions = new List<ContentTypeLocalization>();
            string defaultDescription = ParseLocalizations(viewModel.LocalizedDescriptions, localizedDescriptions, portalId);

            return SaveEntity(contentFieldId, 
                                () => new FieldDefinition
                                                {
                                                    ContentTypeId = contentType.ContentTypeId,
                                                    DataTypeId = viewModel.DataTypeId,
                                                    Label = defaultLabel,
                                                    Name = defaultName,
                                                    Description = defaultDescription,
                                                    PortalId = portalId
                                                },

                                contentField => FieldDefinitionManager.Instance.AddFieldDefinition(contentField),

                                () => FieldDefinitionManager.Instance.GetFieldDefinition(viewModel.ContentFieldId, viewModel.ContentTypeId),

                                contentField =>
                                                {
                                                    contentField.Name = defaultName;
                                                    contentField.Description = defaultDescription;
                                                    contentField.Label = defaultLabel;
                                                    contentField.DataTypeId = viewModel.DataTypeId;
                                                    FieldDefinitionManager.Instance.UpdateFieldDefinition(contentField);
                                                },

                                (id) =>
                                                {
                                                    SaveContentLocalizations(localizedNames, FieldDefinitionManager.NameKey, id, portalId);
                                                    SaveContentLocalizations(localizedLabels, FieldDefinitionManager.LabelKey, id, portalId);
                                                    SaveContentLocalizations(localizedDescriptions, FieldDefinitionManager.DescriptionKey, id, portalId);
                                                });
                }

        /// <summary>
        /// SaveContentType saves the content type
        /// </summary>
        /// <param name="viewModel">The ViewModel for the DataType to save</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SaveContentType(ContentTypeViewModel viewModel)
        {
            var contentTypeId = viewModel.ContentTypeId;
            var portalId = viewModel.IsSystem ? -1 : PortalSettings.PortalId;

            var localizedNames = new List<ContentTypeLocalization>();
            string defaultName = ParseLocalizations(viewModel.LocalizedNames, localizedNames, portalId);

            var localizedDescriptions = new List<ContentTypeLocalization>();
            string defaultDescription = ParseLocalizations(viewModel.LocalizedDescriptions, localizedDescriptions, portalId);

            //Check if the content Type exists
            HttpResponseMessage response;
            var type = DynamicContentTypeManager.Instance.GetContentTypes(portalId, true).SingleOrDefault((t => t.Name == defaultName));
            if (type != null)
            {
                response = Request.CreateResponse(new { success = false, message = LocalizeString("ContentTypeExists") });
            }
            else
            {
                response = SaveEntity(contentTypeId,
                                    () => new DynamicContentType
                                    {
                                        Name = defaultName,
                                        Description = defaultDescription,
                                        PortalId = portalId
                                    },

                                    contentType => DynamicContentTypeManager.Instance.AddContentType(contentType),

                                    () => DynamicContentTypeManager.Instance.GetContentType(contentTypeId, PortalSettings.PortalId, true),

                                    contentType =>
                                    {
                                        contentType.Name = defaultName;
                                        contentType.Description = defaultDescription;
                                        DynamicContentTypeManager.Instance.UpdateContentType(contentType);
                                    },

                                    id =>
                                    {
                                        SaveContentLocalizations(localizedNames, DynamicContentTypeManager.NameKey, id, portalId);
                                        SaveContentLocalizations(localizedDescriptions, DynamicContentTypeManager.DescriptionKey, id, portalId);
                                    });

            }

            return response;
        }
    }
}
